using System;
using System.Drawing;
using System.Windows.Forms;

namespace TheSharpTurn
{
    public partial class Form1
    {
        Button buttonFullscreen;
        Panel panelFocusMode;
        Label labelFocusModeTitle;
        Label labelFocusModeInstructions;

        bool focusModeActive = false;
        bool fullscreenActive = false;
        Rectangle windowedBounds;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            windowedBounds = this.Bounds;
            this.MinimumSize = this.Size;

            CreatePhase8Controls();

            buttonFullscreen.Click += new EventHandler(buttonFullscreen_Click);
            this.Resize += new EventHandler(Form1_Resize_Phase8);
            this.KeyDown += new KeyEventHandler(Form1_KeyDown_Phase8);
            buttonManual.Click += new EventHandler(UpdatePhase8AfterAction);
            buttonAdd.Click += new EventHandler(UpdatePhase8AfterAction);
            buttonDelete.Click += new EventHandler(UpdatePhase8AfterAction);
            buttonLoad.Click += new EventHandler(UpdatePhase8AfterAction);
            pictureBoxMap.MouseDown += new MouseEventHandler(pictureBoxMap_MouseDown_Phase8);
            simulationTimer.Tick += new EventHandler(simulationTimer_Tick_Phase8);

            UpdatePhase8State();
        }

        private void CreatePhase8Controls()
        {
            buttonFullscreen = new Button();
            buttonFullscreen.Name = "buttonFullscreen";
            buttonFullscreen.Size = new Size(105, 30);
            buttonFullscreen.Text = "Fullscreen";
            buttonFullscreen.UseVisualStyleBackColor = true;
            this.Controls.Add(buttonFullscreen);

            panelFocusMode = new Panel();
            panelFocusMode.Name = "panelFocusMode";
            panelFocusMode.Size = new Size(410, 126);
            panelFocusMode.BackColor = Color.FromArgb(40, 40, 40);
            panelFocusMode.BorderStyle = BorderStyle.FixedSingle;
            panelFocusMode.Visible = false;

            labelFocusModeTitle = new Label();
            labelFocusModeTitle.Location = new Point(12, 10);
            labelFocusModeTitle.Size = new Size(384, 24);
            labelFocusModeTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelFocusModeTitle.ForeColor = Color.White;
            labelFocusModeTitle.Text = "DRIVE MODE";

            labelFocusModeInstructions = new Label();
            labelFocusModeInstructions.Location = new Point(12, 38);
            labelFocusModeInstructions.Size = new Size(384, 76);
            labelFocusModeInstructions.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            labelFocusModeInstructions.ForeColor = Color.White;

            panelFocusMode.Controls.Add(labelFocusModeTitle);
            panelFocusMode.Controls.Add(labelFocusModeInstructions);
            this.Controls.Add(panelFocusMode);
            panelFocusMode.BringToFront();
        }

        private void buttonFullscreen_Click(object sender, EventArgs e)
        {
            ToggleFullscreen();
        }

        private void ToggleFullscreen()
        {
            if (!fullscreenActive)
            {
                windowedBounds = this.Bounds;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                fullscreenActive = true;
                buttonFullscreen.Text = "Windowed";
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.Bounds = windowedBounds;
                fullscreenActive = false;
                buttonFullscreen.Text = "Fullscreen";
            }

            UpdatePhase8Layout();
        }

        private void Form1_Resize_Phase8(object sender, EventArgs e)
        {
            UpdatePhase8Layout();
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
        }

        private void pictureBoxMap_MouseDown_Phase8(object sender, MouseEventArgs e)
        {
            UpdatePhase8State();
        }

        private void simulationTimer_Tick_Phase8(object sender, EventArgs e)
        {
            UpdatePhase8State();
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
            buttonFullscreen.Visible = visible;
            panelControls.Visible = visible;
            labelStatus.Visible = visible;
        }

        private void UpdateFocusModeText()
        {
            if (manualObject is EmergencyVehicle)
            {
                labelFocusModeTitle.Text = "EMERGENCY DRIVE MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up     S / Down: Slow down\r\n" +
                    "A / Left: Move left  D / Right: Move right\r\n" +
                    "Space: Brake         H: Toggle siren\r\n" +
                    "Esc or any mouse click on the map: Exit";
            }
            else if (manualObject is Bicycle)
            {
                labelFocusModeTitle.Text = "CYCLE MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up     S / Down: Slow down\r\n" +
                    "A / Left: Move left  D / Right: Move right\r\n" +
                    "Space: Brake         H: Bike horn\r\n" +
                    "Esc or any mouse click on the map: Exit";
            }
            else if (manualObject is Pedestrian)
            {
                labelFocusModeTitle.Text = "WALK MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up     S / Down: Slow down\r\n" +
                    "A / Left: Move left  D / Right: Move right\r\n" +
                    "Space: Stop          H: Shout\r\n" +
                    "Esc or any mouse click on the map: Exit";
            }
            else
            {
                labelFocusModeTitle.Text = "DRIVE MODE";
                labelFocusModeInstructions.Text =
                    "W / Up: Speed up     S / Down: Slow down\r\n" +
                    "A / Left: Move left  D / Right: Move right\r\n" +
                    "Space: Brake         H: Horn\r\n" +
                    "Esc or any mouse click on the map: Exit";
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
            if (pictureBoxMap == null)
                return;

            if (focusModeActive)
                UpdateFocusModeLayout();
            else
                UpdateNormalModeLayout();
        }

        private void UpdateNormalModeLayout()
        {
            const int ContentWidth = 1238;
            const int GroupHeight = 669;
            const int SidePanelWidth = 250;
            const int Gap = 18;

            int left = (this.ClientSize.Width - ContentWidth) / 2;
            int top = (this.ClientSize.Height - GroupHeight) / 2;

            if (left < 12)
                left = 12;
            if (top < 10)
                top = 10;

            labelTitle.Location = new Point(left, top);

            buttonLoad.Location = new Point(left + ContentWidth - buttonLoad.Width,
                top + 4);
            buttonSave.Location = new Point(buttonLoad.Left - buttonSave.Width - 6,
                top + 4);
            buttonFullscreen.Location = new Point(buttonSave.Left - buttonFullscreen.Width - 6,
                top + 4);

            panelControls.Location = new Point(left, top + 48);
            pictureBoxMap.Location = new Point(left + SidePanelWidth + Gap, top + 48);
            pictureBoxMap.Size = new Size(970, 580);

            labelStatus.Location = new Point(left, top + 641);
            labelStatus.Size = new Size(ContentWidth, 28);
        }

        private void UpdateFocusModeLayout()
        {
            int mapLeft = (this.ClientSize.Width - pictureBoxMap.Width) / 2;
            int mapTop = (this.ClientSize.Height - pictureBoxMap.Height) / 2;

            if (mapLeft < 0)
                mapLeft = 0;
            if (mapTop < 0)
                mapTop = 0;

            pictureBoxMap.Location = new Point(mapLeft, mapTop);
            pictureBoxMap.Size = new Size(970, 580);

            panelFocusMode.Location = new Point(pictureBoxMap.Left + 12,
                pictureBoxMap.Top + 12);
            panelFocusMode.BringToFront();
        }
    }
}
