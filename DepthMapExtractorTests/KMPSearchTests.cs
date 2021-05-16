using Microsoft.VisualStudio.TestTools.UnitTesting;
using DepthMapExtractor;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DepthMapExtractor.Tests
{
    [TestClass()]
    public class KMPSearchTests
    {
        private KMPSearch kMPSearch;
        private const string Token = "10110110";
        private readonly string[] Chunks = new string[]{ "First Chunk", "Second Chunk", "Third Chunk"};
        private readonly BinaryReader data;

        public KMPSearchTests() 
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            foreach (var chunk in Chunks) 
            {
                bw.Write(Encoding.ASCII.GetBytes(chunk));
                bw.Write(Encoding.ASCII.GetBytes(Token));
            }
            bw.Write(0x00);
            data = new BinaryReader(ms);
            
        }

        [TestMethod()]
        public void FindAllTest()
        {
            data.BaseStream.Seek(0, SeekOrigin.Begin);
            kMPSearch = new KMPSearch(Token);
            data.BaseStream.Position = 0;
            var positionsWithTokens = kMPSearch.FindAll(data, false);
            data.BaseStream.Position = 0;
            var positionsWithout = kMPSearch.FindAll(data, true);
            data.BaseStream.Position = 0;

            Assert.AreEqual(positionsWithTokens.Count, positionsWithout.Count);
            Assert.AreEqual(positionsWithTokens.Count, Chunks.Length);

            // every chunk should be a string and the token
            long offset = 0;
            for (int i = 0; i < positionsWithTokens.Count; i++) 
            {
                var buffer = data.ReadBytes((int)(positionsWithTokens[i]-offset));
                string chunk = System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                Assert.IsTrue(Chunks[i] + Token == chunk);
                offset = positionsWithTokens[i];
            }
            data.BaseStream.Position = 0;

            // every chunk should be just the string
            offset = 0;
            for (int i = 0; i < positionsWithout.Count; i++)
            {
                var buffer = data.ReadBytes((int)(positionsWithout[i] - offset));
                string chunk = System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                Assert.IsTrue(Chunks[i] == chunk);
                offset = positionsWithout[i] + Token.Length;
                // skip the token
                data.BaseStream.Seek(Token.Length, SeekOrigin.Current);
            }
        }
    }
}