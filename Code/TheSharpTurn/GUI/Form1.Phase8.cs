using System;
using System.Drawing;
using System.Windows.Forms;

namespace TheSharpTurn
{
    public partial class Form1
    {
        const int LogicalMapWidth = 970;
        const int LogicalMapHeight = 580;

        PictureBox pictureBoxDisplay;
        Panel panelFocusMode;
        Label labelFocusModeTitle;
        Label labelFocusModeInstructions;

        bool focusModeActive = false;
        bool fullscreenActive = false;
        Rectangle windowedBounds;
        FormWindowState windowStateBeforeFullscreen = FormWindowState.Normal;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            windowedBounds = this.Bounds;
            this.MinimumSize = new Size(800, 650);

            CreatePhase8Controls();

            this.Resize += new EventHandler(Form1_Resize_Phase8);
            this.KeyDown += new KeyEventHandler(Form1_KeyDown_Phase8);
            buttonManual.Click += new EventHandler(UpdatePhase8AfterAction);
            buttonAdd.Click += new EventHandler(UpdatePhase8AfterAction);
            buttonDelete.Click += new EventHandler(UpdatePhase8AfterAction);
            buttonLoad.Click += new EventHandler(UpdatePhase8AfterAction);
            simulationTimer.Tick += new EventHandler(simulationTimer_Tick_Phase8);

            UpdatePhase8State();
        }

        private void CreatePhase8Controls()
        {
            pictureBoxDisplay = new PictureBox();
            pictureBoxDisplay.Name = "pictureBoxDisplay";
            pictureBoxDisplay.BackColor = pictureBoxMap.BackColor;
            pictureBoxDisplay.BorderStyle = pictureBoxMap.BorderStyle;
            pictureBoxDisplay.TabStop = false;
            pictureBoxDisplay.Paint += new PaintEventHandler(pictureBoxDisplay_Paint);
            pictureBoxDisplay.MouseDown += new MouseEventHandler(pictureBoxDisplay_MouseDown);
            this.Controls.Add(pictureBoxDisplay);

            pictureBoxMap.Size = new Size(LogicalMapWidth, LogicalMapHeight);
            pictureBoxMap.Visible = false;

            panelFocusMode = new Panel();
            panelFocusMode.Name = "panelFocusMode";
            panelFocusMode.Size = new Size(760, 72);
            panelFocusMode.BackColor = Color.FromArgb(40, 40, 40);
            panelFocusMode.BorderStyle = BorderStyle.FixedSingle;
            panelFocusMode.Visible = false;

            labelFocusModeTitle = new Label();
            labelFocusModeTitle.Location = new Point(14, 10);
            labelFocusModeTitle.Size = new Size(190, 50);
            labelFocusModeTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelFocusModeTitle.ForeColor = Color.White;
            labelFocusModeTitle.TextAlign = ContentAlignment.MiddleLeft;
            labelFocusModeTitle.Text = "DRIVE MODE";

            labelFocusModeInstructions = new Label();
            labelFocusModeInstructions.Location = new Point(210, 8);
            labelFocusModeInstructions.Size = new Size(536, 54);
            labelFocusModeInstructions.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            labelFocusModeInstructions.ForeColor = Color.White;
            labelFocusModeInstructions.TextAlign = ContentAlignment.MiddleLeft;

            panelFocusMode.Controls.Add(labelFocusModeTitle);
            panelFocusMode.Controls.Add(labelFocusModeInstructions);
            this.Controls.Add(panelFocusMode);
            panelFocusMode.BringToFront();
        }

        private void ToggleFullscreen()
        {
            if (!fullscreenActive)
            {
                windowStateBeforeFullscreen = this.WindowState;

                if (this.WindowState == FormWindowState.Normal)
                    windowedBounds = this.Bounds;
                else
                    windowedBounds = this.RestoreBounds;

                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                fullscreenActive = true;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MaximizeBox = true;

                if (windowStateBeforeFullscreen == FormWindowState.Maximized)
                    this.WindowState = FormWindowState.Maximized;
                else
                    this.Bounds = windowedBounds;

                fullscreenActive = false;
            }

            UpdatePhase8Layout();
            pictureBoxDisplay.Invalidate();
        }

        private void Form1_Resize_Phase8(object sender, EventArgs e)
        {
            UpdatePhase8Layout();
            pictureBoxDisplay.Invalidate();
        }

        private void Form1_KeyDown_Phase8(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullscreen();
                e.Handled = true;
                return;
            }

            UpdatePhase8State();
        }

        private void UpdatePhase8AfterAction(object sender, EventArgs e)
        {
            UpdatePhase8State();
            pictureBoxDisplay.Invalidate();
        }

        private void simulationTimer_Tick_Phase8(object sender, EventArgs e)
        {
            UpdatePhase8State();
            pictureBoxDisplay.Invalidate();
        }

        private void pictureBoxDisplay_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(35, 35, 35));

            float scale = GetMapScale();
            if (scale <= 0)
                return;

            int drawWidth = (int)(LogicalMapWidth * scale);
            int drawHeight = (int)(LogicalMapHeight * scale);
            int offsetX = (pictureBoxDisplay.ClientSize.Width - drawWidth) / 2;
            int offsetY = (pictureBoxDisplay.ClientSize.Height - drawHeight) / 2;

            e.Graphics.TranslateTransform(offsetX, offsetY);
            e.Graphics.ScaleTransform(scale, scale);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            DrawMap(e.Graphics);
            trafficObjects.DrawAll(e.Graphics);

            if (curIndex >= 0 && curIndex < trafficObjects.Count)
            {
                Rectangle selectedBounds = trafficObjects[curIndex].Bounds;
                selectedBounds.Inflate(3, 3);
                e.Graphics.DrawRectangle(Pens.Gold, selectedBounds);
            }
        }

        private void pictureBoxDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            if (manualObject != null)
            {
                MouseEventArgs exitEvent = new MouseEventArgs(e.Button, e.Clicks,
                    0, 0, e.Delta);
                pictureBoxMap_MouseDown(pictureBoxMap, exitEvent);
                UpdatePhase8State();
                pictureBoxDisplay.Invalidate();
                return;
            }

            int logicalX;
            int logicalY;

            if (!TryGetLogicalMapPoint(e.X, e.Y, out logicalX, out logicalY))
                return;

            MouseEventArgs logicalEvent = new MouseEventArgs(e.Button, e.Clicks,
                logicalX, logicalY, e.Delta);
            pictureBoxMap_MouseDown(pictureBoxMap, logicalEvent);
            UpdatePhase8State();
            pictureBoxDisplay.Invalidate();
        }

        private float GetMapScale()
        {
            if (pictureBoxDisplay.ClientSize.Width <= 0 ||
                pictureBoxDisplay.ClientSize.Height <= 0)
                return 0;

            float scaleX = pictureBoxDisplay.ClientSize.Width /
                (float)LogicalMapWidth;
            float scaleY = pictureBoxDisplay.ClientSize.Height /
                (float)LogicalMapHeight;

            return Math.Min(scaleX, scaleY);
        }

        private bool TryGetLogicalMapPoint(int displayX, int displayY,
            out int logicalX, out int logicalY)
        {
            logicalX = 0;
            logicalY = 0;

            float scale = GetMapScale();
            if (scale <= 0)
                return false;

            int drawWidth = (int)(LogicalMapWidth * scale);
            int drawHeight = (int)(LogicalMapHeight * scale);
            int offsetX = (pictureBoxDisplay.ClientSize.Width - drawWidth) / 2;
            int offsetY = (pictureBoxDisplay.ClientSize.Height - drawHeight) / 2;

            if (displayX < offsetX || displayX >= offsetX + drawWidth ||
                displayY < offsetY || displayY >= offsetY + drawHeight)
                return false;

            logicalX = (int)((displayX - offsetX) / scale);
            logicalY = (int)((displayY - offsetY) / scale);

            if (logicalX >= LogicalMapWidth)
                logicalX = LogicalMapWidth - 1;
            if (logicalY >= LogicalMapHeight)
                logicalY = LogicalMapHeight - 1;

            return true;
        }

        private void UpdatePhase8State()
        {
            bool shouldUseFocusMode = manualObject != null;

            if (shouldUseFocusMode != focusModeActive)
            {
                focusModeActive = shouldUseFocusMode;
                SetNormalControlsVisible(!focusModeActive);
                panelFocusMode.Visible = focusModeActive;
            }

            if (focusModeActive)
                UpdateFocusModeText();

            RefreshSelectedInfoDisplay();
            UpdatePhase8Layout();
        }

        private void SetNormalControlsVisible(bool visible)
        {
            labelTitle.Visible = visible;
            buttonSave.Visible = visible;
            buttonLoad.Visible = visible;
            panelControls.Visible = visible;
            labelStatus.Visible = visible;
        }

        private void UpdateFocusModeText()
        {
            if (manualObject is EmergencyVehicle)
            {
                labelFocusModeTitle.Text = "EMERGENCY DRIVE MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up   S / Down: Slow down   A / Left: Move left   D / Right: Move right\r\n" +
                    "Space: Brake   H: Toggle siren   Esc or any mouse click on the map: Exit";
            }
            else if (manualObject is Bicycle)
            {
                labelFocusModeTitle.Text = "CYCLE MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up   S / Down: Slow down   A / Left: Move left   D / Right: Move right\r\n" +
                    "Space: Brake   H: Bike horn   Esc or any mouse click on the map: Exit";
            }
            else if (manualObject is Pedestrian)
            {
                labelFocusModeTitle.Text = "WALK MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up   S / Down: Slow down   A / Left: Move left   D / Right: Move right\r\n" +
                    "Space: Stop   H: Shout   Esc or any mouse click on the map: Exit";
            }
            else
            {
                labelFocusModeTitle.Text = "DRIVE MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up   S / Down: Slow down   A / Left: Move left   D / Right: Move right\r\n" +
                    "Space: Brake   H: Horn   Esc or any mouse click on the map: Exit";
            }
        }

        private void RefreshSelectedInfoDisplay()
        {
            if (curIndex >= 0 && curIndex < trafficObjects.Count)
            {
                TrafficObject obj = trafficObjects[curIndex];
                labelSelected.Text = "Selected: " + obj.ToString() +
                    "\r\nLane: " + obj.Lane.ToString() +
                    "\r\nSpeed: " + obj.ActualSpeed.ToString() +
                    " / " + obj.MaximumSpeed.ToString();
            }
            else
            {
                labelSelected.Text = "Selected: none";
            }
        }

        private void UpdatePhase8Layout()
        {
            if (pictureBoxDisplay == null)
                return;

            if (focusModeActive)
                UpdateFocusModeLayout();
            else
                UpdateNormalModeLayout();
        }

        private void UpdateNormalModeLayout()
        {
            const int Margin = 12;
            const int SidePanelWidth = 250;
            const int Gap = 18;
            const int ContentTop = 58;

            labelTitle.Location = new Point(Margin, 10);

            buttonLoad.Location = new Point(
                this.ClientSize.Width - Margin - buttonLoad.Width, 14);
            buttonSave.Location = new Point(
                buttonLoad.Left - buttonSave.Width - 6, 14);

            int statusTop = this.ClientSize.Height - 40;
            int contentHeight = statusTop - ContentTop - 13;

            if (contentHeight < 200)
                contentHeight = 200;

            panelControls.Location = new Point(Margin, ContentTop);
            panelControls.Size = new Size(SidePanelWidth, contentHeight);

            int displayLeft = Margin + SidePanelWidth + Gap;
            int displayWidth = this.ClientSize.Width - displayLeft - Margin;

            if (displayWidth < 100)
                displayWidth = 100;

            pictureBoxDisplay.Location = new Point(displayLeft, ContentTop);
            pictureBoxDisplay.Size = new Size(displayWidth, contentHeight);

            labelStatus.Location = new Point(Margin, statusTop);
            labelStatus.Size = new Size(this.ClientSize.Width - Margin * 2, 28);

            pictureBoxDisplay.SendToBack();
            panelControls.BringToFront();
        }

        private void UpdateFocusModeLayout()
        {
            const int Margin = 12;
            const int HeaderHeight = 72;
            const int HeaderGap = 8;

            int headerWidth = this.ClientSize.Width - Margin * 2;

            if (headerWidth < 300)
                headerWidth = 300;

            panelFocusMode.Location = new Point(Margin, Margin);
            panelFocusMode.Size = new Size(headerWidth, HeaderHeight);

            labelFocusModeTitle.Location = new Point(14, 10);
            labelFocusModeTitle.Size = new Size(190, 50);
            labelFocusModeInstructions.Location = new Point(210, 8);

            int instructionsWidth = panelFocusMode.ClientSize.Width - 222;
            if (instructionsWidth < 100)
                instructionsWidth = 100;

            labelFocusModeInstructions.Size = new Size(instructionsWidth, 54);

            int mapTop = Margin + HeaderHeight + HeaderGap;
            int mapHeight = this.ClientSize.Height - mapTop - Margin;

            if (mapHeight < 100)
                mapHeight = 100;

            pictureBoxDisplay.Location = new Point(Margin, mapTop);
            pictureBoxDisplay.Size = new Size(
                this.ClientSize.Width - Margin * 2,
                mapHeight);

            pictureBoxDisplay.SendToBack();
            panelFocusMode.BringToFront();
        }
    }
}
