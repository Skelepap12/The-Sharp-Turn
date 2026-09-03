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
                  desiredSpeedVal, 28, 12)
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

                switch (model)
                {
                    case BicycleModel.BMX:
                        Width = 18;
                        Height = 10;
                        break;

                    case BicycleModel.MountainBike:
                        Width = 24;
                        Height = 12;
                        break;

                    case BicycleModel.Cruiser:
                        Width = 28;
                        Height = 12;
                        break;
                }
            }
        }

        public override void Draw(Graphics g)
        {
            string spritePath = "Bicycles/Cruiser_Right.png";
            Rectangle drawBounds = new Rectangle(X - 4, Y - 3,
                Width + 8, Height + 6);

            switch (Model)
            {
                case BicycleModel.BMX:
                    spritePath = "Bicycles/BMX_Right.png";
                    break;

                case BicycleModel.MountainBike:
                    spritePath = "Bicycles/MountainBike_Right.png";
                    break;
            }

            if (SpriteLibrary.Draw(g, spritePath, drawBounds, Direction))
                return;

            g.DrawRectangle(Pens.Sienna, Bounds);
        }

        public override string ToString()
        {
            return "Bicycle - " + Model.ToString();
        }
    }
}
