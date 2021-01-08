using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace DepthMapExtractor
{
    /// <summary>
    /// Wraps a Graphics and its Bitmap in a single, disposable container.
    /// Exposes some simplified drawing methods
    /// </summary>    
    public class Canvas : IDisposable
    {
        public enum Corner { Nw, Ne, Se, Sw }
        public Graphics Graphics { get; private set; }
        public Bitmap Bitmap { get; private set; }

        public int Height { get { return Bitmap.Height; } }

        public int Width { get { return Bitmap.Width; } }

        public Canvas(int width, int height, RotateFlipType rotateFlip = RotateFlipType.RotateNoneFlipNone)
        {
            this.Bitmap = new Bitmap(width, height);
            this.Bitmap.RotateFlip(rotateFlip);
            this.Graphics = System.Drawing.Graphics.FromImage(this.Bitmap);
        }

        /// <summary>
        /// Paste a bitmap starting from a corner, no scaling.
        /// </summary>
        /// <param name="b">the image</param>
        /// <param name="c">the corner</param>
        /// <param name="autoFill">if true it will cover the entire canvas surface with fake data</param>
        /// <returns></returns>
        public bool Paste(Image b, Corner c, bool autoFill = false) 
        {
            int deltaW = Bitmap.Width - b.Width;
            int deltaH = Bitmap.Height - b.Height;
            if (deltaH < 0 || deltaW < 0)
                return false;
            Point p = c switch
            {
                Corner.Nw => new Point(0, deltaH),
                Corner.Sw => new Point(0, 0),
                Corner.Se => new Point(deltaW, 0),
                Corner.Ne => new Point(deltaW, deltaH),
                _ => throw new NotImplementedException(),
            };

            if (autoFill) //@TODO This really sucks, i need to mirror the content and blur
                Graphics.DrawImage(b, 0, 0, Bitmap.Width, Bitmap.Height);
            Graphics.DrawImage(b, p);

            return true;
        }

        public void Save(string filename, ImageFormat format = null)
        {
            if (format == null)
                format = ImageFormat.Png;
            this.Bitmap.Save(filename, format);
        }

        public void FillWithColor(byte grayColor)
        {
            FillWithColor(grayColor, grayColor, grayColor);
        }

        public void FillWithColor(byte red, byte green, byte blue)
        {
            Brush b = new SolidBrush(Color.FromArgb(red, green, blue));
            Rectangle r = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
            Graphics.FillRectangle(b, r);
        }

        public void Dispose()
        {
            Graphics.Dispose();
            Bitmap.Dispose();
        }
    }
}
