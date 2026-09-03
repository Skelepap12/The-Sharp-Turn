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
            ApplyPhase8Style();

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
            pictureBoxDisplay.Paint += new PaintEventHandler(
                pictureBoxDisplay_Paint_MetroCity);
            pictureBoxDisplay.MouseDown += new MouseEventHandler(
                pictureBoxDisplay_MouseDown);
            this.Controls.Add(pictureBoxDisplay);

            pictureBoxMap.Size = new Size(LogicalMapWidth, LogicalMapHeight);
            pictureBoxMap.Visible = false;

            Panel selectedDivider = new Panel();
            selectedDivider.Name = "selectedDivider";
            selectedDivider.Location = new Point(16, 280);
            selectedDivider.Size = new Size(216, 1);
            selectedDivider.Anchor = AnchorStyles.Top |
                AnchorStyles.Left | AnchorStyles.Right;
            panelControls.Controls.Add(selectedDivider);

            panelFocusMode = new Panel();
            panelFocusMode.Name = "panelFocusMode";
            panelFocusMode.Size = new Size(760, 72);
            panelFocusMode.BorderStyle = BorderStyle.FixedSingle;
            panelFocusMode.Visible = false;

            Panel focusAccent = new Panel();
            focusAccent.Name = "focusAccent";
            focusAccent.Dock = DockStyle.Top;
            focusAccent.Height = 2;

            labelFocusModeTitle = new Label();
            labelFocusModeTitle.Location = new Point(14, 10);
            labelFocusModeTitle.Size = new Size(230, 50);
            labelFocusModeTitle.TextAlign = ContentAlignment.MiddleLeft;
            labelFocusModeTitle.Text = "DRIVE MODE";

            labelFocusModeInstructions = new Label();
            labelFocusModeInstructions.Location = new Point(250, 8);
            labelFocusModeInstructions.Size = new Size(496, 54);
            labelFocusModeInstructions.TextAlign = ContentAlignment.MiddleLeft;

            panelFocusMode.Controls.Add(focusAccent);
            panelFocusMode.Controls.Add(labelFocusModeTitle);
            panelFocusMode.Controls.Add(labelFocusModeInstructions);
            this.Controls.Add(panelFocusMode);
            panelFocusMode.BringToFront();
        }

        private void ApplyPhase8Style()
        {
            Color formBack = Color.FromArgb(22, 23, 22);
            Color panelBack = Color.FromArgb(31, 32, 31);
            Color panelRaised = Color.FromArgb(41, 42, 40);
            Color inputBack = Color.FromArgb(47, 48, 46);
            Color cream = Color.FromArgb(232, 226, 202);
            Color muted = Color.FromArgb(164, 161, 149);
            Color gold = Color.FromArgb(196, 157, 74);
            Color danger = Color.FromArgb(191, 105, 92);

            this.BackColor = formBack;
            this.ForeColor = cream;

            labelTitle.Text = "THE SHARP TURN";
            labelTitle.Font = new Font("Consolas", 16F, FontStyle.Bold);
            labelTitle.ForeColor = cream;

            StyleDarkButton(buttonSave, panelRaised, cream, gold);
            StyleDarkButton(buttonLoad, panelRaised, cream, gold);

            panelControls.BackColor = panelBack;
            panelControls.ForeColor = cream;

            labelObjectMenu.Text = "CREATE OBJECT";
            labelObjectMenu.Font = new Font("Consolas", 11F, FontStyle.Bold);
            labelObjectMenu.ForeColor = gold;
            labelObjectMenu.Location = new Point(16, 14);

            labelType.Text = "TYPE";
            labelType.Location = new Point(16, 50);
            StyleFieldLabel(labelType, muted);

            comboType.Location = new Point(16, 70);
            comboType.Size = new Size(216, 24);
            StyleComboBox(comboType, inputBack, cream);

            labelModel.Text = "MODEL";
            labelModel.Location = new Point(16, 108);
            StyleFieldLabel(labelModel, muted);

            comboModel.Location = new Point(16, 128);
            comboModel.Size = new Size(216, 24);
            StyleComboBox(comboModel, inputBack, cream);

            labelSpeed.Text = "DESIRED SPEED";
            labelSpeed.Location = new Point(16, 166);
            StyleFieldLabel(labelSpeed, muted);

            numSpeed.Location = new Point(16, 186);
            numSpeed.Size = new Size(216, 24);
            numSpeed.BackColor = inputBack;
            numSpeed.ForeColor = cream;
            numSpeed.BorderStyle = BorderStyle.FixedSingle;
            numSpeed.Font = new Font("Consolas", 9F, FontStyle.Regular);

            buttonAdd.Location = new Point(16, 226);
            buttonAdd.Size = new Size(216, 44);
            buttonAdd.Text = "ADD OBJECT";
            StyleAccentButton(buttonAdd, gold, formBack);

            Control[] dividerControls = panelControls.Controls.Find(
                "selectedDivider", false);
            if (dividerControls.Length > 0)
                dividerControls[0].BackColor = Color.FromArgb(78, 76, 68);

            labelSelectedTitle.Text = "SELECTED";
            labelSelectedTitle.Location = new Point(16, 294);
            labelSelectedTitle.Font = new Font("Consolas", 10F,
                FontStyle.Bold);
            labelSelectedTitle.ForeColor = gold;

            labelSelected.Location = new Point(16, 320);
            labelSelected.Size = new Size(216, 74);
            labelSelected.BackColor = panelRaised;
            labelSelected.ForeColor = cream;
            labelSelected.BorderStyle = BorderStyle.FixedSingle;
            labelSelected.Font = new Font("Consolas", 8.5F,
                FontStyle.Regular);
            labelSelected.Padding = new Padding(8, 5, 8, 5);
            labelSelected.TextAlign = ContentAlignment.MiddleLeft;

            buttonManual.Location = new Point(16, 410);
            buttonManual.Size = new Size(216, 46);
            StyleDarkButton(buttonManual, panelRaised, cream, gold);

            buttonDelete.Location = new Point(16, 468);
            buttonDelete.Size = new Size(216, 40);
            StyleDarkButton(buttonDelete, panelRaised, danger, danger);

            labelStatus.BackColor = panelBack;
            labelStatus.ForeColor = muted;
            labelStatus.BorderStyle = BorderStyle.FixedSingle;
            labelStatus.Font = new Font("Consolas", 8.5F,
                FontStyle.Regular);
            labelStatus.Padding = new Padding(8, 0, 8, 0);

            pictureBoxDisplay.BackColor = Color.FromArgb(15, 15, 15);
            pictureBoxDisplay.BorderStyle = BorderStyle.FixedSingle;

            panelFocusMode.BackColor = panelBack;
            panelFocusMode.ForeColor = cream;

            Control[] focusAccentControls = panelFocusMode.Controls.Find(
                "focusAccent", false);
            if (focusAccentControls.Length > 0)
                focusAccentControls[0].BackColor = gold;

            labelFocusModeTitle.Font = new Font("Consolas", 10F,
                FontStyle.Bold);
            labelFocusModeTitle.ForeColor = gold;

            labelFocusModeInstructions.Font = new Font("Consolas", 8.5F,
                FontStyle.Regular);
            labelFocusModeInstructions.ForeColor = cream;
        }

        private void StyleFieldLabel(Label label, Color color)
        {
            label.Font = new Font("Consolas", 8F, FontStyle.Bold);
            label.ForeColor = color;
        }

        private void StyleComboBox(ComboBox comboBox, Color backColor,
            Color foreColor)
        {
            comboBox.BackColor = backColor;
            comboBox.ForeColor = foreColor;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.Font = new Font("Consolas", 9F, FontStyle.Regular);
        }

        private void StyleDarkButton(Button button, Color backColor,
            Color foreColor, Color borderColor)
        {
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = borderColor;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Consolas", 9F, FontStyle.Bold);
        }

        private void StyleAccentButton(Button button, Color backColor,
            Color foreColor)
        {
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = backColor;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Consolas", 9F, FontStyle.Bold);
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

                if (windowStateBeforeFullscreen ==
                    FormWindowState.Maximized)
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

        private void pictureBoxDisplay_MouseDown(object sender,
            MouseEventArgs e)
        {
            if (manualObject != null)
            {
                MouseEventArgs exitEvent = new MouseEventArgs(e.Button,
                    e.Clicks, 0, 0, e.Delta);
                pictureBoxMap_MouseDown(pictureBoxMap, exitEvent);
                UpdatePhase8State();
                pictureBoxDisplay.Invalidate();
                return;
            }

            int logicalX;
            int logicalY;

            if (!TryGetLogicalMapPoint(e.X, e.Y,
                out logicalX, out logicalY))
                return;

            MouseEventArgs logicalEvent = new MouseEventArgs(e.Button,
                e.Clicks, logicalX, logicalY, e.Delta);
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
            int offsetX =
                (pictureBoxDisplay.ClientSize.Width - drawWidth) / 2;
            int offsetY =
                (pictureBoxDisplay.ClientSize.Height - drawHeight) / 2;

            if (displayX < offsetX ||
                displayX >= offsetX + drawWidth ||
                displayY < offsetY ||
                displayY >= offsetY + drawHeight)
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
            else
                buttonManual.Text = "CONTROL SELECTED";

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
            string modeTitle = "DRIVE MODE";

            if (manualObject is EmergencyVehicle)
            {
                modeTitle = "EMERGENCY DRIVE MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up   S / Down: Slow down   " +
                    "A / Left: Move left   D / Right: Move right\r\n" +
                    "Space: Brake   H: Toggle siren   " +
                    "Esc or map click: Exit";
            }
            else if (manualObject is Bicycle)
            {
                modeTitle = "CYCLE MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up   S / Down: Slow down   " +
                    "A / Left: Move left   D / Right: Move right\r\n" +
                    "Space: Brake   H: Bike horn   " +
                    "Esc or map click: Exit";
            }
            else if (manualObject is Pedestrian)
            {
                modeTitle = "WALK MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up   S / Down: Slow down   " +
                    "A / Left: Move left   D / Right: Move right\r\n" +
                    "Space: Stop   H: Shout   Esc or map click: Exit";
            }
            else
            {
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up   S / Down: Slow down   " +
                    "A / Left: Move left   D / Right: Move right\r\n" +
                    "Space: Brake   H: Horn   Esc or map click: Exit";
            }

            if (manualObject != null)
            {
                labelFocusModeTitle.Text = modeTitle +
                    "\r\nLANE " + manualObject.Lane.ToString() +
                    "    SPEED " +
                    manualObject.ActualSpeed.ToString() + " / " +
                    manualObject.MaximumSpeed.ToString();
            }
            else
            {
                labelFocusModeTitle.Text = modeTitle;
            }
        }

        private void RefreshSelectedInfoDisplay()
        {
            if (curIndex >= 0 && curIndex < trafficObjects.Count)
            {
                TrafficObject obj = trafficObjects[curIndex];

                labelSelected.Text = obj.ToString() +
                    "\r\nLANE  " + obj.Lane.ToString() +
                    "\r\nSPEED " + obj.ActualSpeed.ToString() +
                    " / " + obj.MaximumSpeed.ToString();
            }
            else
            {
                labelSelected.Text = "No object selected.";
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
            const int Gap = 12;
            const int ContentTop = 58;

            labelTitle.Location = new Point(Margin, 11);

            buttonLoad.Size = new Size(82, 30);
            buttonSave.Size = new Size(82, 30);

            buttonLoad.Location = new Point(
                this.ClientSize.Width - Margin - buttonLoad.Width, 14);
            buttonSave.Location = new Point(
                buttonLoad.Left - buttonSave.Width - 7, 14);

            int statusTop = this.ClientSize.Height - 39;
            int contentHeight = statusTop - ContentTop - 12;

            if (contentHeight < 200)
                contentHeight = 200;

            panelControls.Location = new Point(Margin, ContentTop);
            panelControls.Size = new Size(
                SidePanelWidth, contentHeight);

            int displayLeft =
                Margin + SidePanelWidth + Gap;
            int displayWidth =
                this.ClientSize.Width - displayLeft - Margin;

            if (displayWidth < 100)
                displayWidth = 100;

            pictureBoxDisplay.Location =
                new Point(displayLeft, ContentTop);
            pictureBoxDisplay.Size =
                new Size(displayWidth, contentHeight);

            labelStatus.Location =
                new Point(Margin, statusTop);
            labelStatus.Size =
                new Size(this.ClientSize.Width - Margin * 2, 27);

            pictureBoxDisplay.SendToBack();
            panelControls.BringToFront();
        }

        private void UpdateFocusModeLayout()
        {
            const int Margin = 12;
            const int HeaderHeight = 72;
            const int HeaderGap = 8;

            int headerWidth =
                this.ClientSize.Width - Margin * 2;

            if (headerWidth < 300)
                headerWidth = 300;

            panelFocusMode.Location =
                new Point(Margin, Margin);
            panelFocusMode.Size =
                new Size(headerWidth, HeaderHeight);

            labelFocusModeTitle.Location =
                new Point(14, 10);
            labelFocusModeTitle.Size =
                new Size(230, 50);
            labelFocusModeInstructions.Location =
                new Point(250, 8);

            int instructionsWidth =
                panelFocusMode.ClientSize.Width - 262;
            if (instructionsWidth < 100)
                instructionsWidth = 100;

            labelFocusModeInstructions.Size =
                new Size(instructionsWidth, 54);

            int mapTop =
                Margin + HeaderHeight + HeaderGap;
            int mapHeight =
                this.ClientSize.Height - mapTop - Margin;

            if (mapHeight < 100)
                mapHeight = 100;

            pictureBoxDisplay.Location =
                new Point(Margin, mapTop);
            pictureBoxDisplay.Size =
                new Size(this.ClientSize.Width - Margin * 2,
                    mapHeight);

            pictureBoxDisplay.SendToBack();
            panelFocusMode.BringToFront();
        }
    }
}
