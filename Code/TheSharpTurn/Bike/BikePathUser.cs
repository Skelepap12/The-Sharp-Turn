using System;

namespace TheSharpTurn
{
    [Serializable]
    public abstract class BikePathUser : TrafficObject
    {
        int motionSpeed;
        int motionBrakeTickCounter;
        bool motionInitialized;
        int previousMotionY;
        bool isChangingLane;
        int laneChangeTargetY;
        int laneChangeTargetLane;

        public BikePathUser()
            : base()
        {
            InitializeMotionState();
        }

        public BikePathUser(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            int actualSpeedVal, int widthVal, int heightVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  actualSpeedVal, widthVal, heightVal)
        {
            InitializeMotionState();
        }

        private void InitializeMotionState()
        {
            MotionSpeed = ActualSpeed;
            MotionBrakeTickCounter = 0;
            MotionInitialized = false;
            PreviousMotionY = Y;
            IsChangingLane = false;
            LaneChangeTargetY = Y;
            LaneChangeTargetLane = Lane;
        }

        public int MotionSpeed
        {
            get
            {
                return motionSpeed;
            }
            set
            {
                if (value < 0)
                    motionSpeed = 0;
                else
                    motionSpeed = value;
            }
        }

        public int MotionBrakeTickCounter
        {
            get
            {
                return motionBrakeTickCounter;
            }
            set
            {
                if (value < 0)
                    motionBrakeTickCounter = 0;
                else
                    motionBrakeTickCounter = value;
            }
        }

        public bool MotionInitialized
        {
            get
            {
                return motionInitialized;
            }
            set
            {
                motionInitialized = value;
            }
        }

        public int PreviousMotionY
        {
            get
            {
                return previousMotionY;
            }
            set
            {
                previousMotionY = value;
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

        public int LaneChangeTargetLane
        {
            get
            {
                return laneChangeTargetLane;
            }
            set
            {
                laneChangeTargetLane = value;
            }
        }
    }
}
