using CommandLine;
using Kaitai;
using System;
using System.Diagnostics;
using System.IO;

namespace DepthMapExtractor
{
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Write output to stdout.", Default = true)]
        public bool Verbose { get; set; } = true;

        [Option('l', "log", Required = false, HelpText = "Write to a logfile too.")]
        public bool Log { get; set; }

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

        [Option("overwrite", HelpText = "Never overwrite existing files", Default = false)]
        public bool Overwrite { get; set; } = false;

        public FileMode FileWritingMode
        {
            get
            {
                if (Overwrite)
                    return FileMode.Create;
                return FileMode.CreateNew;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached && args.Length == 0)
            { // Debug mode
                Options o = new Options
                {
                    InputFile = "ImageP.jpg",
                    SubImages = true,
                    Overwrite = true
                };
                using DepthMapExtractor depthMapExtractor = new DepthMapExtractor(o);
                depthMapExtractor.Process();
            }
            else if (args.Length == 1)
            { // trivial mode, only input file provided
                using DepthMapExtractor depthMapExtractor = new DepthMapExtractor(args[0]);
                depthMapExtractor.Process();
            }
            else
            { // Custom mode, commandline options provided
                Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       using DepthMapExtractor depthMapExtractor = new DepthMapExtractor(o);
                       depthMapExtractor.Process();
                   });
            }
        }
    }
}
