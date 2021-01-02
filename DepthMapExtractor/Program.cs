using CommandLine;
using Kaitai;
using System;
using System.Diagnostics;
using System.IO;

namespace DepthMapExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached && args.Length == 0)
            { // Debug mode
                using DepthMapExtractor depthMapExtractor = new DepthMapExtractor("Image.jpg");
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
