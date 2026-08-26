using System;

namespace TheSharpTurn
{
    [Serializable]
    public abstract class RoadUser : TrafficObject
    {
        int overtakeReturnLane;
        bool isOvertaking;
        bool isChangingLane;
        int laneChangeTargetY;
        int brakeTickCounter;

        public RoadUser()
            : base()
        {
            OvertakeReturnLane = -1;
            IsOvertaking = false;
            IsChangingLane = false;
            LaneChangeTargetY = Y;
            BrakeTickCounter = 0;
        }

        public RoadUser(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            int actualSpeedVal, int widthVal, int heightVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  actualSpeedVal, widthVal, heightVal)
        {
            OvertakeReturnLane = -1;
            IsOvertaking = false;
            IsChangingLane = false;
            LaneChangeTargetY = Y;
            BrakeTickCounter = 0;
        }

        public int OvertakeReturnLane
        {
            get
            {
                return overtakeReturnLane;
            }
            set
            {
                overtakeReturnLane = value;
            }
        }

        public bool IsOvertaking
        {
            get
            {
                return isOvertaking;
            }
            set
            {
                isOvertaking = value;
            }
        }

        public bool IsChangingLane
        {
            get
            {
                return isChangingLane;
            }
            set
            {
                isChangingLane = value;
            }
        }

        public int LaneChangeTargetY
        {
            get
            {
                return laneChangeTargetY;
            }
            set
            {
                laneChangeTargetY = value;
            }
        }

        public int BrakeTickCounter
        {
            get
            {
                return brakeTickCounter;
            }
            set
            {
                if (value < 0)
                    brakeTickCounter = 0;
                else
                    brakeTickCounter = value;
            }
        }
    }
}
