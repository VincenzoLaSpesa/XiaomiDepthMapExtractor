using CommandLine;
using Kaitai;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DepthMapExtractor
{
    internal struct ImageData
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public RotateFlipType RotateFlip { get; set; }

        public float Ratio { get { return (float)Width / Height; } }
    }

    /// <summary>
    /// Wraps all the used streams, making all of them disposable
    /// </summary>
    class Streams : IDisposable
    {
        public BinaryWriter ConfidenceMap;
        public BinaryWriter ConfidenceMapRaw;
        public BinaryWriter DepthMapRaw;
        public BinaryReader DepthMapSector = null;

        public Streams(string basename, bool confidenceMap, bool confidenceMapRaw, bool deapthMapRaw)
            => Init(basename, confidenceMap, confidenceMapRaw, deapthMapRaw);

        public Streams(Options setup)
        {
            if (setup.OutputFile == null)
                setup.OutputFile = Path.GetFileNameWithoutExtension(setup.InputFile);
            Init(setup.OutputFile, setup.ConfidenceMap, setup.ConfidenceMapRaw, setup.DepthmapRaw);
        }

        private void Init(string basename, bool confidenceMap, bool confidenceMapRaw, bool deapthMapRaw)
        {
            if (confidenceMapRaw)
                ConfidenceMapRaw = new BinaryWriter(File.Open(basename + "_ConfidenceMapFull.raw", FileMode.Create));
            if (confidenceMap)
                ConfidenceMap = new BinaryWriter(File.Open(basename + "_ConfidenceMapStripped.raw", FileMode.Create));
            if (deapthMapRaw)
                DepthMapRaw = new BinaryWriter(File.Open(basename + "_DepthMap.raw", FileMode.Create));
        }

        public void Dispose()
        {
            DepthMapRaw?.Dispose();
            ConfidenceMap?.Dispose();
            DepthMapRaw?.Dispose();
            DepthMapSector?.Dispose();
        }
    }

    static class Constants
    {
        public static readonly byte[] EOI = new byte[] { 0xFF, 0xD9 };// Jpeg marker for end of image
        public static readonly byte[] SOF0 = new byte[] { 0xFF, 0xC0 };// Start of frame 0
        public static readonly byte[] JPEG = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 };// Jpeg header 
        public static readonly byte[] DMP1 = new byte[] { 80, 77, 80, 68 };// Dmap header PMPD
        public static readonly byte[] DMP2 = new byte[] { 0x10, 0, 0xFF, 0xFF };// Other dmap
        public const short ExifOrientationID = 0x112; //274, the exif tag used for orientation
        public const short NPaddingPixels = 4; //number of padding pixels at the end of every row
    }

    public class DepthMapExtractor : IDisposable
    {
        private Logger logger;
        private ImageData imageMetadata;
        private Options options;
        private Streams streams;

        /// <summary>
        /// Maps the exif values to the dotnetcore values
        /// </summary>
        private static readonly RotateFlipType[] lookupRotateFlip = new RotateFlipType[]
        {
            RotateFlipType.RotateNoneFlipNone,
            RotateFlipType.RotateNoneFlipNone,
            RotateFlipType.RotateNoneFlipX,
            RotateFlipType.Rotate180FlipNone,
            RotateFlipType.Rotate180FlipX,
            RotateFlipType.Rotate90FlipX,
            RotateFlipType.Rotate90FlipNone,
            RotateFlipType.Rotate270FlipX,
            RotateFlipType.Rotate270FlipNone
        };

        public DepthMapExtractor(string filename)
        {
            options = new Options
            {
                InputFile = filename
            };
            Init(options);
        }

        public DepthMapExtractor(Options setup) => Init(setup);

        private void Init(Options setup)
        {
            options = setup;
            streams = new Streams(setup);
            if (options.Log)
                logger = new Logger(setup.OutputFile + ".log");
            else
                logger = new Logger();
        }

        public void Process()
        {
            if (File.Exists(options.InputFile))
            {
                SeparateToFiles(options.InputFile);
                imageMetadata = ExtractJpegMetadata(options.InputFile);
                logger.Log("The main image has this shape: " + JsonSerializer.Serialize(imageMetadata));
                ExtractDepthMap();
                logger.Log("Extraction completed");
            }
            else 
            {
                logger.Log("Can't open the file " + options.InputFile);
            }
        }

        /// <summary>
        /// Gets the size and the rotation of a proper jpeg image ( with exif metadata )
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal static ImageData ExtractJpegMetadata(string filename)
        {
            ImageData metadata = new ImageData();
            Image img = Image.FromFile(filename);
            metadata.Height = img.Height;
            metadata.Width = img.Width;

            // Read orientation tag
            metadata.RotateFlip = RotateFlipType.RotateNoneFlipNone;
            var prop = img.GetPropertyItem(Constants.ExifOrientationID);
            if (prop != null)
            {
                short rotation = BitConverter.ToInt16(prop.Value, 0);
                if (rotation < lookupRotateFlip.Length)
                    metadata.RotateFlip = lookupRotateFlip[rotation];
            }
            return metadata;
        }

        /// <summary>
        /// Estract the XML metadata specific to MiCamera and exports it as a string->string map
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private Dictionary<String, String> ExtractXmlMetadata(string filename)
        {
            using BinaryReader binaryReader = new BinaryReader(File.OpenRead(filename));
            var magic = binaryReader.ReadBytes(4);
            if (!magic.SequenceEqual(Constants.JPEG))
            {
                logger.Log("The file does not look like it came from a Xiaomi, or the metadata are in a different format");
                logger.Log("The extraction will probably fail.");
            }
            logger.Log("Extracting xml from " + filename);
            const string token = "MiCamera:XMPMeta=";
            binaryReader.BaseStream.Seek(0x3A7, SeekOrigin.Begin);
            KMPSearch finder = new KMPSearch(token);
            if (finder.FindNext(binaryReader) < 0) // it moves the stream
                return default;
            string blob = Encoding.UTF8.GetString(binaryReader.ReadBytes(1024));
            blob = blob.Split("/>")[0];
            blob = System.Web.HttpUtility.HtmlDecode(blob);
            string pattern = @"([a-zA-Z]+)=['""](.*?)['""]";
            RegexOptions options = RegexOptions.Multiline;

            Dictionary<String, String> metadata = new Dictionary<string, string>();
            foreach (Match m in Regex.Matches(blob, pattern, options))
                metadata.TryAdd(m.Groups[1].Value, m.Groups[2].Value);

            foreach (var kv in metadata)
                logger.Log($"{kv.Key} -> {kv.Value}");

            if(!metadata.ContainsKey("depthlength"))
                logger.Log("The depth lenght is missing in the metadata");
            return metadata;
        }

        private void ExtractDepthMap()
        {
            XiaomiDepthmap xi;

            try
            {
                var xs = new KaitaiStream(streams.DepthMapSector.BaseStream);
                xi = new XiaomiDepthmap(xs);
                logger.Log("It looks like the stream contains a Xiaomi depth map");
            }
            catch (Exception e)
            {
                logger.Log("It looks like the stream DOES NOT contain a Xiaomi depth map\n" + e.Message);
                throw;
            }


            foreach (XiaomiDepthmap.Sector s in xi.ConfidenceMap)
            {
                streams.ConfidenceMapRaw?.Write(s.Padding);
                streams.ConfidenceMapRaw?.Write(s.Data);
                streams.ConfidenceMap?.Write(s.Data);
            }
            if (options.ConfidenceMapRaw)
                logger.Log($"Confidence map full size is {streams.ConfidenceMapRaw.BaseStream.Position}");
            if (options.ConfidenceMapRaw)
                logger.Log($"Confidence map payload size is {streams.ConfidenceMap.BaseStream.Position}");

            logger.Log($"Depthmap size is {xi.Depthmap.Length}");

            long width, height;
            width = xi.DepthmapInfo.ImageWidth;
            height = xi.Depthmap.Length / width;

            // padding before the data
            long padding = xi.Depthmap.Length - width * height;
            
            // the size the map will have after the padding
            long padded_lenght = xi.DepthmapInfo.ImageWidth * xi.DepthmapInfo.ImageHeightPadded;

            logger.Log($"Depthmap valid size is  {width}x{height} as WxH, ratio is {(float)width / height}, padding should be {padding}");
            logger.Log($"Depthmap declared size is {xi.DepthmapInfo.ImageWidth}x{xi.DepthmapInfo.ImageHeightPadded} --> {(float)xi.DepthmapInfo.ImageWidth / xi.DepthmapInfo.ImageHeightPadded} ");
            if(padded_lenght > xi.Depthmap.Length)
                logger.Log($"Declared size would not fit into the file");

            streams.DepthMapRaw?.Write(xi.Depthmap);
            using Bitmap depthUnscaled = new Bitmap((int)width - Constants.NPaddingPixels, (int)height);
            for (long i = padding; i < xi.Depthmap.Length; i++)
            {
                long j = i - padding;
                int x = (int)(j % width);
                int y = (int)(height - 1 - j / width); // depthmap is flipped
                int gvalue = 255 - xi.Depthmap[i]; // black is the farest point
                Color c = Color.FromArgb(gvalue, gvalue, gvalue);
                if (x < depthUnscaled.Width)
                    depthUnscaled.SetPixel(x, y, c);
            }
            if (!options.Depthmap)
            {
                logger.Log("The program will terminate withouth generating the png depth map.");
                return;
            }

            using Canvas unscaledCanvas = new Canvas((int)xi.DepthmapInfo.ImageWidth , (int)xi.DepthmapInfo.ImageHeightPadded);
            //unscaledCanvas.FillWithColor(0,128,0);
            unscaledCanvas.Paste(depthUnscaled, Canvas.Corner.Se, true);

            using Canvas outputCanvas = new Canvas(imageMetadata.Width, imageMetadata.Height);
            outputCanvas.Graphics.DrawImage(unscaledCanvas.Bitmap, 0, 0, outputCanvas.Bitmap.Width, outputCanvas.Bitmap.Height);

            if (xi.DepthmapInfo.IsLandscape == 0)
                outputCanvas.Bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            outputCanvas.Save(options.OutputFile + "_depth.png");
        }

        /// <summary>
        /// Extracts all the subimages from the file.
        /// (The implementation could be simplified assuming that the depthmap is always the last file)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private List<String> SeparateToFiles(string filename)
        {
            long length = new System.IO.FileInfo(filename).Length;
            double lengthMB = length / (1024.0 * 1024);
            logger.Log("Extracting all sub files from " + filename);
            logger.Log($"Total lenght: {length} ({lengthMB:F})");
            var metadata = ExtractXmlMetadata(filename);

            const short JPEG = 0;
            const short DMAP = 1;
            const short UNKN = 2;
            // [ [begin, lenght, kind] ... ]
            List<Tuple<int, int, short>> cutsInfo = new List<Tuple<int, int, short>>();

            long declaredLenght = -1;
            if (metadata.TryGetValue("depthlength", out string rawValue))
                long.TryParse(rawValue, out declaredLenght);

            // BinaryReader directly from a file is terribly slow and i don't want to write a buffered version
            using BinaryReader binaryReader =  new BinaryReader(new MemoryStream(File.ReadAllBytes(filename)));

            KMPSearch finder = new KMPSearch(Constants.EOI);
            long from = 0;
            long to = 0;
            while (from < length) 
            {
                binaryReader.BaseStream.Seek(from, SeekOrigin.Begin);
                byte[] magic = binaryReader.ReadBytes(4);
                to = finder.FindNext(binaryReader);
                
                short kind = UNKN;

                if (magic.SequenceEqual(Constants.JPEG))
                    kind = JPEG;
                else
                {
                    var hdump = Helper.HexDump(magic,false,false);
                    logger.Log($"Found a depthmap that starts with "+hdump);

                    if (magic.SequenceEqual(Constants.DMP1))
                    {// Depthmap has no delimiters, so i will just jump forward using the declared lenght
                        kind = DMAP;
                        if (declaredLenght > 0)
                        {
                            to = from + declaredLenght + 1;
                            if (to > length)
                                logger.Log($"The depthmap is shorter than it's supposed to be");
                        }
                        else
                        {
                            logger.Log($"XML metadata are malformed. Lenght is not declared in the metadata");
                        }
                    }
                    else if (magic.SequenceEqual(Constants.DMP2))
                    {
                        logger.Log($"This kind of depthmap is not yet supported");
                        var delta = length - from;
                        if (delta < declaredLenght)
                            logger.Log($"There are {length - to} bytes left on the file, {declaredLenght} are needed. The dephmap is compressed");
                        to = from + declaredLenght + 1;
                    }
                }

                if (to < 0)
                    to = length;

                cutsInfo.Add(new Tuple<int, int, short>((int)from, (int)(to - 1 - from), kind));
                from = to;
            }

            List<String> flist = new List<String>();
            for (int i = 0; i < cutsInfo.Count; i++) 
            {
                logger.Log($"Found a chunk starting at {cutsInfo[i].Item1}, the size is {cutsInfo[i].Item2}, kind is {cutsInfo[i].Item3}");

                if (i == 0 || options.SubImages) 
                {
                    string chunkname = options.OutputFile;
                    if (cutsInfo[i].Item3 == JPEG)
                        chunkname += $"{i}.jpg";
                    else
                        chunkname += $"{i}.raw";
                    
                    logger.Log($"Writing {chunkname}");
                    binaryReader.BaseStream.Seek(cutsInfo[i].Item1, SeekOrigin.Begin);
                    using (BinaryWriter chunk = new BinaryWriter(File.Open(chunkname, options.FileWritingMode)))
                        chunk.Write(binaryReader.ReadBytes(cutsInfo[i].Item2));
                    flist.Add(chunkname);
                }
                
                if (cutsInfo[i].Item3 == DMAP)
                {
                        binaryReader.BaseStream.Seek(cutsInfo[i].Item1, SeekOrigin.Begin);
                        streams.DepthMapSector = new BinaryReader(new MemoryStream(binaryReader.ReadBytes(cutsInfo[i].Item2)));
                }
            }
            if (streams.DepthMapSector == null)
                logger.Log($"There is not depthmap in the file");
            
            return flist;
        }

        public void Dispose()
        {
            logger.Dispose();
            streams.Dispose();
        }
    }
}
