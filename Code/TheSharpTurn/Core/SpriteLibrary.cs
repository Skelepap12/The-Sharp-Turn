using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace TheSharpTurn
{
    public static class SpriteLibrary
    {
        static Hashtable images = new Hashtable();

        private static Image GetImage(string relativePath)
        {
            string path = relativePath.Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Sprites", path);

            if (images.ContainsKey(fullPath))
                return (Image)images[fullPath];

            if (!File.Exists(fullPath))
                return (Image)null;

            Image image = Image.FromFile(fullPath);
            images.Add(fullPath, image);
            return image;
        }

        public static bool Draw(Graphics g, string relativePath,
            Rectangle bounds, TravelDirection direction)
        {
            Image image = GetImage(relativePath);

            if (image == null)
                return false;

            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            if (direction == TravelDirection.Right)
            {
                g.DrawImage(image, bounds);
            }
            else
            {
                g.TranslateTransform(bounds.X + bounds.Width, bounds.Y);
                g.ScaleTransform(-1, 1);
                g.DrawImage(image, 0, 0, bounds.Width, bounds.Height);
            }

            g.Restore(state);
            return true;
        }
    }
}
