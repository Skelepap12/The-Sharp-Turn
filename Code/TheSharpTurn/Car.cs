using System;
using System.Drawing;

namespace TheSharpTurn
{
    [Serializable]
    public class Car : RoadUser
    {
        CarModel model;

        public Car()
            : this(0, 0, 0, TravelDirection.Right, 0, CarModel.Sedan)
        { }

        public Car(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            CarModel modelVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  desiredSpeedVal, 38, 18)
        {
            Model = modelVal;
        }

        public CarModel Model
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
            Brush bodyBrush = Brushes.SteelBlue;

            switch (Model)
            {
                case CarModel.SportsCar:
                    bodyBrush = Brushes.Firebrick;
                    break;

                case CarModel.Hatchback:
                    bodyBrush = Brushes.SeaGreen;
                    break;
            }

            g.FillRectangle(bodyBrush, Bounds);
            g.DrawRectangle(Pens.Black, Bounds);

            int windowX;
            if (Direction == TravelDirection.Right)
                windowX = X + Width - 12;
            else
                windowX = X + 4;

            g.FillRectangle(Brushes.LightGray, windowX, Y + 4, 8, Height - 8);
        }

        public override string ToString()
        {
            return "Car - " + Model.ToString();
        }
    }
}
