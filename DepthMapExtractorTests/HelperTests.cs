using Microsoft.VisualStudio.TestTools.UnitTesting;
using DepthMapExtractor;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DepthMapExtractor.Tests
{
    [TestClass()]
    public class HelperTests
    {
        private BinaryReader data;
        [TestMethod()]
        public void SequenceStartsWithTest()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(Constants.EOI);
            bw.Write(Constants.DMP1);
            bw.Write(Constants.DMP2);
            bw.Write("Bla bla bla");

            data = new BinaryReader(ms);

            byte[] goodSequence = data.ReadBytes((int)ms.Length);
            byte[] badSequence = Encoding.ASCII.GetBytes("ammaccabanane");

            byte[][] patterns = { Constants.EOI, Constants.DMP1, Constants.DMP2 };

            Assert.IsTrue(goodSequence.SequenceStartsWith(patterns));
            Assert.IsFalse(badSequence.SequenceStartsWith(patterns));


        }
    }
}