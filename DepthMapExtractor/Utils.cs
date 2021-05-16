using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DepthMapExtractor
{
    public class Logger : IDisposable
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
    public static class Helper
    {
        public static bool SequenceStartsWith(this byte[] stringa, byte[] pattern) 
        {
            if (stringa.Length > pattern.Length)
                return stringa[0..(pattern.Length)].SequenceEqual(pattern);
            return stringa.SequenceEqual(pattern);
        }

        public static bool SequenceStartsWith(this byte[] stringa, IEnumerable<byte[]> pattern)
        {
            int totallenght = 0;
            foreach (var chunk in pattern)
                totallenght += chunk.Length;

            int offset = 0;
            foreach (var chunk in pattern)
                if (stringa.Length - offset > chunk.Length)
                    if (!stringa[offset..(offset + chunk.Length)].SequenceEqual(chunk))
                        return false;
                else                   
                        return false;
            return true;
        }



        public static bool EOF(this BinaryReader binaryReader)
        {
            var bs = binaryReader.BaseStream;
            return (bs.Position == bs.Length);
        }
        public static string HexDump(byte[] data, bool writeAddress = true, bool writeStringDump = true)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < data.Length; i += 16)
            {
                if(writeAddress)
                    sb.AppendFormat("{0:X8}:", i);

                for (int j = i; j < i + 16; ++j)
                {
                    if (j % 2 == 0)
                        sb.Append(" ");

                    if (j < data.Length)
                        sb.AppendFormat("{0:X2}", data[j]);
                    else
                        sb.Append("  ");
                }

                if (writeStringDump) 
                {
                    sb.Append("\t");

                    for (int j = i; j < i + 16; ++j)
                    {
                        if (j < data.Length)
                        {
                            char c = (char)data[j];
                            if (c >= ' ' && c <= '~')
                                sb.Append(c);
                            else
                                sb.Append(".");
                        }
                        else
                            sb.Append(" ");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
