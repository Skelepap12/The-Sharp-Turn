using System;

namespace TheSharpTurn
{
    [Serializable]
    public abstract class SidewalkUser : TrafficObject
    {
        public SidewalkUser()
            : base()
        { }

        public SidewalkUser(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            int actualSpeedVal, int widthVal, int heightVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  actualSpeedVal, widthVal, heightVal)
        { }
    }
}
