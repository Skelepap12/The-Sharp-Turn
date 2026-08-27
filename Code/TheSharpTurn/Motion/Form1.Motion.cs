using System;
using System.Drawing;

namespace TheSharpTurn
{
    public partial class Form1
    {
        const int RoadLaneChangeStep = 5;
        const int RoadBrakeTickDelay = 2;
        const int RoadMergeRearClearance = 32;
        const int RoadLaneChangePadding = 16;
        const int RoadFollowingReleaseBuffer = 16;

        const int BikeLaneChangeStep = 4;
        const int BikeBrakeTickDelay = 2;
        const int BikeFollowingDistance = 18;
        const int BikePassClearance = 20;
        const int BikeOncomingClearance = 140;
        const int BikeHeadOnDistance = 60;
        const int BikeOvertakeSectionDistance = 140;
        const int BikeLaneChangePadding = 12;

        const int PedestrianLaneChangeStep = 6;
        const int PedestrianBrakeTickDelay = 2;
        const int PedestrianFollowingDistance = 10;
        const int PedestrianPassClearance = 12;
        const int PedestrianHeadOnDistance = 20;
        const int PedestrianLaneChangeClearance = 24;
        const int PedestrianLaneChangePadding = 12;

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
            if (roadUser.IsChangingLane)
            {
                if (!TryMaintainRoadFollowingDistance(roadUser))
                    MoveAtBestSpeed(roadUser);

                return;
            }

            if (roadUser.IsOvertaking)
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
                    RoadUser returnLaneFollower = FindRoadUserBehindInLane(roadUser,
                        roadUser.OvertakeReturnLane);

                    bool frontClear = returnLaneBlocker == null ||
                        GetForwardGap(roadUser, returnLaneBlocker) >
                        GetRoadBrakingDistance(roadUser);
                    bool rearClear = returnLaneFollower == null ||
                        GetRearGap(roadUser, returnLaneFollower) >
                        RoadMergeRearClearance;

                    if (frontClear && rearClear &&
                        TryMoveRoadUserToLane(roadUser,
                            roadUser.OvertakeReturnLane))
                    {
                        roadUser.IsOvertaking = false;
                        roadUser.OvertakeReturnLane = -1;
                    }
                }

                if (roadUser.IsOvertaking &&
                    TryMaintainRoadFollowingDistance(roadUser))
                    return;
            }

            if (!roadUser.IsOvertaking)
            {
                RoadUser blocker = FindRoadUserAhead(roadUser);

                if (blocker != null)
                {
                    int gap = GetForwardGap(roadUser, blocker);
                    int safeDistance = GetRoadBrakingDistance(roadUser);
                    int releaseDistance = safeDistance +
                        RoadFollowingReleaseBuffer;
                    bool blockerIsSlower = blocker.ActualSpeed < roadUser.DesiredSpeed ||
                        blocker.DesiredSpeed < roadUser.DesiredSpeed;

                    if (gap <= safeDistance)
                    {
                        if (blockerIsSlower && TryRegularRoadOvertake(roadUser))
                        {
                            MoveAtBestSpeed(roadUser);
                            return;
                        }

                        int followSpeed = blocker.ActualSpeed;

                        if (gap < safeDistance && followSpeed > 0)
                            followSpeed--;

                        MoveAtBestSpeed(roadUser, followSpeed);
                        return;
                    }

                    if (gap <= releaseDistance)
                    {
                        MoveAtBestSpeed(roadUser, blocker.ActualSpeed);
                        return;
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

            if (emergency.IsChangingLane)
            {
                MoveAtBestSpeed(emergency);
                return;
            }

            RoadUser blocker = FindRoadUserAhead(emergency);

            if (blocker != null)
            {
                int gap = GetForwardGap(emergency, blocker);

                if (gap <= GetRoadBrakingDistance(emergency))
                {
                    if (blocker != manualObject)
                    {
                        if (TryEmergencyRoadLaneChange(blocker))
                        {
                            blocker.IsOvertaking = false;
                            blocker.OvertakeReturnLane = -1;
                        }
                    }

                    int emergencySpeed = blocker.ActualSpeed;

                    if (gap < GetRoadBrakingDistance(emergency) &&
                        emergencySpeed > 0)
                        emergencySpeed--;

                    MoveAtBestSpeed(emergency, emergencySpeed);
                    return;
                }
            }

            MoveAtBestSpeed(emergency);
        }

        private int GetRoadBrakingDistance(RoadUser roadUser)
        {
            return 12 + roadUser.DesiredSpeed * 8;
        }

        private bool TryMaintainRoadFollowingDistance(RoadUser roadUser)
        {
            RoadUser blocker = FindRoadUserAhead(roadUser);

            if (blocker == null)
                return false;

            int gap = GetForwardGap(roadUser, blocker);
            int safeDistance = GetRoadBrakingDistance(roadUser);
            int releaseDistance = safeDistance + RoadFollowingReleaseBuffer;

            if (gap > releaseDistance)
                return false;

            int followSpeed = blocker.ActualSpeed;

            if (gap < safeDistance && followSpeed > 0)
                followSpeed--;

            MoveAtBestSpeed(roadUser, followSpeed);
            return true;
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

        private RoadUser FindRoadUserBehindInLane(RoadUser roadUser, int lane)
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

                int gap = GetRearGap(roadUser, current);

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

            if (!IsRoadTargetLaneSafe(roadUser, targetLane))
                return false;

            int targetY = GetRoadLaneTop(targetLane) +
                (RoadLaneHeight - roadUser.Height) / 2;
            Rectangle laneChangeBounds = GetLaneChangeBounds(roadUser,
                targetY, RoadLaneChangePadding, 2);

            if (!IsMotionAreaFree(laneChangeBounds, roadUser))
                return false;

            roadUser.Lane = targetLane;
            roadUser.LaneChangeTargetY = targetY;
            roadUser.IsChangingLane = roadUser.Y != targetY;
            return true;
        }

        private bool IsRoadTargetLaneSafe(RoadUser roadUser, int targetLane)
        {
            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == roadUser || !(current is RoadUser))
                    continue;

                if (current.Lane != targetLane ||
                    current.Direction != roadUser.Direction)
                    continue;

                RoadUser other = (RoadUser)current;
                int forwardGap = GetForwardGap(roadUser, other);
                int rearGap = GetRearGap(roadUser, other);

                if (forwardGap != int.MaxValue &&
                    forwardGap <= GetRoadBrakingDistance(roadUser))
                    return false;

                if (rearGap != int.MaxValue &&
                    rearGap <= RoadMergeRearClearance)
                    return false;
            }

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

            if (!IsMotionAreaFree(nextBounds, roadUser))
                return;

            roadUser.Y = nextY;

            if (roadUser.Y == roadUser.LaneChangeTargetY)
                roadUser.IsChangingLane = false;
        }

        private void UpdateBicycle(Bicycle bicycle)
        {
            InitializeBicycleMotionIfNeeded(bicycle);

            if (bicycle.IsChangingLane)
            {
                if (!TryMaintainBicycleFollowingDistance(bicycle))
                    MoveAtBestSpeed(bicycle);

                return;
            }

            int correctLane = GetBicycleCorrectLane(bicycle);

            if (bicycle.Lane != correctLane)
            {
                Bicycle oncoming = FindBicycleAhead(bicycle, false);
                bool oncomingIsClose = oncoming != null &&
                    GetForwardGap(bicycle, oncoming) <= BikeHeadOnDistance;

                Bicycle passedBicycle = FindBicycleBehindInLane(bicycle,
                    correctLane, true);
                Bicycle bicycleAheadInCorrectLane = FindBicycleAheadInLane(
                    bicycle, correctLane, true);

                bool rearClear = passedBicycle == null ||
                    GetRearGap(bicycle, passedBicycle) >= BikePassClearance;
                bool frontClear = bicycleAheadInCorrectLane == null ||
                    GetForwardGap(bicycle, bicycleAheadInCorrectLane) >
                    BikeFollowingDistance;

                if (rearClear && frontClear &&
                    IsBikeReturnLaneSafe(bicycle, correctLane,
                        BikePassClearance) &&
                    TryMoveBicycleToLane(bicycle, correctLane))
                {
                    MoveAtBestSpeed(bicycle);
                    return;
                }

                if (oncomingIsClose)
                {
                    if (IsBikeReturnLaneSafe(bicycle, correctLane,
                        BikeFollowingDistance) &&
                        TryMoveBicycleToLane(bicycle, correctLane))
                    {
                        MoveAtBestSpeed(bicycle);
                        return;
                    }

                    MoveAtBestSpeed(bicycle, 0);
                    return;
                }

                if (TryMaintainBicycleFollowingDistance(bicycle))
                    return;

                MoveAtBestSpeed(bicycle);
                return;
            }

            Bicycle blocker = FindBicycleAhead(bicycle, true);

            if (blocker != null)
            {
                int gap = GetForwardGap(bicycle, blocker);
                bool blockerIsSlower = blocker.ActualSpeed < bicycle.DesiredSpeed ||
                    blocker.DesiredSpeed < bicycle.DesiredSpeed;

                if (gap <= BikeFollowingDistance)
                {
                    if (blockerIsSlower)
                    {
                        int passingLane;

                        if (correctLane == 6)
                            passingLane = 7;
                        else
                            passingLane = 6;

                        if (!IsNearbyBikeOvertakeActive(bicycle) &&
                            IsBikePassingLaneSafe(bicycle, passingLane) &&
                            TryMoveBicycleToLane(bicycle, passingLane))
                        {
                            MoveAtBestSpeed(bicycle);
                            return;
                        }
                    }

                    int followSpeed = blocker.ActualSpeed;

                    if (gap < BikeFollowingDistance && followSpeed > 0)
                        followSpeed--;

                    MoveAtBestSpeed(bicycle, followSpeed);
                    return;
                }
            }

            Bicycle wrongWayBicycle = FindBicycleAhead(bicycle, false);

            if (wrongWayBicycle != null &&
                GetForwardGap(bicycle, wrongWayBicycle) <=
                BikeHeadOnDistance)
            {
                MoveAtBestSpeed(bicycle, 0);
                return;
            }

            MoveAtBestSpeed(bicycle);
        }

        private int GetBicycleCorrectLane(Bicycle bicycle)
        {
            if (bicycle.Direction == TravelDirection.Left)
                return 6;

            return 7;
        }

        private bool TryMaintainBicycleFollowingDistance(Bicycle bicycle)
        {
            Bicycle blocker = FindBicycleAhead(bicycle, true);

            if (blocker == null)
                return false;

            int gap = GetForwardGap(bicycle, blocker);

            if (gap > BikeFollowingDistance)
                return false;

            int followSpeed = blocker.ActualSpeed;

            if (gap < BikeFollowingDistance && followSpeed > 0)
                followSpeed--;

            MoveAtBestSpeed(bicycle, followSpeed);
            return true;
        }

        private bool IsNearbyBikeOvertakeActive(Bicycle bicycle)
        {
            int bicycleCenter = bicycle.X + bicycle.Width / 2;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == bicycle || !(current is Bicycle))
                    continue;

                Bicycle other = (Bicycle)current;
                int otherCorrectLane = GetBicycleCorrectLane(other);

                if (!other.IsChangingLane && other.Lane == otherCorrectLane)
                    continue;

                int otherCenter = other.X + other.Width / 2;

                if (Math.Abs(otherCenter - bicycleCenter) <
                    BikeOvertakeSectionDistance)
                    return true;
            }

            return false;
        }

        private bool IsBikeReturnLaneSafe(Bicycle bicycle, int targetLane,
            int rearClearance)
        {
            int bicycleCenter = bicycle.X + bicycle.Width / 2;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == bicycle || !(current is Bicycle))
                    continue;

                if (current.Lane != targetLane)
                    continue;

                Bicycle other = (Bicycle)current;

                if (other.Direction == bicycle.Direction)
                {
                    int forwardGap = GetForwardGap(bicycle, other);
                    int rearGap = GetRearGap(bicycle, other);

                    if (forwardGap != int.MaxValue &&
                        forwardGap <= BikeFollowingDistance)
                        return false;

                    if (rearGap != int.MaxValue &&
                        rearGap < rearClearance)
                        return false;
                }
                else
                {
                    int otherCenter = other.X + other.Width / 2;

                    if (Math.Abs(otherCenter - bicycleCenter) <
                        BikeOncomingClearance)
                        return false;
                }
            }

            return true;
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

        private Bicycle FindBicycleBehindInLane(Bicycle bicycle, int lane,
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

                int gap = GetRearGap(bicycle, current);

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
            Rectangle laneChangeBounds = GetLaneChangeBounds(bicycle,
                targetY, BikeLaneChangePadding, 1);

            if (!IsMotionAreaFree(laneChangeBounds, bicycle))
                return false;

            int bicycleCenter = bicycle.X + bicycle.Width / 2;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (!(current is Bicycle) || current == bicycle)
                    continue;

                if (current.Lane != targetLane)
                    continue;

                int currentCenter = current.X + current.Width / 2;
                int centerDistance = Math.Abs(currentCenter - bicycleCenter);

                if (current.Direction != bicycle.Direction)
                {
                    if (centerDistance < BikeOncomingClearance)
                        return false;
                }
                else if (centerDistance < BikePassClearance +
                    bicycle.Width + current.Width)
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryMoveBicycleToLane(Bicycle bicycle, int targetLane)
        {
            if (bicycle.IsChangingLane)
                return false;

            if (targetLane < 6 || targetLane > 7)
                return false;

            InitializeBicycleMotionIfNeeded(bicycle);

            int targetY = BikeTop + (targetLane - 6) * BikeLaneHeight +
                (BikeLaneHeight - bicycle.Height) / 2;
            Rectangle laneChangeBounds = GetLaneChangeBounds(bicycle,
                targetY, BikeLaneChangePadding, 1);

            if (!IsMotionAreaFree(laneChangeBounds, bicycle))
                return false;

            bicycle.PreviousMotionY = bicycle.Y;
            bicycle.Lane = targetLane;
            bicycle.LaneChangeTargetLane = targetLane;
            bicycle.LaneChangeTargetY = targetY;
            bicycle.IsChangingLane = bicycle.Y != targetY;
            return true;
        }

        private void UpdatePedestrian(Pedestrian pedestrian)
        {
            InitializePedestrianMotionIfNeeded(pedestrian);

            if (pedestrian.IsChangingLane)
            {
                if (!TryMaintainPedestrianFollowingDistance(pedestrian))
                    MoveAtBestSpeed(pedestrian);

                return;
            }

            int keepRightLane = GetPedestrianKeepRightLane(pedestrian);

            if (pedestrian.Lane != keepRightLane)
            {
                Pedestrian passedPedestrian = FindPedestrianBehindInLane(
                    pedestrian, keepRightLane, true);
                Pedestrian pedestrianAheadInKeepRight =
                    FindPedestrianAheadInLane(pedestrian, keepRightLane,
                        true);

                bool rearClear = passedPedestrian == null ||
                    GetRearGap(pedestrian, passedPedestrian) >=
                    PedestrianPassClearance;
                bool frontClear = pedestrianAheadInKeepRight == null ||
                    GetForwardGap(pedestrian, pedestrianAheadInKeepRight) >
                    PedestrianFollowingDistance;

                if (rearClear && frontClear &&
                    TryMovePedestrianToLane(pedestrian, keepRightLane))
                {
                    MoveAtBestSpeed(pedestrian);
                    return;
                }
            }

            Pedestrian sameDirectionBlocker = FindPedestrianAheadInLane(
                pedestrian, pedestrian.Lane, true);

            if (sameDirectionBlocker != null)
            {
                int gap = GetForwardGap(pedestrian, sameDirectionBlocker);
                bool blockerIsSlower =
                    sameDirectionBlocker.ActualSpeed < pedestrian.DesiredSpeed ||
                    sameDirectionBlocker.DesiredSpeed < pedestrian.DesiredSpeed;

                if (gap <= PedestrianFollowingDistance)
                {
                    if (blockerIsSlower && pedestrian.Lane == keepRightLane &&
                        TryMovePedestrianToLane(pedestrian,
                            GetPairedPedestrianLane(pedestrian.Lane)))
                    {
                        MoveAtBestSpeed(pedestrian);
                        return;
                    }

                    int followSpeed = sameDirectionBlocker.ActualSpeed;

                    if (gap < PedestrianFollowingDistance &&
                        followSpeed > 0)
                        followSpeed--;

                    MoveAtBestSpeed(pedestrian, followSpeed);
                    return;
                }
            }

            Pedestrian oncoming = FindPedestrianAheadInLane(pedestrian,
                pedestrian.Lane, false);

            if (oncoming != null &&
                GetForwardGap(pedestrian, oncoming) <=
                PedestrianHeadOnDistance)
            {
                if (pedestrian.Lane != keepRightLane &&
                    TryMovePedestrianToLane(pedestrian, keepRightLane))
                {
                    MoveAtBestSpeed(pedestrian);
                    return;
                }

                MoveAtBestSpeed(pedestrian, 0);
                return;
            }

            MoveAtBestSpeed(pedestrian);
        }

        private bool TryMaintainPedestrianFollowingDistance(
            Pedestrian pedestrian)
        {
            Pedestrian blocker = FindPedestrianAheadInLane(pedestrian,
                pedestrian.Lane, true);

            if (blocker == null)
                return false;

            int gap = GetForwardGap(pedestrian, blocker);

            if (gap > PedestrianFollowingDistance)
                return false;

            int followSpeed = blocker.ActualSpeed;

            if (gap < PedestrianFollowingDistance && followSpeed > 0)
                followSpeed--;

            MoveAtBestSpeed(pedestrian, followSpeed);
            return true;
        }

        private Pedestrian FindPedestrianAhead(Pedestrian pedestrian)
        {
            Pedestrian sameDirection = FindPedestrianAheadInLane(pedestrian,
                pedestrian.Lane, true);

            if (sameDirection != null)
                return sameDirection;

            return FindPedestrianAheadInLane(pedestrian, pedestrian.Lane,
                false);
        }

        private Pedestrian FindPedestrianAheadInLane(Pedestrian pedestrian,
            int lane, bool sameDirection)
        {
            Pedestrian nearest = (Pedestrian)null;
            int nearestGap = int.MaxValue;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == pedestrian || !(current is Pedestrian))
                    continue;

                if (current.Lane != lane)
                    continue;

                if (sameDirection &&
                    current.Direction != pedestrian.Direction)
                    continue;

                if (!sameDirection &&
                    current.Direction == pedestrian.Direction)
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

        private Pedestrian FindPedestrianBehindInLane(Pedestrian pedestrian,
            int lane, bool sameDirection)
        {
            Pedestrian nearest = (Pedestrian)null;
            int nearestGap = int.MaxValue;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == pedestrian || !(current is Pedestrian))
                    continue;

                if (current.Lane != lane)
                    continue;

                if (sameDirection &&
                    current.Direction != pedestrian.Direction)
                    continue;

                if (!sameDirection &&
                    current.Direction == pedestrian.Direction)
                    continue;

                int gap = GetRearGap(pedestrian, current);

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

        private bool TryMovePedestrianToLane(Pedestrian pedestrian,
            int targetLane)
        {
            if (pedestrian.IsChangingLane)
                return false;

            if (targetLane < 8 || targetLane > 11)
                return false;

            if ((pedestrian.Lane <= 9 && targetLane >= 10) ||
                (pedestrian.Lane >= 10 && targetLane <= 9))
                return false;

            InitializePedestrianMotionIfNeeded(pedestrian);

            int targetY = GetPedestrianLaneTop(targetLane) +
                (PedestrianLaneHeight - pedestrian.Height) / 2;
            Rectangle laneChangeBounds = GetLaneChangeBounds(pedestrian,
                targetY, PedestrianLaneChangePadding, 1);

            if (!IsMotionAreaFree(laneChangeBounds, pedestrian))
                return false;

            int pedestrianCenter = pedestrian.X + pedestrian.Width / 2;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == pedestrian || !(current is Pedestrian))
                    continue;

                if (current.Lane != targetLane)
                    continue;

                int currentCenter = current.X + current.Width / 2;

                if (Math.Abs(currentCenter - pedestrianCenter) <
                    PedestrianLaneChangeClearance +
                    pedestrian.Width / 2 + current.Width / 2)
                    return false;
            }

            pedestrian.PreviousMotionY = pedestrian.Y;
            pedestrian.Lane = targetLane;
            pedestrian.LaneChangeTargetLane = targetLane;
            pedestrian.LaneChangeTargetY = targetY;
            pedestrian.IsChangingLane = pedestrian.Y != targetY;
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

        private int GetRearGap(TrafficObject obj, TrafficObject other)
        {
            int objCenter = obj.X + obj.Width / 2;
            int otherCenter = other.X + other.Width / 2;

            if (obj.Direction == TravelDirection.Right)
            {
                if (otherCenter >= objCenter)
                    return int.MaxValue;

                int gap = obj.X - (other.X + other.Width);

                if (gap < 0)
                    gap = 0;

                return gap;
            }
            else
            {
                if (otherCenter <= objCenter)
                    return int.MaxValue;

                int gap = other.X - (obj.X + obj.Width);

                if (gap < 0)
                    gap = 0;

                return gap;
            }
        }

        private Rectangle GetLaneChangeBounds(TrafficObject obj, int targetY,
            int paddingX, int paddingY)
        {
            int topY = Math.Min(obj.Y, targetY);
            int bottomY = Math.Max(obj.Y + obj.Height,
                targetY + obj.Height);

            Rectangle bounds = new Rectangle(obj.X, topY, obj.Width,
                bottomY - topY);
            bounds.Inflate(paddingX, paddingY);
            return bounds;
        }

        private bool TryGetActiveLaneChangeBounds(TrafficObject obj,
            out Rectangle bounds)
        {
            bounds = Rectangle.Empty;

            if (obj is RoadUser)
            {
                RoadUser roadUser = (RoadUser)obj;

                if (!roadUser.IsChangingLane)
                    return false;

                bounds = GetLaneChangeBounds(roadUser,
                    roadUser.LaneChangeTargetY, RoadLaneChangePadding, 2);
                return true;
            }

            if (obj is Bicycle)
            {
                Bicycle bicycle = (Bicycle)obj;

                if (!bicycle.IsChangingLane)
                    return false;

                bounds = GetLaneChangeBounds(bicycle,
                    bicycle.LaneChangeTargetY, BikeLaneChangePadding, 1);
                return true;
            }

            if (obj is Pedestrian)
            {
                Pedestrian pedestrian = (Pedestrian)obj;

                if (!pedestrian.IsChangingLane)
                    return false;

                bounds = GetLaneChangeBounds(pedestrian,
                    pedestrian.LaneChangeTargetY,
                    PedestrianLaneChangePadding, 1);
                return true;
            }

            return false;
        }

        private bool IsMotionAreaFree(Rectangle bounds,
            TrafficObject ignoredObject)
        {
            if (!trafficObjects.IsAreaFree(bounds, ignoredObject))
                return false;

            for (int i = 0; i < trafficObjects.Count; i++)
            {
                TrafficObject current = trafficObjects[i];

                if (current == ignoredObject)
                    continue;

                Rectangle laneChangeBounds;

                if (TryGetActiveLaneChangeBounds(current,
                    out laneChangeBounds) &&
                    laneChangeBounds.IntersectsWith(bounds))
                    return false;
            }

            return true;
        }

        private Rectangle GetMovementSafetyBounds(TrafficObject obj,
            int speed)
        {
            Rectangle laneChangeBounds;

            if (!TryGetActiveLaneChangeBounds(obj, out laneChangeBounds))
                return GetMovedBounds(obj, speed);

            if (obj.Direction == TravelDirection.Right)
                laneChangeBounds.Offset(speed, 0);
            else
                laneChangeBounds.Offset(-speed, 0);

            return laneChangeBounds;
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
                !IsMotionAreaFree(GetMovementSafetyBounds(obj,
                    movementSpeed), obj))
            {
                movementSpeed--;
            }

            if (obj is RoadUser && movementSpeed < plannedSpeed)
                ((RoadUser)obj).BrakeTickCounter = 0;

            obj.ActualSpeed = movementSpeed;

            if (obj.ActualSpeed > 0)
                obj.Move();
        }

        private int GetRoadUserSpeedForTick(RoadUser roadUser,
            int allowedSpeed)
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

        private void TryManualLaneChange(TrafficObject obj,
            int verticalDirection)
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

            if (IsMotionAreaFree(nextBounds, bicycle))
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
                !IsMotionAreaFree(GetMovementSafetyBounds(bicycle,
                    extraSpeed), bicycle))
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

            if (IsMotionAreaFree(nextBounds, pedestrian))
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

            if (pedestrian.MotionBrakeTickCounter >=
                PedestrianBrakeTickDelay)
            {
                pedestrian.MotionBrakeTickCounter = 0;
                smoothSpeed--;
            }

            if (smoothSpeed < targetSpeed)
                smoothSpeed = targetSpeed;

            int extraSpeed = smoothSpeed - targetSpeed;
            int plannedExtraSpeed = extraSpeed;

            while (extraSpeed > 0 &&
                !IsMotionAreaFree(GetMovementSafetyBounds(pedestrian,
                    extraSpeed), pedestrian))
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
