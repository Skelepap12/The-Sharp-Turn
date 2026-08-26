using System;

namespace TheSharpTurn
{
    [Serializable]
    public abstract class RoadUser : TrafficObject
    {
        public RoadUser()
            : base()
        { }

        public RoadUser(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            int actualSpeedVal, int widthVal, int heightVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  actualSpeedVal, widthVal, heightVal)
        { }
    }
}
