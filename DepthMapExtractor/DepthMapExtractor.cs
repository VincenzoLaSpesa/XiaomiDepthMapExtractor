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
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('c', "confidence_map", Required = false, HelpText = "Extract the confidence map")]
        public bool ConfidenceMap { get; set; }

        [Option('C', "confidence_map_raw", Required = false, HelpText = "Extract the confidence map")]
        public bool ConfidenceMapRaw { get; set; }

        [Option('d', "depthmap", Required = false, HelpText = "Extracts the depthmap as a png", Default = true)]
        public bool Depthmap { get; set; } = true;

        [Option('D', "depthmap_raw", Required = false, HelpText = "Extract the raw depthmap")]
        public bool DepthmapRaw { get; set; }

        [Option('s', "sub_images", Required = false, HelpText = "Extracts all the subimages instead of just the first one")]
        public bool SubImages { get; set; }

        [Option('i', "input", Required = true, HelpText = "Input file")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file")]
        public string OutputFile { get; set; }
    }

    internal class Logger : IDisposable
    {
        readonly StreamWriter logfile;
        public Logger(string outputFile = null)
        {
            if (outputFile == null || outputFile.Length == 0)
                return;
            logfile = new StreamWriter(File.Open(outputFile, FileMode.Append));
            logfile.WriteLine($"{DateTime.Now} Starting session");
            logfile.AutoFlush = true;
        }

        ~Logger()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (logfile == null)
                return;
            logfile.WriteLine($"{DateTime.Now} Stopping session");
            logfile.Close();
        }

        public void Log(string text)
        {
            Console.WriteLine(text);
            logfile?.WriteLine(text);
        }
    }

    internal struct ImageData
    {
        public int H { get; set; }
        public int W { get; set; }
        public RotateFlipType RotateFlip { get; set; }
    }

    /// <summary>
    /// Wraps all the used streams, making all of them disposable
    /// </summary>
    class Streams : IDisposable
    {
        public BinaryWriter ConfidenceMap;
        public BinaryWriter ConfidenceMapRaw;
        public BinaryWriter DeapthMapRaw;
        public BinaryReader DeapthMapSector;

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
                DeapthMapRaw = new BinaryWriter(File.Open(basename + "_DepthMap.raw", FileMode.Create));
        }

        public void Dispose()
        {
            DeapthMapRaw?.Dispose();
            ConfidenceMap?.Dispose();
            DeapthMapRaw?.Dispose();
            DeapthMapSector?.Dispose();
        }
    }

    static class Constants
    {
        public static readonly byte[] EOI = new byte[] { 0xFF, 0xD9 };// Jpeg marker for end of image
        public static readonly byte[] JPEG = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 };// Jpeg header 
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
            logger = new Logger(setup.OutputFile + ".log");
        }

        public void Process()
        {
            SeparateToFiles(options.InputFile);
            imageMetadata = ExtractJpegMetadata(options.InputFile);
            logger.Log("The main image has this shape: " + JsonSerializer.Serialize(imageMetadata));
            ExtractDepthMap();
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
            metadata.H = img.Height;
            metadata.W = img.Width;

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
                logger.Log("The file does not look like it came from a Xiaomi");
            }
            logger.Log("Extracting xml from " + filename);
            const string token = "MiCamera:XMPMeta=";
            binaryReader.BaseStream.Seek(0x3A7, SeekOrigin.Begin);
            string blob = Encoding.UTF8.GetString(binaryReader.ReadBytes(4096));
            var blobs = blob.Split(token);
            if (blobs.Length < 2)
                return default;
            blob = blobs[1];
            blob = blob.Split("/>")[0];
            blob = System.Web.HttpUtility.HtmlDecode(blob);
            string pattern = @"([a-zA-Z]+)=['""](.*?)['""]";
            RegexOptions options = RegexOptions.Multiline;

            Dictionary<String, String> metadata = new Dictionary<string, string>();
            foreach (Match m in Regex.Matches(blob, pattern, options))
                metadata.TryAdd(m.Groups[1].Value, m.Groups[2].Value);

            foreach (var kv in metadata)
                logger.Log($"{kv.Key} -> {kv.Value}");

            return metadata;
        }

        private void ExtractDepthMap()
        {
            XiaomiDepthmap xi;

            try
            {
                var xs = new KaitaiStream(streams.DeapthMapSector.BaseStream);
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
                streams.ConfidenceMapRaw?.Write(s.Filler);
                streams.ConfidenceMapRaw?.Write(s.Data0);
                streams.ConfidenceMap?.Write(s.Data0);
            }
            if (options.ConfidenceMapRaw)
                logger.Log($"Confidence map full size is {streams.ConfidenceMapRaw.BaseStream.Position}");
            if (options.ConfidenceMapRaw)
                logger.Log($"Confidence map payload size is {streams.ConfidenceMap.BaseStream.Position}");

            logger.Log($"Depthmap size is {xi.Depthmap.Length}");
            uint w = xi.ImageInfo2.SizeW;
            long h = xi.Depthmap.Length / xi.ImageInfo2.SizeW;
            long p = xi.Depthmap.Length - w * h;

            logger.Log($"Depthmap width is {w}, height should be {h}, padding should be {p}");
            streams.DeapthMapRaw?.Write(xi.Depthmap);
            using Bitmap depthUnscaled = new Bitmap((int)w - Constants.NPaddingPixels, (int)h);
            for (long i = p; i < xi.Depthmap.Length; i++)
            {
                long j = i - p;
                int x = (int)(j % w);
                int y = (int)(h - 1 - j / w); // depthmap is flipped
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

            depthUnscaled.RotateFlip(imageMetadata.RotateFlip); // maybe image is flipped too
            using Bitmap depthScaled = new Bitmap(imageMetadata.H, imageMetadata.W);
            var canvas = System.Drawing.Graphics.FromImage(depthScaled);
            float zoom = (float)depthScaled.Height / depthUnscaled.Height;
            canvas.DrawImage(depthUnscaled, depthScaled.Width - depthUnscaled.Width * zoom, 0, depthUnscaled.Width * zoom, depthUnscaled.Height * zoom);

            // Mirror the left side of the image and use it as a background
            depthUnscaled.RotateFlip(RotateFlipType.RotateNoneFlipX);
            int xx = (int)(depthScaled.Width - depthUnscaled.Width * zoom);
            var sourcePosition = new Rectangle(xx, 0, depthUnscaled.Width-xx-1, depthUnscaled.Height);
            var destPosition = new Rectangle(0, 0, xx+1, (int)(depthUnscaled.Height * zoom));
            
            canvas.DrawImage(depthUnscaled, destPosition, sourcePosition, GraphicsUnit.Pixel);

            depthScaled.Save(options.OutputFile + "_depth.png", ImageFormat.Png);
        }

        private List<String> SeparateToFiles(string filename)
        {
            long length = new System.IO.FileInfo(filename).Length;
            double lengthMB = length / (1024.0 * 1024);
            logger.Log("Extracting all sub files from " + filename);
            logger.Log($"Total lenght: {length} ({lengthMB:F})");
            var metadata = ExtractXmlMetadata(filename);
            byte[] wholeFile = File.ReadAllBytes(filename);
            int position = 0;
            List<int> cutpoints = new List<int> { 0 };
            while (position < length)
            {
                position = Array.IndexOf(wholeFile, Constants.EOI[1], ++position);
                if (position < 0)
                    break;
                if (wholeFile[position - 1] == Constants.EOI[0])
                {
                    logger.Log($"Found a chunk at {position}, starts from {cutpoints.Last()}, the size is {position - cutpoints.Last()}");
                    cutpoints.Add(position + 1);
                }
            }

            long x = length - cutpoints.Last();
            if (x == 0)
            {
                logger.Log($"There is no depthmap at the end of the file");
                return default;
            }

            logger.Log($"Found a chunk at {length}, starts from {cutpoints.Last()}, the size is {x}");
            cutpoints.Add((int)length);

            List<String> flist = new List<String>();
            for (int i = 1; i < cutpoints.Count; i++)
            {
                string chunkname = options.OutputFile;
                var magic = wholeFile[cutpoints[i - 1]..(cutpoints[i - 1] + Constants.JPEG.Length)];
                if (magic.SequenceEqual(Constants.JPEG))
                    chunkname += $"{i}.jpg";
                else
                    chunkname += $"{i}.raw";

                if (i < 2 || options.SubImages)
                {
                    logger.Log($"Writing {chunkname}");
                    using (BinaryWriter chunk = new BinaryWriter(File.Open(chunkname, FileMode.CreateNew)))
                        chunk.Write(wholeFile[cutpoints[i - 1]..cutpoints[i]]);
                    flist.Add(chunkname);
                }
                if (i + 1 == cutpoints.Count) 
                { // load the depthmap sector as a file if saved or as a memory stream if not
                    if (options.SubImages)
                        streams.DeapthMapSector = new BinaryReader(File.Open(chunkname, FileMode.Open));
                    else 
                        streams.DeapthMapSector = new BinaryReader(new MemoryStream(wholeFile[cutpoints[i - 1]..cutpoints[i]]));
                }

            }
            if (metadata.TryGetValue("depthlength", out string rawValue) && long.TryParse(rawValue, out long longValue))
            {
                long extractedLenght = cutpoints[^1] - cutpoints[^2];
                if (extractedLenght == longValue)
                    logger.Log($"The extracted depthmap matches the xml metadata value {longValue}");
                else
                    logger.Log($"The extracted depthmap NOES NOT match the xml metadata value.\n Metadata is {longValue}, extracted depthmap is {extractedLenght}");
            }
            else
                logger.Log("XML metadata are malformed");
            return flist;
        }

        public void Dispose()
        {
            logger.Dispose();
            streams.Dispose();
        }
    }
}
