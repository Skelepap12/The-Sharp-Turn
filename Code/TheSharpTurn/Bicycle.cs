using System;
using System.Drawing;

namespace TheSharpTurn
{
    [Serializable]
    public class Bicycle : BikePathUser
    {
        BicycleModel model;

        public Bicycle()
            : this(0, 0, 0, TravelDirection.Right, 0, BicycleModel.Cruiser)
        { }

        public Bicycle(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            BicycleModel modelVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  desiredSpeedVal, 26, 12)
        {
            Model = modelVal;
        }

        public BicycleModel Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
            }
        }

        public override void Draw(Graphics g)
        {
            Pen framePen = Pens.Sienna;

            switch (Model)
            {
                case BicycleModel.BMX:
                    framePen = Pens.DarkSlateBlue;
                    break;

                case BicycleModel.MountainBike:
                    framePen = Pens.ForestGreen;
                    break;
            }

            int wheelSize = Height - 2;
            int leftCenterX = X + wheelSize / 2;
            int rightCenterX = X + Width - wheelSize / 2;
            int centerY = Y + Height / 2;

            g.DrawEllipse(Pens.Black, X, Y + 1, wheelSize, wheelSize);
            g.DrawEllipse(Pens.Black, X + Width - wheelSize, Y + 1,
                wheelSize, wheelSize);

            g.DrawLine(framePen, leftCenterX, centerY,
                X + Width / 2, Y + 2);
            g.DrawLine(framePen, X + Width / 2, Y + 2,
                rightCenterX, centerY);
            g.DrawLine(framePen, leftCenterX, centerY,
                rightCenterX, centerY);
        }

        public override string ToString()
        {
            return "Bicycle - " + Model.ToString();
        }
    }
}
