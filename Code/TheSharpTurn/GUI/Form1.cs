using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TheSharpTurn
{
    public partial class Form1 : Form
    {
        TrafficObjectList trafficObjects = new TrafficObjectList();
        int curIndex = -1;
        bool placementMode = false;
        TrafficObject manualObject = (TrafficObject)null;
        Timer simulationTimer = new Timer();

        const int RoadLaneHeight = 50;
        const int RoadTop = 145;
        const int MedianHeight = 12;
        const int BikeTop = 75;
        const int BikeLaneHeight = 24;
        const int PedestrianLaneHeight = 24;
        const int TopPedestrianTop = 20;
        const int BottomPedestrianTop = 510;

        const int RoadLaneLimit = 12;
        const int BikeLaneLimit = 20;
        const int PedestrianLaneLimit = 20;

        public Form1()
        {
            InitializeComponent();

            comboType.SelectedIndex = 0;
            UpdateModelChoices();

            buttonManual.Click += new EventHandler(buttonManual_Click);
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);

            UpdateSelectedInfo();

            simulationTimer.Interval = 40;
            simulationTimer.Tick += new EventHandler(simulationTimer_Tick);
            simulationTimer.Start();
        }

        private int MedianTop
        {
            get
            {
                return RoadTop + 3 * RoadLaneHeight;
            }
        }

        private int BottomRoadTop
        {
            get
            {
                return MedianTop + MedianHeight;
            }
        }

        private void comboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateModelChoices();
        }

        private void UpdateModelChoices()
        {
            comboModel.Items.Clear();

            switch (comboType.SelectedIndex)
            {
                case 0:
                    comboModel.Items.Add("Sports Car");
                    comboModel.Items.Add("Sedan");
                    comboModel.Items.Add("Hatchback");
                    numSpeed.Maximum = 5;
                    numSpeed.Value = 3;
                    break;

                case 1:
                    comboModel.Items.Add("Sports Bike");
                    comboModel.Items.Add("Chopper");
                    numSpeed.Maximum = 5;
                    numSpeed.Value = 3;
                    break;

                case 2:
                    comboModel.Items.Add("City Bus");
                    comboModel.Items.Add("Intercity Bus");
                    numSpeed.Maximum = 5;
                    numSpeed.Value = 3;
                    break;

                case 3:
                    comboModel.Items.Add("Ambulance");
                    comboModel.Items.Add("Fire Truck");
                    comboModel.Items.Add("Police Car");
                    numSpeed.Maximum = 5;
                    numSpeed.Value = 4;
                    break;

                case 4:
                    comboModel.Items.Add("Cruiser");
                    comboModel.Items.Add("BMX");
                    comboModel.Items.Add("Mountain Bike");
                    numSpeed.Maximum = 3;
                    numSpeed.Value = 2;
                    break;

                case 5:
                    comboModel.Items.Add("Male");
                    comboModel.Items.Add("Female");
                    numSpeed.Maximum = 2;
                    numSpeed.Value = 1;
                    break;
            }

            if (comboModel.Items.Count > 0)
                comboModel.SelectedIndex = 0;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (manualObject != null)
                EndManualMode("Manual Mode ended.");

            curIndex = -1;
            placementMode = true;
            UpdateSelectedInfo();
            labelStatus.Text = "Placement mode: click a valid lane on the map.";
        }

        private void pictureBoxMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (manualObject != null)
            {
                EndManualMode("Manual Mode ended by map click.");
                return;
            }

            if (placementMode)
            {
                TryPlaceObject(e.X, e.Y);
                return;
            }

            curIndex = trafficObjects.FindObjectIndexAt(e.X, e.Y);

            if (curIndex >= 0)
            {
                SetEditorFromSelectedObject();
                labelStatus.Text = "Object selected.";
            }
            else
            {
                labelStatus.Text = "No object selected.";
            }

            UpdateSelectedInfo();
            pictureBoxMap.Invalidate();
        }

        private void TryPlaceObject(int xP, int yP)
        {
            int lane = -1;
            int laneTop = 0;
            int laneHeight = 0;
            int laneLimit = 0;
            TravelDirection direction = TravelDirection.Right;

            if (comboType.SelectedIndex >= 0 && comboType.SelectedIndex <= 3)
            {
                lane = GetRoadLane(yP);

                if (lane < 0)
                {
                    labelStatus.Text = "Road vehicles must be placed on a road lane.";
                    return;
                }

                laneTop = GetRoadLaneTop(lane);
                laneHeight = RoadLaneHeight;
                laneLimit = RoadLaneLimit;

                if (lane <= 2)
                    direction = TravelDirection.Left;
                else
                    direction = TravelDirection.Right;
            }
            else if (comboType.SelectedIndex == 4)
            {
                lane = GetBikeLane(yP);

                if (lane < 0)
                {
                    labelStatus.Text = "Bicycles must be placed on one of the bicycle lanes.";
                    return;
                }

                laneTop = BikeTop + (lane - 6) * BikeLaneHeight;
                laneHeight = BikeLaneHeight;
                laneLimit = BikeLaneLimit;

                if (lane == 6)
                    direction = TravelDirection.Left;
                else
                    direction = TravelDirection.Right;
            }
            else if (comboType.SelectedIndex == 5)
            {
                lane = GetPedestrianLane(yP);

                if (lane < 0)
                {
                    labelStatus.Text = "Pedestrians must be placed on a pedestrian lane.";
                    return;
                }

                laneTop = GetPedestrianLaneTop(lane);
                laneHeight = PedestrianLaneHeight;
                laneLimit = PedestrianLaneLimit;

                if (lane == 8 || lane == 10)
                    direction = TravelDirection.Left;
                else
                    direction = TravelDirection.Right;
            }

            if (lane < 0)
                return;

            if (trafficObjects.CountObjectsInLane(lane) >= laneLimit)
            {
                labelStatus.Text = "That lane has reached its object limit.";
                return;
            }

            TrafficObject obj = CreateSelectedObject(lane, direction);
            if (obj == null)
                return;

            if (direction == TravelDirection.Right)
                obj.X = -obj.Width - 10;
            else
                obj.X = pictureBoxMap.Width + 10;

            obj.Y = laneTop + (laneHeight - obj.Height) / 2;

            if (!trafficObjects.IsAreaFree(obj.Bounds))
            {
                labelStatus.Text = "The entrance is blocked. Wait for the area to become free.";
                return;
            }

            trafficObjects.Add(obj);
            curIndex = trafficObjects.Count - 1;
            placementMode = false;

            SetEditorFromSelectedObject();
            UpdateSelectedInfo();
            labelStatus.Text = "Object added successfully.";
            pictureBoxMap.Invalidate();
        }

        private TrafficObject CreateSelectedObject(int lane, TravelDirection direction)
        {
            int speed = (int)numSpeed.Value;

            switch (comboType.SelectedIndex)
            {
                case 0:
                    return new Car(0, 0, lane, direction, speed,
                        (CarModel)comboModel.SelectedIndex);

                case 1:
                    return new Motorcycle(0, 0, lane, direction, speed,
                        (MotorcycleModel)comboModel.SelectedIndex);

                case 2:
                    return new Bus(0, 0, lane, direction, speed,
                        (BusModel)comboModel.SelectedIndex);

                case 3:
                    return new EmergencyVehicle(0, 0, lane, direction, speed,
                        (EmergencyVehicleModel)comboModel.SelectedIndex);

                case 4:
                    return new Bicycle(0, 0, lane, direction, speed,
                        (BicycleModel)comboModel.SelectedIndex);

                case 5:
                    return new Pedestrian(0, 0, lane, direction, speed,
                        (PedestrianModel)comboModel.SelectedIndex);
            }

            return (TrafficObject)null;
        }

        private void buttonManual_Click(object sender, EventArgs e)
        {
            if (manualObject != null)
            {
                EndManualMode("Manual Mode ended.");
                return;
            }

            if (curIndex < 0 || curIndex >= trafficObjects.Count)
            {
                labelStatus.Text = "Select an object before entering Manual Mode.";
                return;
            }

            manualObject = trafficObjects[curIndex];
            placementMode = false;
            buttonManual.Text = "Stop Manually Controlling Object";
            labelStatus.Text = "Manual Mode: W/S speed, A/D lane, Space stop, H action, Esc exit.";
            UpdateSelectedInfo();
        }

        private void EndManualMode(string message)
        {
            manualObject = (TrafficObject)null;
            buttonManual.Text = "Manually Control Selected Object";
            labelStatus.Text = message;
            UpdateSelectedInfo();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (manualObject == null)
                return;

            if (e.KeyCode == Keys.Escape)
            {
                EndManualMode("Manual Mode ended.");
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
            {
                if (manualObject.DesiredSpeed < manualObject.MaximumSpeed)
                    manualObject.DesiredSpeed++;

                labelStatus.Text = "Desired speed increased.";
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                if (manualObject.DesiredSpeed > 0)
                    manualObject.DesiredSpeed--;

                labelStatus.Text = "Desired speed decreased.";
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Space)
            {
                manualObject.DesiredSpeed = 0;

                if (manualObject is Pedestrian)
                    labelStatus.Text = "Slowing to stop.";
                else
                    labelStatus.Text = "Braking to stop.";

                e.Handled = true;
            }
            else if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
            {
                int laneDirection = -1;

                if (manualObject.Direction == TravelDirection.Left)
                    laneDirection = 1;

                TryManualLaneChange(manualObject, laneDirection);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
            {
                int laneDirection = 1;

                if (manualObject.Direction == TravelDirection.Left)
                    laneDirection = -1;

                TryManualLaneChange(manualObject, laneDirection);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.H)
            {
                HandleManualAudioAction();
                e.Handled = true;
            }

            UpdateSelectedInfo();
            pictureBoxMap.Invalidate();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (curIndex < 0 || curIndex >= trafficObjects.Count)
            {
                labelStatus.Text = "Select an object before deleting it.";
                return;
            }

            if (trafficObjects[curIndex] == manualObject)
                EndManualMode("Manual Mode ended because the object was deleted.");

            trafficObjects.Remove(curIndex);
            curIndex = -1;
            placementMode = false;
            UpdateSelectedInfo();
            labelStatus.Text = "Object deleted.";
            pictureBoxMap.Invalidate();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog1.Filter = TrafficObjectFile.FileFilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.DefaultExt = TrafficObjectFile.FileExtension;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                TrafficObjectFile.Save(saveFileDialog1.FileName, trafficObjects);
                labelStatus.Text = "Scene saved.";
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog1.Filter = TrafficObjectFile.FileFilter;
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                trafficObjects = TrafficObjectFile.Load(openFileDialog1.FileName);
                manualObject = (TrafficObject)null;
                buttonManual.Text = "Manually Control Selected Object";
                curIndex = -1;
                placementMode = false;
                UpdateSelectedInfo();
                labelStatus.Text = "Scene loaded.";
                pictureBoxMap.Invalidate();
            }
        }

        private void pictureBoxMap_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            DrawMap(g);
            trafficObjects.DrawAll(g);

            if (curIndex >= 0 && curIndex < trafficObjects.Count)
            {
                Rectangle selectedBounds = trafficObjects[curIndex].Bounds;
                selectedBounds.Inflate(3, 3);
                g.DrawRectangle(Pens.Gold, selectedBounds);
            }
        }

        private void DrawMap(Graphics g)
        {
            g.Clear(Color.FromArgb(94, 130, 82));

            g.FillRectangle(Brushes.LightGray, 0, TopPedestrianTop,
                pictureBoxMap.Width, PedestrianLaneHeight * 2);
            g.DrawRectangle(Pens.DimGray, 0, TopPedestrianTop,
                pictureBoxMap.Width - 1, PedestrianLaneHeight * 2);
            g.DrawLine(Pens.DarkGray, 0, TopPedestrianTop + PedestrianLaneHeight,
                pictureBoxMap.Width, TopPedestrianTop + PedestrianLaneHeight);

            g.FillRectangle(Brushes.LightGray, 0, BottomPedestrianTop,
                pictureBoxMap.Width, PedestrianLaneHeight * 2);
            g.DrawRectangle(Pens.DimGray, 0, BottomPedestrianTop,
                pictureBoxMap.Width - 1, PedestrianLaneHeight * 2);
            g.DrawLine(Pens.DarkGray, 0, BottomPedestrianTop + PedestrianLaneHeight,
                pictureBoxMap.Width, BottomPedestrianTop + PedestrianLaneHeight);

            g.FillRectangle(Brushes.DarkSeaGreen, 0, BikeTop,
                pictureBoxMap.Width, BikeLaneHeight * 2);
            g.DrawRectangle(Pens.DarkGreen, 0, BikeTop,
                pictureBoxMap.Width - 1, BikeLaneHeight * 2);
            g.DrawLine(Pens.White, 0, BikeTop + BikeLaneHeight,
                pictureBoxMap.Width, BikeTop + BikeLaneHeight);

            g.FillRectangle(Brushes.DimGray, 0, RoadTop,
                pictureBoxMap.Width, RoadLaneHeight * 3);
            g.FillRectangle(Brushes.DimGray, 0, BottomRoadTop,
                pictureBoxMap.Width, RoadLaneHeight * 3);

            g.FillRectangle(Brushes.Gray, 0, MedianTop,
                pictureBoxMap.Width, MedianHeight);

            DrawRoadLaneLines(g, RoadTop);
            DrawRoadLaneLines(g, BottomRoadTop);

            using (Font mapFont = new Font("Arial", 8, FontStyle.Bold))
            {
                g.DrawString("PED LEFT", mapFont, Brushes.DimGray,
                    8, TopPedestrianTop + 5);
                g.DrawString("PED RIGHT", mapFont, Brushes.DimGray,
                    8, TopPedestrianTop + PedestrianLaneHeight + 5);
                g.DrawString("BIKES LEFT", mapFont, Brushes.White,
                    8, BikeTop + 5);
                g.DrawString("BIKES RIGHT", mapFont, Brushes.White,
                    8, BikeTop + BikeLaneHeight + 5);
                g.DrawString("ROAD LEFT", mapFont, Brushes.White,
                    8, RoadTop + 5);
                g.DrawString("ROAD RIGHT", mapFont, Brushes.White,
                    8, BottomRoadTop + 5);
                g.DrawString("PED LEFT", mapFont, Brushes.DimGray,
                    8, BottomPedestrianTop + 5);
                g.DrawString("PED RIGHT", mapFont, Brushes.DimGray,
                    8, BottomPedestrianTop + PedestrianLaneHeight + 5);
            }
        }

        private void DrawRoadLaneLines(Graphics g, int roadTop)
        {
            for (int lane = 1; lane < 3; lane++)
            {
                int y = roadTop + lane * RoadLaneHeight;

                for (int x = 0; x < pictureBoxMap.Width; x += 42)
                    g.DrawLine(Pens.White, x, y, x + 22, y);
            }
        }

        private int GetRoadLane(int yP)
        {
            if (yP >= RoadTop && yP < RoadTop + 3 * RoadLaneHeight)
                return (yP - RoadTop) / RoadLaneHeight;

            if (yP >= BottomRoadTop && yP < BottomRoadTop + 3 * RoadLaneHeight)
                return 3 + (yP - BottomRoadTop) / RoadLaneHeight;

            return -1;
        }

        private int GetRoadLaneTop(int lane)
        {
            if (lane >= 0 && lane <= 2)
                return RoadTop + lane * RoadLaneHeight;

            if (lane >= 3 && lane <= 5)
                return BottomRoadTop + (lane - 3) * RoadLaneHeight;

            return 0;
        }

        private int GetBikeLane(int yP)
        {
            if (yP >= BikeTop && yP < BikeTop + BikeLaneHeight)
                return 6;

            if (yP >= BikeTop + BikeLaneHeight &&
                yP < BikeTop + BikeLaneHeight * 2)
                return 7;

            return -1;
        }

        private int GetPedestrianLane(int yP)
        {
            if (yP >= TopPedestrianTop &&
                yP < TopPedestrianTop + PedestrianLaneHeight * 2)
                return 8 + (yP - TopPedestrianTop) / PedestrianLaneHeight;

            if (yP >= BottomPedestrianTop &&
                yP < BottomPedestrianTop + PedestrianLaneHeight * 2)
                return 10 + (yP - BottomPedestrianTop) / PedestrianLaneHeight;

            return -1;
        }

        private int GetPedestrianLaneTop(int lane)
        {
            if (lane >= 8 && lane <= 9)
                return TopPedestrianTop + (lane - 8) * PedestrianLaneHeight;

            if (lane >= 10 && lane <= 11)
                return BottomPedestrianTop + (lane - 10) * PedestrianLaneHeight;

            return 0;
        }

        private int GetObjectTypeIndex(TrafficObject obj)
        {
            if (obj is Car)
                return 0;
            if (obj is Motorcycle)
                return 1;
            if (obj is Bus)
                return 2;
            if (obj is EmergencyVehicle)
                return 3;
            if (obj is Bicycle)
                return 4;
            if (obj is Pedestrian)
                return 5;

            return -1;
        }

        private void SetEditorFromSelectedObject()
        {
            if (curIndex < 0 || curIndex >= trafficObjects.Count)
                return;

            TrafficObject obj = trafficObjects[curIndex];
            comboType.SelectedIndex = GetObjectTypeIndex(obj);

            if (obj is Car)
                comboModel.SelectedIndex = (int)((Car)obj).Model;
            else if (obj is Motorcycle)
                comboModel.SelectedIndex = (int)((Motorcycle)obj).Model;
            else if (obj is Bus)
                comboModel.SelectedIndex = (int)((Bus)obj).Model;
            else if (obj is EmergencyVehicle)
                comboModel.SelectedIndex = (int)((EmergencyVehicle)obj).Model;
            else if (obj is Bicycle)
                comboModel.SelectedIndex = (int)((Bicycle)obj).Model;
            else if (obj is Pedestrian)
                comboModel.SelectedIndex = (int)((Pedestrian)obj).Model;
        }

        private void UpdateSelectedInfo()
        {
            if (curIndex >= 0 && curIndex < trafficObjects.Count)
            {
                TrafficObject obj = trafficObjects[curIndex];
                labelSelected.Text = "Selected: " + obj.ToString() +
                    "\r\nLane: " + obj.Lane.ToString() +
                    "\r\nSpeed: " + obj.ActualSpeed.ToString() +
                    " / " + obj.DesiredSpeed.ToString();

                buttonManual.Enabled = true;
            }
            else
            {
                labelSelected.Text = "Selected: none";
                buttonManual.Enabled = manualObject != null;
            }
        }
    }
}
