using System.Drawing;
using System.Windows.Forms;

namespace TheSharpTurn
{
    public partial class Form1
    {
        private void pictureBoxDisplay_Paint_MetroCity(object sender,
            PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(30, 30, 30));

            float scale = GetMapScale();
            if (scale <= 0)
                return;

            int drawWidth = (int)(LogicalMapWidth * scale);
            int drawHeight = (int)(LogicalMapHeight * scale);
            int offsetX = (pictureBoxDisplay.ClientSize.Width - drawWidth) / 2;
            int offsetY = (pictureBoxDisplay.ClientSize.Height - drawHeight) / 2;

            e.Graphics.TranslateTransform(offsetX, offsetY);
            e.Graphics.ScaleTransform(scale, scale);
            e.Graphics.SetClip(new Rectangle(0, 0,
                LogicalMapWidth, LogicalMapHeight));

            DrawMetroCityMap(e.Graphics);
            trafficObjects.DrawAll(e.Graphics);

            if (curIndex >= 0 && curIndex < trafficObjects.Count)
            {
                Rectangle selectedBounds = trafficObjects[curIndex].Bounds;
                selectedBounds.Inflate(3, 3);
                e.Graphics.DrawRectangle(Pens.Gold, selectedBounds);
            }
        }

        private void DrawMetroCityMap(Graphics g)
        {
            SpriteLibrary.Draw(g, "Environment/MetroCityMap.png",
                new Rectangle(0, 0, LogicalMapWidth, LogicalMapHeight),
                TravelDirection.Right);
        }
    }
}
