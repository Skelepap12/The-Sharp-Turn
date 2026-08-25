using System;
using System.Drawing;

namespace TheSharpTurn
{
    [Serializable]
    public abstract class TrafficObject
    {
        int x;
        int y;
        int lane;
        TravelDirection direction;
        int desiredSpeed;
        int actualSpeed;
        int width;
        int height;

        public TrafficObject()
            : this(0, 0, 0, TravelDirection.Right, 0, 0, 1, 1)
        { }

        public TrafficObject(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            int actualSpeedVal, int widthVal, int heightVal)
        {
            X = xVal;
            Y = yVal;
            Lane = laneVal;
            Direction = directionVal;
            DesiredSpeed = desiredSpeedVal;
            ActualSpeed = actualSpeedVal;
            Width = widthVal;
            Height = heightVal;
        }

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public int Lane
        {
            get
            {
                return lane;
            }
            set
            {
                lane = value;
            }
        }

        public TravelDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }

        public int DesiredSpeed
        {
            get
            {
                return desiredSpeed;
            }
            set
            {
                desiredSpeed = value;
            }
        }

        public int ActualSpeed
        {
            get
            {
                return actualSpeed;
            }
            set
            {
                actualSpeed = value;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(X, Y, Width, Height);
            }
        }

        public virtual void Move()
        {
            if (Direction == TravelDirection.Right)
                X += ActualSpeed;
            else
                X -= ActualSpeed;
        }

        public virtual bool Contains(int xP, int yP)
        {
            return Bounds.Contains(xP, yP);
        }

        public abstract void Draw(Graphics g);
    }
}
