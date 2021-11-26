using Microsoft.VisualStudio.TestTools.UnitTesting;
using DepthMapExtractor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace DepthMapExtractorTests
{
    [TestClass()]
    public class DepthMapExtractorTests
    {
        /// <summary>
        /// Try to download some test images
        /// </summary>
        private static List<DepthMapExtractor.Options> tests;
        [TestMethod()]
        public void DepthMapExtractorTest()
        {
            Options template = new Options
            {
                InputFile = "",
                SubImages = true,
                Overwrite = true,
                Log = true,
                Trailer = false
            };

            tests = new List<Options>();
            template.InputFile = "https://raw.githubusercontent.com/wiki/VincenzoLaSpesa/XiaomiDepthMapExtractor/data/Gatto.jpg";
            tests.Add(template.Clone());
            template.InputFile = "https://raw.githubusercontent.com/wiki/VincenzoLaSpesa/XiaomiDepthMapExtractor/data/Teapot.jpg";
            tests.Add(template.Clone());
            template.InputFile = "https://raw.githubusercontent.com/wiki/VincenzoLaSpesa/XiaomiDepthMapExtractor/data/Solaris.jpg";
            tests.Add(template.Clone());


            using var client = new WebClient();
            try
            {
                int id = 1;
                if (!Directory.Exists("../../tests"))
                    Directory.CreateDirectory("../../tests");

                File.WriteAllText("../../tests/readme.txt", "You can safely delete this folder\n");

                foreach (var t in tests)
                {
                    string outfile = $"../../tests/{id++}.jpg";
                    if (!File.Exists(outfile))
                    {
                        Console.WriteLine($"Downloading {t.InputFile} in {outfile}");
                        client.DownloadFile(t.InputFile, outfile);
                    }
                    t.InputFile = Path.GetFullPath(outfile);
                    t.OutputFile = Path.GetFullPath(outfile + "_");
                }
            }
            catch (Exception)
            {
                Assert.Fail("Unable to download test files");
            }
        }

        [TestMethod()]
        public void ProcessTest()
        {
            int id=0;
            try
            {
                foreach (var t in tests)
                {
                    DepthMapExtractor.DepthMapExtractor d= new DepthMapExtractor.DepthMapExtractor(t);
                    d.Process();
                    id++;
                }
            }
            catch (Exception e)
            {
                Assert.Fail($"Unable to extract data from {tests[id].InputFile}, {e.Message}");
            }
        }

    }
}