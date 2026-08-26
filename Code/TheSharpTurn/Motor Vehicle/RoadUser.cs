using System;

namespace TheSharpTurn
{
    [Serializable]
    public abstract class RoadUser : TrafficObject
    {
        int overtakeReturnLane;
        bool isOvertaking;

        public RoadUser()
            : base()
        {
            OvertakeReturnLane = -1;
            IsOvertaking = false;
        }

        public RoadUser(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            int actualSpeedVal, int widthVal, int heightVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  actualSpeedVal, widthVal, heightVal)
        {
            OvertakeReturnLane = -1;
            IsOvertaking = false;
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

        public override int MaximumSpeed
        {
            get
            {
                return 5;
            }
        }
    }
}
