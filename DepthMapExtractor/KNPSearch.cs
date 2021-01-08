using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DepthMapExtractor
{
    /// <summary>
    /// An implementation of the Knuth-Morris-Pratt algorithm
    /// It finds strings inside streams, reasonably fast.
    /// </summary>
    class KNPSearch
    {
        public KNPSearch(char[] pattern)
        {
            BuildBacktrace(pattern);
        }

        private void BuildBacktrace(char[] pattern)
        {
            _pattern = pattern;
            _backtrace = new int[pattern.Length];
            int a, b;
            bool flag;
            for (int c = 1; c < pattern.Length; c++)
            {
                _backtrace[c] = 0;
                for (a = 1; a < c; a++)
                {
                    flag = true;
                    for (b = 0; b < a; b++) 
                        flag &= (pattern[c + b - a] == pattern[b]);
                    if (flag) 
                        _backtrace[c] = a;
                }
            }
        }

        public long FindNext(BinaryReader reader)
        {
            int goal = _backtrace.Length;
            long fine = reader.BaseStream.Length - reader.BaseStream.Position;
            int stato = 0;
            byte x = reader.ReadByte();
            while (stato != goal && reader.BaseStream.Position < fine)
            {
                if (_pattern[stato] == x)
                {
                    stato++;
                    x = reader.ReadByte();
                }
                else
                {
                    if (stato == 0)
                        x = reader.ReadByte();
                    stato = _backtrace[stato];
                }
            }
            if (stato == goal) 
                return reader.BaseStream.Position - goal + 1;
            return -1;
        }

        private int[] _backtrace;
        private char[] _pattern;
    }
}
