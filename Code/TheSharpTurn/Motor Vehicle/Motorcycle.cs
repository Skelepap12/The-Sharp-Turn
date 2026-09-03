using System;
using System.Drawing;

namespace TheSharpTurn
{
    [Serializable]
    public class Motorcycle : RoadUser
    {
        MotorcycleModel model;

        public Motorcycle()
            : this(0, 0, 0, TravelDirection.Right, 0,
                  MotorcycleModel.SportsBike)
        { }

        public Motorcycle(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            MotorcycleModel modelVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  desiredSpeedVal, 24, 10)
        {
            Model = modelVal;
        }

        public MotorcycleModel Model
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
                    case MotorcycleModel.SportsBike:
                        Width = 24;
                        Height = 10;
                        break;

                    case MotorcycleModel.Chopper:
                        Width = 30;
                        Height = 12;
                        break;
                }
            }
        }

        public override void Draw(Graphics g)
        {
            string spritePath = "Motorcycles/SportsBike_Right.png";

            if (Model == MotorcycleModel.Chopper)
                spritePath = "Motorcycles/Chopper_Right.png";

            TravelDirection spriteDirection = TravelDirection.Right;

            if (Direction == TravelDirection.Right)
                spriteDirection = TravelDirection.Left;

            if (SpriteLibrary.Draw(g, spritePath, Bounds, spriteDirection))
                return;

            Brush bodyBrush = Brushes.OrangeRed;

            if (Model == MotorcycleModel.Chopper)
                bodyBrush = Brushes.SaddleBrown;

            g.FillRectangle(bodyBrush, Bounds);
            g.DrawRectangle(Pens.Black, Bounds);
        }

        public override string ToString()
        {
            return "Motorcycle - " + Model.ToString();
        }
    }
}
