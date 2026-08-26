using System;
using System.Drawing;

namespace TheSharpTurn
{
    public partial class Form1
    {
        const int RoadLaneChangeStep = 5;
        const int RoadBrakeTickDelay = 2;
        const int BikeLaneChangeStep = 4;
        const int PedestrianLaneChangeStep = 6;
        const int BikeBrakeTickDelay = 2;
        const int PedestrianBrakeTickDelay = 2;

        private void simulationTimer_Tick(object sender, EventArgs e)
        {
            for (int i = trafficObjects.Count - 1; i >= 0; i--)
            {
                TrafficObject obj = trafficObjects[i];

                if (obj == manualObject)
                {
                    if (obj is EmergencyVehicle &&
                        ((EmergencyVehicle)obj).SirenOn)
                        UpdateEmergencyVehicle((EmergencyVehicle)obj);
                    else
                        MoveAtBestSpeed(obj);
                }
                else if (obj is EmergencyVehicle)
                    UpdateEmergencyVehicle((EmergencyVehicle)obj);
                else if (obj is RoadUser)
                    UpdateRoadUser((RoadUser)obj);
                else if (obj is Bicycle)
                    UpdateBicycle((Bicycle)obj);
                else if (obj is Pedestrian)
                    UpdatePedestrian((Pedestrian)obj);
                else
                    MoveAtBestSpeed(obj);

                if (obj is RoadUser)
                    UpdateRoadLaneChange((RoadUser)obj);

                if (IsOutsideMap(obj))
                {
                    if (obj == manualObject)
                        EndManualMode("Manual Mode ended because the object left the map.");

                    trafficObjects.Remove(i);

                    if (curIndex == i)
                        curIndex = -1;
                    else if (curIndex > i)
                        curIndex--;
                }
            }

            UpdateSmoothMotion();
            UpdateSelectedInfo();
            pictureBoxMap.Invalidate();
        }

        private void UpdateRoadUser(RoadUser roadUser)
        {
            if (roadUser.IsOvertaking && !roadUser.IsChangingLane)
            {
                if (roadUser.Lane == roadUser.OvertakeReturnLane)
                {
                    roadUser.IsOvertaking = false;
                    roadUser.OvertakeReturnLane = -1;
                }
                else
                {
                    RoadUser returnLaneBlocker = FindRoadUserAheadInLane(roadUser,
                        roadUser.OvertakeReturnLane);
                    bool returnLaneReady = returnLaneBlocker == null ||
                        GetForwardGap(roadUser, returnLaneBlocker) >
                        GetRoadBrakingDistance(roadUser);

                    if (returnLaneReady && TryMoveRoadUserToLane(roadUser,
                        roadUser.OvertakeReturnLane))
                    {
                        roadUser.IsOvertaking = false;
                        roadUser.OvertakeReturnLane = -1;
                    }
                }
            }

            if (!roadUser.IsOvertaking)
            {
                RoadUser blocker = FindRoadUserAhead(roadUser);

                if (blocker != null)
                {
                    int gap = GetForwardGap(roadUser, blocker);
                    bool blockerIsSlower = blocker.ActualSpeed < roadUser.DesiredSpeed ||
                        blocker.DesiredSpeed < roadUser.DesiredSpeed;

                    if (blockerIsSlower && gap <= GetRoadBrakingDistance(roadUser))
                    {
                        if (!TryRegularRoadOvertake(roadUser))
                        {
                            MoveAtBestSpeed(roadUser, blocker.ActualSpeed);
                            return;
                        }
                    }
                }
            }

            MoveAtBestSpeed(roadUser);
        }

        private void UpdateEmergencyVehicle(EmergencyVehicle emergency)
        {
            if (!emergency.SirenOn)
            {
                UpdateRoadUser(emergency);
                return;
            }

            RoadUser blocker = FindRoadUserAhead(emergency);

            if (blocker != null)
            {
                int gap = GetForwardGap(emergency, blocker);

                if (gap <= GetRoadBrakingDistance(emergency) &&
                    blocker != manualObject)
                {
                    if (TryEmergencyRoadLaneChange(blocker))
                    {
                        blocker.IsOvertaking = false;
                        blocker.OvertakeReturnLane = -1;
                    }
                }
            }

            MoveAtBestSpeed(emergency);
        }

        private int GetRoadBrakingDistance(RoadUser roadUser)
        {
            return 12 + roadUser.ActualSpeed * 8;
        }

        private RoadUser FindRoadUserAhead(RoadUser roadUser)
        {
            return FindRoadUserAheadInLane(roadUser, roadUser.Lane);
        }

        private RoadUser FindRoadUserAheadInLane(RoadUser roadUser, int lane)
        {
            RoadUser nearest = (RoadUser)null;
            int nearestGap = int.MaxValue;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == roadUser || !(current is RoadUser))
                    continue;

                if (current.Lane != lane ||
                    current.Direction != roadUser.Direction)
                    continue;

                int gap = GetForwardGap(roadUser, current);

                if (gap < nearestGap)
                {
                    nearestGap = gap;
                    nearest = (RoadUser)current;
                }
            }

            return nearest;
        }

        private bool TryRegularRoadOvertake(RoadUser roadUser)
        {
            if (roadUser.IsChangingLane)
                return false;

            int leftLane;
            int rightLane;

            if (roadUser.Direction == TravelDirection.Right)
            {
                leftLane = roadUser.Lane - 1;
                rightLane = roadUser.Lane + 1;
            }
            else
            {
                leftLane = roadUser.Lane + 1;
                rightLane = roadUser.Lane - 1;
            }

            int returnLane = roadUser.Lane;

            if (!TryMoveRoadUserToLane(roadUser, leftLane) &&
                !TryMoveRoadUserToLane(roadUser, rightLane))
                return false;

            roadUser.OvertakeReturnLane = returnLane;
            roadUser.IsOvertaking = true;
            return true;
        }

        private bool TryEmergencyRoadLaneChange(RoadUser roadUser)
        {
            if (roadUser.IsChangingLane)
                return false;

            int rightLane;
            int leftLane;

            if (roadUser.Direction == TravelDirection.Right)
            {
                rightLane = roadUser.Lane + 1;
                leftLane = roadUser.Lane - 1;
            }
            else
            {
                rightLane = roadUser.Lane - 1;
                leftLane = roadUser.Lane + 1;
            }

            if (TryMoveRoadUserToLane(roadUser, rightLane))
                return true;

            return TryMoveRoadUserToLane(roadUser, leftLane);
        }

        private bool TryMoveRoadUserToLane(RoadUser roadUser, int targetLane)
        {
            if (roadUser.IsChangingLane)
                return false;

            if (targetLane < 0 || targetLane > 5)
                return false;

            if ((roadUser.Lane <= 2 && targetLane > 2) ||
                (roadUser.Lane >= 3 && targetLane < 3))
                return false;

            int targetY = GetRoadLaneTop(targetLane) +
                (RoadLaneHeight - roadUser.Height) / 2;
            int topY = Math.Min(roadUser.Y, targetY);
            int bottomY = Math.Max(roadUser.Y + roadUser.Height,
                targetY + roadUser.Height);

            Rectangle laneChangeBounds = new Rectangle(roadUser.X, topY,
                roadUser.Width, bottomY - topY);
            laneChangeBounds.Inflate(12, 2);

            if (!trafficObjects.IsAreaFree(laneChangeBounds, roadUser))
                return false;

            roadUser.Lane = targetLane;
            roadUser.LaneChangeTargetY = targetY;

            if (roadUser.Y == targetY)
                roadUser.IsChangingLane = false;
            else
                roadUser.IsChangingLane = true;

            return true;
        }

        private void UpdateRoadLaneChange(RoadUser roadUser)
        {
            if (!roadUser.IsChangingLane)
                return;

            int nextY = roadUser.Y;

            if (roadUser.Y < roadUser.LaneChangeTargetY)
            {
                nextY += RoadLaneChangeStep;

                if (nextY > roadUser.LaneChangeTargetY)
                    nextY = roadUser.LaneChangeTargetY;
            }
            else if (roadUser.Y > roadUser.LaneChangeTargetY)
            {
                nextY -= RoadLaneChangeStep;

                if (nextY < roadUser.LaneChangeTargetY)
                    nextY = roadUser.LaneChangeTargetY;
            }

            Rectangle nextBounds = new Rectangle(roadUser.X, nextY,
                roadUser.Width, roadUser.Height);

            if (!trafficObjects.IsAreaFree(nextBounds, roadUser))
                return;

            roadUser.Y = nextY;

            if (roadUser.Y == roadUser.LaneChangeTargetY)
                roadUser.IsChangingLane = false;
        }

        private void UpdateBicycle(Bicycle bicycle)
        {
            int correctLane;

            if (bicycle.Direction == TravelDirection.Left)
                correctLane = 6;
            else
                correctLane = 7;

            if (bicycle.Lane != correctLane)
            {
                Bicycle oncoming = FindBicycleAhead(bicycle, false);
                bool oncomingIsClose = oncoming != null &&
                    GetForwardGap(bicycle, oncoming) <= 40;

                Bicycle bicycleAheadInCorrectLane = FindBicycleAheadInLane(
                    bicycle, correctLane, true);
                bool stillPassing = bicycleAheadInCorrectLane != null &&
                    GetForwardGap(bicycle, bicycleAheadInCorrectLane) <=
                    bicycle.DesiredSpeed + 8;

                if (oncomingIsClose || !stillPassing)
                {
                    if (TryMoveBicycleToLane(bicycle, correctLane))
                    {
                        MoveAtBestSpeed(bicycle);
                        return;
                    }

                    if (oncomingIsClose)
                    {
                        bicycle.ActualSpeed = 0;
                        return;
                    }
                }

                MoveAtBestSpeed(bicycle);
                return;
            }

            Bicycle blocker = FindBicycleAhead(bicycle, true);

            if (blocker != null)
            {
                int gap = GetForwardGap(bicycle, blocker);
                bool blockerIsSlower = blocker.ActualSpeed < bicycle.DesiredSpeed ||
                    blocker.DesiredSpeed < bicycle.DesiredSpeed;

                if (blockerIsSlower && gap <= bicycle.DesiredSpeed + 8)
                {
                    int passingLane;

                    if (correctLane == 6)
                        passingLane = 7;
                    else
                        passingLane = 6;

                    if (IsBikePassingLaneSafe(bicycle, passingLane))
                        TryMoveBicycleToLane(bicycle, passingLane);
                }
            }

            MoveAtBestSpeed(bicycle);
        }

        private Bicycle FindBicycleAhead(Bicycle bicycle, bool sameDirection)
        {
            return FindBicycleAheadInLane(bicycle, bicycle.Lane,
                sameDirection);
        }

        private Bicycle FindBicycleAheadInLane(Bicycle bicycle, int lane,
            bool sameDirection)
        {
            Bicycle nearest = (Bicycle)null;
            int nearestGap = int.MaxValue;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == bicycle || !(current is Bicycle))
                    continue;

                if (current.Lane != lane)
                    continue;

                if (sameDirection && current.Direction != bicycle.Direction)
                    continue;

                if (!sameDirection && current.Direction == bicycle.Direction)
                    continue;

                int gap = GetForwardGap(bicycle, current);

                if (gap < nearestGap)
                {
                    nearestGap = gap;
                    nearest = (Bicycle)current;
                }
            }

            return nearest;
        }

        private bool IsBikePassingLaneSafe(Bicycle bicycle, int targetLane)
        {
            int targetY = BikeTop + (targetLane - 6) * BikeLaneHeight +
                (BikeLaneHeight - bicycle.Height) / 2;

            Rectangle targetBounds = new Rectangle(bicycle.X, targetY,
                bicycle.Width, bicycle.Height);
            Rectangle safeBounds = targetBounds;
            safeBounds.Inflate(8, 1);

            if (!trafficObjects.IsAreaFree(safeBounds, bicycle))
                return false;

            int bicycleCenter = bicycle.X + bicycle.Width / 2;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (!(current is Bicycle) || current == bicycle)
                    continue;

                if (current.Lane == targetLane &&
                    current.Direction != bicycle.Direction)
                {
                    int currentCenter = current.X + current.Width / 2;

                    if (Math.Abs(currentCenter - bicycleCenter) < 100)
                        return false;
                }
            }

            return true;
        }

        private bool TryMoveBicycleToLane(Bicycle bicycle, int targetLane)
        {
            if (targetLane < 6 || targetLane > 7)
                return false;

            int targetY = BikeTop + (targetLane - 6) * BikeLaneHeight +
                (BikeLaneHeight - bicycle.Height) / 2;

            Rectangle targetBounds = new Rectangle(bicycle.X, targetY,
                bicycle.Width, bicycle.Height);
            Rectangle safeBounds = targetBounds;
            safeBounds.Inflate(8, 1);

            if (!trafficObjects.IsAreaFree(safeBounds, bicycle))
                return false;

            bicycle.Lane = targetLane;
            bicycle.Y = targetY;
            return true;
        }

        private void UpdatePedestrian(Pedestrian pedestrian)
        {
            Pedestrian blocker = FindPedestrianAhead(pedestrian);

            if (blocker != null)
            {
                int gap = GetForwardGap(pedestrian, blocker);

                if (blocker.Direction == pedestrian.Direction)
                {
                    bool blockerIsSlower = blocker.ActualSpeed < pedestrian.DesiredSpeed ||
                        blocker.DesiredSpeed < pedestrian.DesiredSpeed;

                    if (blockerIsSlower && gap <= pedestrian.DesiredSpeed + 6)
                        TryMovePedestrianToLane(pedestrian,
                            GetPairedPedestrianLane(pedestrian.Lane));
                }
                else if (gap <= 20)
                {
                    int keepRightLane = GetPedestrianKeepRightLane(pedestrian);

                    if (pedestrian.Lane != keepRightLane)
                        TryMovePedestrianToLane(pedestrian, keepRightLane);
                }
            }

            MoveAtBestSpeed(pedestrian);
        }

        private Pedestrian FindPedestrianAhead(Pedestrian pedestrian)
        {
            Pedestrian nearest = (Pedestrian)null;
            int nearestGap = int.MaxValue;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == pedestrian || !(current is Pedestrian))
                    continue;

                if (current.Lane != pedestrian.Lane)
                    continue;

                int gap = GetForwardGap(pedestrian, current);

                if (gap < nearestGap)
                {
                    nearestGap = gap;
                    nearest = (Pedestrian)current;
                }
            }

            return nearest;
        }

        private int GetPairedPedestrianLane(int lane)
        {
            switch (lane)
            {
                case 8:
                    return 9;
                case 9:
                    return 8;
                case 10:
                    return 11;
                case 11:
                    return 10;
            }

            return lane;
        }

        private int GetPedestrianKeepRightLane(Pedestrian pedestrian)
        {
            if (pedestrian.Lane == 8 || pedestrian.Lane == 9)
            {
                if (pedestrian.Direction == TravelDirection.Right)
                    return 9;
                else
                    return 8;
            }

            if (pedestrian.Direction == TravelDirection.Right)
                return 11;
            else
                return 10;
        }

        private bool TryMovePedestrianToLane(Pedestrian pedestrian, int targetLane)
        {
            if (targetLane < 8 || targetLane > 11)
                return false;

            if ((pedestrian.Lane <= 9 && targetLane >= 10) ||
                (pedestrian.Lane >= 10 && targetLane <= 9))
                return false;

            int targetY = GetPedestrianLaneTop(targetLane) +
                (PedestrianLaneHeight - pedestrian.Height) / 2;

            Rectangle targetBounds = new Rectangle(pedestrian.X, targetY,
                pedestrian.Width, pedestrian.Height);
            Rectangle safeBounds = targetBounds;
            safeBounds.Inflate(3, 1);

            if (!trafficObjects.IsAreaFree(safeBounds, pedestrian))
                return false;

            pedestrian.Lane = targetLane;
            pedestrian.Y = targetY;
            return true;
        }

        private int GetForwardGap(TrafficObject obj, TrafficObject other)
        {
            int objCenter = obj.X + obj.Width / 2;
            int otherCenter = other.X + other.Width / 2;

            if (obj.Direction == TravelDirection.Right)
            {
                if (otherCenter <= objCenter)
                    return int.MaxValue;

                int gap = other.X - (obj.X + obj.Width);

                if (gap < 0)
                    gap = 0;

                return gap;
            }
            else
            {
                if (otherCenter >= objCenter)
                    return int.MaxValue;

                int gap = obj.X - (other.X + other.Width);

                if (gap < 0)
                    gap = 0;

                return gap;
            }
        }

        private void MoveAtBestSpeed(TrafficObject obj)
        {
            MoveAtBestSpeed(obj, obj.DesiredSpeed);
        }

        private void MoveAtBestSpeed(TrafficObject obj, int maximumSpeed)
        {
            if (maximumSpeed < 0)
                maximumSpeed = 0;

            int allowedSpeed = obj.DesiredSpeed;

            if (allowedSpeed > maximumSpeed)
                allowedSpeed = maximumSpeed;

            int movementSpeed = allowedSpeed;

            if (obj is RoadUser)
                movementSpeed = GetRoadUserSpeedForTick((RoadUser)obj,
                    allowedSpeed);

            int plannedSpeed = movementSpeed;

            while (movementSpeed > 0 &&
                !trafficObjects.IsAreaFree(GetMovedBounds(obj, movementSpeed), obj))
            {
                movementSpeed--;
            }

            if (obj is RoadUser && movementSpeed < plannedSpeed)
                ((RoadUser)obj).BrakeTickCounter = 0;

            obj.ActualSpeed = movementSpeed;

            if (obj.ActualSpeed > 0)
                obj.Move();
        }

        private int GetRoadUserSpeedForTick(RoadUser roadUser, int allowedSpeed)
        {
            if (roadUser.ActualSpeed <= allowedSpeed)
            {
                roadUser.BrakeTickCounter = 0;
                return allowedSpeed;
            }

            roadUser.BrakeTickCounter++;

            if (roadUser.BrakeTickCounter >= RoadBrakeTickDelay)
            {
                roadUser.BrakeTickCounter = 0;
                return roadUser.ActualSpeed - 1;
            }

            return roadUser.ActualSpeed;
        }

        private Rectangle GetMovedBounds(TrafficObject obj, int speed)
        {
            int nextX = obj.X;

            if (obj.Direction == TravelDirection.Right)
                nextX += speed;
            else
                nextX -= speed;

            return new Rectangle(nextX, obj.Y, obj.Width, obj.Height);
        }

        private bool IsOutsideMap(TrafficObject obj)
        {
            const int OffScreenBuffer = 20;

            return obj.X > pictureBoxMap.Width + OffScreenBuffer ||
                obj.X + obj.Width < -OffScreenBuffer;
        }

        private void TryManualLaneChange(TrafficObject obj, int verticalDirection)
        {
            bool changed = false;

            if (obj is RoadUser)
            {
                RoadUser roadUser = (RoadUser)obj;
                changed = TryMoveRoadUserToLane(roadUser,
                    roadUser.Lane + verticalDirection);

                if (changed)
                {
                    roadUser.IsOvertaking = false;
                    roadUser.OvertakeReturnLane = -1;
                }
            }
            else if (obj is Bicycle)
            {
                Bicycle bicycle = (Bicycle)obj;

                if (bicycle.IsChangingLane)
                {
                    labelStatus.Text = "Finish the current lane change first.";
                    return;
                }

                InitializeBicycleMotionIfNeeded(bicycle);
                changed = TryMoveBicycleToLane(bicycle,
                    bicycle.Lane + verticalDirection);

                if (changed)
                    BeginBicycleLaneChangeIfNeeded(bicycle);
            }
            else if (obj is Pedestrian)
            {
                Pedestrian pedestrian = (Pedestrian)obj;

                if (pedestrian.IsChangingLane)
                {
                    labelStatus.Text = "Finish the current lane change first.";
                    return;
                }

                InitializePedestrianMotionIfNeeded(pedestrian);
                changed = TryMovePedestrianToLane(pedestrian,
                    pedestrian.Lane + verticalDirection);

                if (changed)
                    BeginPedestrianLaneChangeIfNeeded(pedestrian);
            }

            if (changed)
                labelStatus.Text = "Lane change started.";
            else
                labelStatus.Text = "Lane change is not available or the target lane is blocked.";
        }

        private void UpdateSmoothMotion()
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
