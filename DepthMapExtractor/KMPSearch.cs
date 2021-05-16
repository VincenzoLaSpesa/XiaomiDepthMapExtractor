using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DepthMapExtractor
{
    /// <summary>
    /// An implementation of the Knuth-Morris-Pratt algorithm
    /// It finds strings inside streams, reasonably fast.
    /// 
    /// The class holds the target string, the BinaryReader where to search in is provided on every call.
    /// </summary>
    public class KMPSearch
    {
        public KMPSearch(byte[] pattern)
        {
            BuildBacktrace(pattern);
        }

        public KMPSearch(string token)
        {
            BuildBacktrace(Encoding.ASCII.GetBytes(token));
        }

        private void BuildBacktrace(byte[] pattern)
        {
            _backtrace = new int[pattern.Length];
            _pattern = pattern;
            
            for (int i = 1; i < pattern.Length; i++)
            {
                int j = _backtrace[i - 1];
                while (j > 0 && pattern[i] != pattern[j])
                    j = _backtrace[j - 1];
                if (pattern[i] == pattern[j])
                    j++;
                _backtrace[i] = j;
            }
        }

        /// <summary>
        /// Finds teh next match
        /// </summary>
        /// <param name="reader">A valid open binary stream (it will be moved by the function and not rewind back to starting position)</param>
        /// <param name="includeDelimiter">
        ///     if TRUE the found string will start with the delimiter.
        ///     if FALSE the found string will start right after the delimiter.
        /// </param>
        /// <returns>The position of the next match, -1 if there are no more matches</returns>
        public long FindNext(BinaryReader reader, bool includeDelimiter)
        {
            if (reader.EOF())
                return -1;
            
            int goal = _backtrace.Length;
            int stato = 0;
            byte x = reader.ReadByte();
            while (stato != goal && !reader.EOF())
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
                if(includeDelimiter)
                    return reader.BaseStream.Position - goal - 1;
                else
                    return reader.BaseStream.Position - 1;
                            
            
            return -1;
        }

        public List<long> FindAll(BinaryReader reader, bool includeDelimiter) 
        {
            List<long> positions = new List<long>();
            while(!reader.EOF())
            {
                var p = FindNext(reader, includeDelimiter);
                if (p > -1)
                    positions.Add(p);
            }
            return positions;
        }

        private int[] _backtrace;
        private byte[] _pattern;
    }
}
