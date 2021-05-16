using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DepthMapExtractor
{
    public abstract class Constants
    {
        private static readonly byte[] soi = new byte[] { 0xFF, 0xD8 };// Jpeg marker for start of image
        private static readonly byte[] eoi = new byte[] { 0xFF, 0xD9 };// Jpeg marker for end of image
        private static readonly byte[] sof0 = new byte[] { 0xFF, 0xC0 };// Start of frame 0 (baseline DCT)
        private static readonly byte[] sof2 = new byte[] { 0xFF, 0xC2 };// Start of frame 0 (progressive DCT)
        private static readonly byte[] exif = new byte[] { 0xFF, 0xE1 };// Exif segment
        private static readonly byte[] dqt = new byte[] { 0xFF, 0xDB };// Exif segment
        private static readonly byte[] jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 };// Jpeg+exif header 
        private static readonly byte[] jpgc = new byte[] { 0xFF, 0xD8, 0xFF };// Custom jpeg header
        private static readonly byte[] dmp1 = new byte[] { 0x50, 0x4D, 0x50, 0x44 };// Dmap header PMPD
        private static readonly byte[] dmp2 = new byte[] { 0x10, 0x00, 0xFF, 0xFF };// Other dmap found in Redmi Note 9
        private static readonly byte[] dmp3 = new byte[] { 0xED, 0xFF, 0x00, 0x00 };// Other dmap found in Xiaomi Mi 10

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


        public const short ExifOrientationID = 0x112; //274, the exif tag used for orientation
        public const short NPaddingPixels = 4; //number of padding pixels at the end of every row

        #region wrappers
        public static byte[] SOI => soi;
        public static byte[] EOI => eoi;
        public static byte[] SOF0 => sof0;
        public static byte[] SOF2 => sof2;
        public static byte[] EXIF => exif;

        public static byte[] JPEG => jpeg;
        public static byte[] JPGC => jpgc;
        public static byte[] DMP1 => dmp1;
        public static byte[] DMP2 => dmp2;
        public static byte[] DMP3 => dmp3;
        public static byte[] DQT => dqt;
        public static RotateFlipType[] LookupRotateFlip => lookupRotateFlip;
        #endregion
    }
}
