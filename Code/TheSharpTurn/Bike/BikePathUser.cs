using System;

namespace TheSharpTurn
{
    [Serializable]
    public abstract class BikePathUser : TrafficObject
    {
        public BikePathUser()
            : base()
        { }

        public BikePathUser(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            int actualSpeedVal, int widthVal, int heightVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  actualSpeedVal, widthVal, heightVal)
        { }

        public override int MaximumSpeed
        {
            get
            {
                return 3;
            }
        }
    }
}
