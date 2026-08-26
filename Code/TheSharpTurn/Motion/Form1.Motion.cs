using System;
using System.Drawing;
using System.Windows.Forms;

namespace TheSharpTurn
{
    public partial class Form1
    {
        const int BikeLaneChangeStep = 4;
        const int PedestrianLaneChangeStep = 6;
        const int BikeBrakeTickDelay = 2;
        const int PedestrianBrakeTickDelay = 2;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            simulationTimer.Tick += new EventHandler(SmoothMotionTimer_Tick);
            this.KeyDown += new KeyEventHandler(SmoothMotion_KeyDown);
        }

        private void SmoothMotionTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject obj = trafficObjects[i];

                if (obj is Bicycle)
                    UpdateBicycleSmoothMotion((Bicycle)obj);
                else if (obj is Pedestrian)
                    UpdatePedestrianSmoothMotion((Pedestrian)obj);
            }
        }

        private void SmoothMotion_KeyDown(object sender, KeyEventArgs e)
        {
            if (manualObject == null)
                return;

            bool laneKey = e.KeyCode == Keys.A || e.KeyCode == Keys.D ||
                e.KeyCode == Keys.Left || e.KeyCode == Keys.Right;

            if (manualObject is Bicycle)
            {
                Bicycle bicycle = (Bicycle)manualObject;

                if (laneKey)
                {
                    if (bicycle.IsChangingLane)
                    {
                        bicycle.Y = bicycle.PreviousMotionY;
                        bicycle.Lane = bicycle.LaneChangeTargetLane;
                        labelStatus.Text = "Finish the current lane change first.";
                    }
                    else
                    {
                        BeginBicycleLaneChangeIfNeeded(bicycle);

                        if (bicycle.IsChangingLane)
                            labelStatus.Text = "Lane change started.";
                    }
                }
                else if (e.KeyCode == Keys.Space)
                    labelStatus.Text = "Braking to stop.";
            }
            else if (manualObject is Pedestrian)
            {
                Pedestrian pedestrian = (Pedestrian)manualObject;

                if (laneKey)
                {
                    if (pedestrian.IsChangingLane)
                    {
                        pedestrian.Y = pedestrian.PreviousMotionY;
                        pedestrian.Lane = pedestrian.LaneChangeTargetLane;
                        labelStatus.Text = "Finish the current lane change first.";
                    }
                    else
                    {
                        BeginPedestrianLaneChangeIfNeeded(pedestrian);

                        if (pedestrian.IsChangingLane)
                            labelStatus.Text = "Lane change started.";
                    }
                }
                else if (e.KeyCode == Keys.Space)
                    labelStatus.Text = "Slowing to stop.";
            }
        }

        private void UpdateBicycleSmoothMotion(Bicycle bicycle)
        {
            InitializeBicycleMotionIfNeeded(bicycle);
            BeginBicycleLaneChangeIfNeeded(bicycle);
            AdvanceBicycleLaneChange(bicycle);
            ApplyBicycleGradualSpeed(bicycle);
        }

        private void InitializeBicycleMotionIfNeeded(Bicycle bicycle)
        {
            if (bicycle.MotionInitialized)
                return;

            bicycle.MotionSpeed = bicycle.ActualSpeed;
            bicycle.MotionBrakeTickCounter = 0;
            bicycle.PreviousMotionY = bicycle.Y;
            bicycle.IsChangingLane = false;
            bicycle.LaneChangeTargetY = bicycle.Y;
            bicycle.LaneChangeTargetLane = bicycle.Lane;
            bicycle.MotionInitialized = true;
        }

        private void BeginBicycleLaneChangeIfNeeded(Bicycle bicycle)
        {
            InitializeBicycleMotionIfNeeded(bicycle);

            if (bicycle.IsChangingLane)
            {
                if (bicycle.Y != bicycle.PreviousMotionY)
                    bicycle.Y = bicycle.PreviousMotionY;

                if (bicycle.Lane != bicycle.LaneChangeTargetLane)
                    bicycle.Lane = bicycle.LaneChangeTargetLane;

                return;
            }

            if (bicycle.Y == bicycle.PreviousMotionY)
                return;

            bicycle.LaneChangeTargetY = bicycle.Y;
            bicycle.LaneChangeTargetLane = bicycle.Lane;
            bicycle.Y = bicycle.PreviousMotionY;
            bicycle.IsChangingLane = true;
        }

        private void AdvanceBicycleLaneChange(Bicycle bicycle)
        {
            if (!bicycle.IsChangingLane)
            {
                bicycle.PreviousMotionY = bicycle.Y;
                return;
            }

            int nextY = bicycle.Y;

            if (bicycle.Y < bicycle.LaneChangeTargetY)
            {
                nextY += BikeLaneChangeStep;

                if (nextY > bicycle.LaneChangeTargetY)
                    nextY = bicycle.LaneChangeTargetY;
            }
            else if (bicycle.Y > bicycle.LaneChangeTargetY)
            {
                nextY -= BikeLaneChangeStep;

                if (nextY < bicycle.LaneChangeTargetY)
                    nextY = bicycle.LaneChangeTargetY;
            }

            Rectangle nextBounds = new Rectangle(bicycle.X, nextY,
                bicycle.Width, bicycle.Height);

            if (trafficObjects.IsAreaFree(nextBounds, bicycle))
                bicycle.Y = nextY;

            bicycle.PreviousMotionY = bicycle.Y;

            if (bicycle.Y == bicycle.LaneChangeTargetY)
                bicycle.IsChangingLane = false;
        }

        private void ApplyBicycleGradualSpeed(Bicycle bicycle)
        {
            int targetSpeed = bicycle.ActualSpeed;

            if (bicycle.MotionSpeed <= targetSpeed)
            {
                bicycle.MotionSpeed = targetSpeed;
                bicycle.MotionBrakeTickCounter = 0;
                return;
            }

            bicycle.MotionBrakeTickCounter++;
            int smoothSpeed = bicycle.MotionSpeed;

            if (bicycle.MotionBrakeTickCounter >= BikeBrakeTickDelay)
            {
                bicycle.MotionBrakeTickCounter = 0;
                smoothSpeed--;
            }

            if (smoothSpeed < targetSpeed)
                smoothSpeed = targetSpeed;

            int extraSpeed = smoothSpeed - targetSpeed;
            int plannedExtraSpeed = extraSpeed;

            while (extraSpeed > 0 &&
                !trafficObjects.IsAreaFree(GetMovedBounds(bicycle, extraSpeed),
                    bicycle))
            {
                extraSpeed--;
            }

            if (extraSpeed < plannedExtraSpeed)
            {
                smoothSpeed = targetSpeed + extraSpeed;
                bicycle.MotionBrakeTickCounter = 0;
            }

            if (extraSpeed > 0)
            {
                bicycle.ActualSpeed = extraSpeed;
                bicycle.Move();
            }

            bicycle.ActualSpeed = smoothSpeed;
            bicycle.MotionSpeed = smoothSpeed;
        }

        private void UpdatePedestrianSmoothMotion(Pedestrian pedestrian)
        {
            InitializePedestrianMotionIfNeeded(pedestrian);
            BeginPedestrianLaneChangeIfNeeded(pedestrian);
            AdvancePedestrianLaneChange(pedestrian);
            ApplyPedestrianGradualSpeed(pedestrian);
        }

        private void InitializePedestrianMotionIfNeeded(Pedestrian pedestrian)
        {
            if (pedestrian.MotionInitialized)
                return;

            pedestrian.MotionSpeed = pedestrian.ActualSpeed;
            pedestrian.MotionBrakeTickCounter = 0;
            pedestrian.PreviousMotionY = pedestrian.Y;
            pedestrian.IsChangingLane = false;
            pedestrian.LaneChangeTargetY = pedestrian.Y;
            pedestrian.LaneChangeTargetLane = pedestrian.Lane;
            pedestrian.MotionInitialized = true;
        }

        private void BeginPedestrianLaneChangeIfNeeded(Pedestrian pedestrian)
        {
            InitializePedestrianMotionIfNeeded(pedestrian);

            if (pedestrian.IsChangingLane)
            {
                if (pedestrian.Y != pedestrian.PreviousMotionY)
                    pedestrian.Y = pedestrian.PreviousMotionY;

                if (pedestrian.Lane != pedestrian.LaneChangeTargetLane)
                    pedestrian.Lane = pedestrian.LaneChangeTargetLane;

                return;
            }

            if (pedestrian.Y == pedestrian.PreviousMotionY)
                return;

            pedestrian.LaneChangeTargetY = pedestrian.Y;
            pedestrian.LaneChangeTargetLane = pedestrian.Lane;
            pedestrian.Y = pedestrian.PreviousMotionY;
            pedestrian.IsChangingLane = true;
        }

        private void AdvancePedestrianLaneChange(Pedestrian pedestrian)
        {
            if (!pedestrian.IsChangingLane)
            {
                pedestrian.PreviousMotionY = pedestrian.Y;
                return;
            }

            int nextY = pedestrian.Y;

            if (pedestrian.Y < pedestrian.LaneChangeTargetY)
            {
                nextY += PedestrianLaneChangeStep;

                if (nextY > pedestrian.LaneChangeTargetY)
                    nextY = pedestrian.LaneChangeTargetY;
            }
            else if (pedestrian.Y > pedestrian.LaneChangeTargetY)
            {
                nextY -= PedestrianLaneChangeStep;

                if (nextY < pedestrian.LaneChangeTargetY)
                    nextY = pedestrian.LaneChangeTargetY;
            }

            Rectangle nextBounds = new Rectangle(pedestrian.X, nextY,
                pedestrian.Width, pedestrian.Height);

            if (trafficObjects.IsAreaFree(nextBounds, pedestrian))
                pedestrian.Y = nextY;

            pedestrian.PreviousMotionY = pedestrian.Y;

            if (pedestrian.Y == pedestrian.LaneChangeTargetY)
                pedestrian.IsChangingLane = false;
        }

        private void ApplyPedestrianGradualSpeed(Pedestrian pedestrian)
        {
            int targetSpeed = pedestrian.ActualSpeed;

            if (pedestrian.MotionSpeed <= targetSpeed)
            {
                pedestrian.MotionSpeed = targetSpeed;
                pedestrian.MotionBrakeTickCounter = 0;
                return;
            }

            pedestrian.MotionBrakeTickCounter++;
            int smoothSpeed = pedestrian.MotionSpeed;

            if (pedestrian.MotionBrakeTickCounter >= PedestrianBrakeTickDelay)
            {
                pedestrian.MotionBrakeTickCounter = 0;
                smoothSpeed--;
            }

            if (smoothSpeed < targetSpeed)
                smoothSpeed = targetSpeed;

            int extraSpeed = smoothSpeed - targetSpeed;
            int plannedExtraSpeed = extraSpeed;

            while (extraSpeed > 0 &&
                !trafficObjects.IsAreaFree(GetMovedBounds(pedestrian, extraSpeed),
                    pedestrian))
            {
                extraSpeed--;
            }

            if (extraSpeed < plannedExtraSpeed)
            {
                smoothSpeed = targetSpeed + extraSpeed;
                pedestrian.MotionBrakeTickCounter = 0;
            }

            if (extraSpeed > 0)
            {
                pedestrian.ActualSpeed = extraSpeed;
                pedestrian.Move();
            }

            pedestrian.ActualSpeed = smoothSpeed;
            pedestrian.MotionSpeed = smoothSpeed;
        }
    }
}
