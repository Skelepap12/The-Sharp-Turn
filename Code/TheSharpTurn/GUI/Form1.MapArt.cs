using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TheSharpTurn
{
    public partial class Form1
    {
        bool metroMapPaintInstalled = false;

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (metroMapPaintInstalled || pictureBoxDisplay == null)
                return;

            pictureBoxDisplay.Paint -= new PaintEventHandler(
                pictureBoxDisplay_Paint);
            pictureBoxDisplay.Paint += new PaintEventHandler(
                pictureBoxDisplay_Paint_MetroCity);
            metroMapPaintInstalled = true;
            pictureBoxDisplay.Invalidate();
        }

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
            GraphicsState state = g.Save();
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            Color grass = Color.FromArgb(74, 103, 67);
            Color grassDark = Color.FromArgb(59, 83, 55);
            Color sidewalk = Color.FromArgb(154, 151, 139);
            Color sidewalkDark = Color.FromArgb(105, 103, 96);
            Color sidewalkLight = Color.FromArgb(183, 179, 164);
            Color bike = Color.FromArgb(75, 111, 76);
            Color bikeDark = Color.FromArgb(48, 80, 52);
            Color road = Color.FromArgb(58, 61, 60);
            Color roadPatch = Color.FromArgb(63, 66, 65);
            Color laneLine = Color.FromArgb(219, 214, 190);
            Color median = Color.FromArgb(111, 106, 91);
            Color medianDark = Color.FromArgb(77, 73, 64);

            using (SolidBrush grassBrush = new SolidBrush(grass))
            using (SolidBrush grassDarkBrush = new SolidBrush(grassDark))
            using (SolidBrush sidewalkBrush = new SolidBrush(sidewalk))
            using (SolidBrush sidewalkDarkBrush = new SolidBrush(sidewalkDark))
            using (SolidBrush sidewalkLightBrush = new SolidBrush(sidewalkLight))
            using (SolidBrush bikeBrush = new SolidBrush(bike))
            using (SolidBrush bikeDarkBrush = new SolidBrush(bikeDark))
            using (SolidBrush roadBrush = new SolidBrush(road))
            using (SolidBrush roadPatchBrush = new SolidBrush(roadPatch))
            using (SolidBrush laneBrush = new SolidBrush(laneLine))
            using (SolidBrush medianBrush = new SolidBrush(median))
            using (SolidBrush medianDarkBrush = new SolidBrush(medianDark))
            {
                g.FillRectangle(grassBrush, 0, 0,
                    LogicalMapWidth, LogicalMapHeight);

                DrawMetroCityEdgeBuildings(g);

                DrawMetroSidewalk(g, TopPedestrianTop,
                    sidewalkBrush, sidewalkDarkBrush, sidewalkLightBrush);
                DrawMetroSidewalk(g, BottomPedestrianTop,
                    sidewalkBrush, sidewalkDarkBrush, sidewalkLightBrush);

                g.FillRectangle(bikeDarkBrush, 0, BikeTop - 2,
                    LogicalMapWidth, BikeLaneHeight * 2 + 4);
                g.FillRectangle(bikeBrush, 0, BikeTop,
                    LogicalMapWidth, BikeLaneHeight * 2);
                g.FillRectangle(laneBrush, 0,
                    BikeTop + BikeLaneHeight, LogicalMapWidth, 1);

                g.FillRectangle(grassDarkBrush, 0,
                    BikeTop + BikeLaneHeight * 2, LogicalMapWidth,
                    RoadTop - (BikeTop + BikeLaneHeight * 2));

                DrawMetroRoad(g, RoadTop, roadBrush,
                    roadPatchBrush, laneBrush);
                DrawMetroRoad(g, BottomRoadTop, roadBrush,
                    roadPatchBrush, laneBrush);

                g.FillRectangle(medianDarkBrush, 0, MedianTop,
                    LogicalMapWidth, MedianHeight);
                g.FillRectangle(medianBrush, 0, MedianTop + 2,
                    LogicalMapWidth, MedianHeight - 4);

                for (int x = 18; x < LogicalMapWidth; x += 64)
                    g.FillRectangle(laneBrush, x, MedianTop + 5, 10, 2);

                g.FillRectangle(grassDarkBrush, 0,
                    BottomRoadTop + RoadLaneHeight * 3,
                    LogicalMapWidth,
                    BottomPedestrianTop -
                    (BottomRoadTop + RoadLaneHeight * 3));

                DrawMetroVergeDetails(g);
                DrawMetroMapLabels(g);
            }

            g.Restore(state);
        }

        private void DrawMetroSidewalk(Graphics g, int top,
            Brush sidewalkBrush, Brush darkBrush, Brush lightBrush)
        {
            int height = PedestrianLaneHeight * 2;

            g.FillRectangle(darkBrush, 0, top - 2,
                LogicalMapWidth, height + 4);
            g.FillRectangle(sidewalkBrush, 0, top,
                LogicalMapWidth, height);
            g.FillRectangle(lightBrush, 0, top + 2,
                LogicalMapWidth, 2);
            g.FillRectangle(darkBrush, 0,
                top + PedestrianLaneHeight,
                LogicalMapWidth, 1);
            g.FillRectangle(lightBrush, 0,
                top + height - 4, LogicalMapWidth, 2);

            for (int x = 0; x < LogicalMapWidth; x += 32)
            {
                g.FillRectangle(darkBrush, x,
                    top + PedestrianLaneHeight - 1, 1, 3);
            }
        }

        private void DrawMetroRoad(Graphics g, int roadTop,
            Brush roadBrush, Brush patchBrush, Brush laneBrush)
        {
            int roadHeight = RoadLaneHeight * 3;

            g.FillRectangle(roadBrush, 0, roadTop,
                LogicalMapWidth, roadHeight);

            g.FillRectangle(patchBrush, 116, roadTop + 20, 64, 9);
            g.FillRectangle(patchBrush, 412, roadTop + 116, 82, 8);
            g.FillRectangle(patchBrush, 716, roadTop + 67, 54, 7);
            g.FillRectangle(patchBrush, 870, roadTop + 17, 74, 8);

            DrawMetroLaneDashes(g, roadTop + RoadLaneHeight, laneBrush);
            DrawMetroLaneDashes(g, roadTop + RoadLaneHeight * 2, laneBrush);

            using (SolidBrush shoulderBrush = new SolidBrush(
                Color.FromArgb(43, 45, 44)))
            {
                g.FillRectangle(shoulderBrush, 0, roadTop,
                    LogicalMapWidth, 3);
                g.FillRectangle(shoulderBrush, 0,
                    roadTop + roadHeight - 3,
                    LogicalMapWidth, 3);
            }
        }

        private void DrawMetroLaneDashes(Graphics g, int y, Brush laneBrush)
        {
            for (int x = 8; x < LogicalMapWidth; x += 42)
                g.FillRectangle(laneBrush, x, y - 1, 22, 2);
        }

        private void DrawMetroCityEdgeBuildings(Graphics g)
        {
            Color roof1 = Color.FromArgb(80, 76, 71);
            Color roof2 = Color.FromArgb(92, 86, 77);
            Color roof3 = Color.FromArgb(68, 69, 67);
            Color roofLine = Color.FromArgb(48, 48, 46);
            Color vent = Color.FromArgb(128, 119, 100);

            using (SolidBrush roof1Brush = new SolidBrush(roof1))
            using (SolidBrush roof2Brush = new SolidBrush(roof2))
            using (SolidBrush roof3Brush = new SolidBrush(roof3))
            using (SolidBrush roofLineBrush = new SolidBrush(roofLine))
            using (SolidBrush ventBrush = new SolidBrush(vent))
            {
                int[] widths = { 132, 176, 112, 156, 126, 168, 120 };
                int x = 0;

                for (int i = 0; x < LogicalMapWidth; i++)
                {
                    int width = widths[i % widths.Length];
                    Brush roofBrush = roof1Brush;

                    if (i % 3 == 1)
                        roofBrush = roof2Brush;
                    else if (i % 3 == 2)
                        roofBrush = roof3Brush;

                    g.FillRectangle(roofBrush, x, 0, width - 3, 18);
                    g.FillRectangle(roofLineBrush, x, 16, width - 3, 2);
                    g.FillRectangle(ventBrush, x + 18, 5, 18, 6);
                    g.FillRectangle(ventBrush, x + width - 42, 7, 12, 5);
                    x += width;
                }

                x = 0;
                for (int i = 0; x < LogicalMapWidth; i++)
                {
                    int width = widths[(i + 2) % widths.Length];
                    Brush roofBrush = roof3Brush;

                    if (i % 3 == 1)
                        roofBrush = roof1Brush;
                    else if (i % 3 == 2)
                        roofBrush = roof2Brush;

                    g.FillRectangle(roofLineBrush, x, 558, width - 3, 2);
                    g.FillRectangle(roofBrush, x, 560, width - 3, 20);
                    g.FillRectangle(ventBrush, x + 12, 566, 16, 6);
                    g.FillRectangle(ventBrush, x + width - 36, 568, 10, 5);
                    x += width;
                }
            }
        }

        private void DrawMetroVergeDetails(Graphics g)
        {
            using (SolidBrush planterBrush = new SolidBrush(
                Color.FromArgb(93, 86, 67)))
            using (SolidBrush planterEdgeBrush = new SolidBrush(
                Color.FromArgb(62, 59, 48)))
            using (SolidBrush grassAccentBrush = new SolidBrush(
                Color.FromArgb(88, 119, 75)))
            {
                for (int x = 28; x < LogicalMapWidth; x += 116)
                {
                    g.FillRectangle(planterEdgeBrush, x, 128, 42, 8);
                    g.FillRectangle(planterBrush, x + 2, 129, 38, 6);
                }

                for (int x = 18; x < LogicalMapWidth; x += 58)
                    g.FillRectangle(grassAccentBrush, x, 473, 22, 3);
            }

            DrawMetroTree(g, "Environment/Tree1.png",
                52, 462, 32, 43);
            DrawMetroTree(g, "Environment/Tree2.png",
                205, 466, 28, 39);
            DrawMetroTree(g, "Environment/Tree3.png",
                358, 461, 32, 44);
            DrawMetroTree(g, "Environment/Tree4.png",
                511, 475, 28, 31);
            DrawMetroTree(g, "Environment/Tree1.png",
                667, 463, 32, 43);
            DrawMetroTree(g, "Environment/Tree2.png",
                823, 466, 28, 39);
            DrawMetroTree(g, "Environment/Tree4.png",
                930, 476, 26, 29);
        }

        private void DrawMetroTree(Graphics g, string spritePath,
            int x, int y, int width, int height)
        {
            SpriteLibrary.Draw(g, spritePath,
                new Rectangle(x, y, width, height),
                TravelDirection.Right);
        }

        private void DrawMetroMapLabels(Graphics g)
        {
            using (Font mapFont = new Font("Consolas", 7F,
                FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(
                Color.FromArgb(235, 232, 211)))
            using (SolidBrush darkTextBrush = new SolidBrush(
                Color.FromArgb(63, 62, 58)))
            using (SolidBrush labelBack = new SolidBrush(
                Color.FromArgb(145, 25, 25, 24)))
            {
                DrawMetroLabel(g, "PED LEFT", 7,
                    TopPedestrianTop + 4, mapFont,
                    darkTextBrush, null);
                DrawMetroLabel(g, "PED RIGHT", 7,
                    TopPedestrianTop + PedestrianLaneHeight + 4,
                    mapFont, darkTextBrush, null);

                DrawMetroLabel(g, "BIKES LEFT", 7,
                    BikeTop + 4, mapFont, textBrush, labelBack);
                DrawMetroLabel(g, "BIKES RIGHT", 7,
                    BikeTop + BikeLaneHeight + 4,
                    mapFont, textBrush, labelBack);

                DrawMetroLabel(g, "ROAD LEFT", 7,
                    RoadTop + 5, mapFont, textBrush, labelBack);
                DrawMetroLabel(g, "ROAD RIGHT", 7,
                    BottomRoadTop + 5, mapFont, textBrush, labelBack);

                DrawMetroLabel(g, "PED LEFT", 7,
                    BottomPedestrianTop + 4,
                    mapFont, darkTextBrush, null);
                DrawMetroLabel(g, "PED RIGHT", 7,
                    BottomPedestrianTop + PedestrianLaneHeight + 4,
                    mapFont, darkTextBrush, null);
            }
        }

        private void DrawMetroLabel(Graphics g, string text,
            int x, int y, Font font, Brush textBrush, Brush backBrush)
        {
            if (backBrush != null)
                g.FillRectangle(backBrush, x - 2, y - 1,
                    text.Length * 5 + 5, 12);

            g.DrawString(text, font, textBrush, x, y);
        }
    }
}
