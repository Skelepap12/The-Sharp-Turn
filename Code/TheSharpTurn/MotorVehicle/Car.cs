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

                switch (model)
                {
                    case CarModel.Hatchback:
                        Width = 34;
                        Height = 18;
                        break;

                    case CarModel.SportsCar:
                        Width = 38;
                        Height = 16;
                        break;

                    case CarModel.Sedan:
                        Width = 42;
                        Height = 18;
                        break;
                }
            }
        }

        public override void Draw(Graphics g)
        {
            string spritePath = "Cars/Sedan_Right.png";

            switch (Model)
            {
                case CarModel.SportsCar:
                    spritePath = "Cars/SportsCar_Right.png";
                    break;

                case CarModel.Hatchback:
                    spritePath = "Cars/Hatchback_Right.png";
                    break;
            }

            if (SpriteLibrary.Draw(g, spritePath, Bounds, Direction))
                return;

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
        }

        public override string ToString()
        {
            return "Car - " + Model.ToString();
        }
    }
}
