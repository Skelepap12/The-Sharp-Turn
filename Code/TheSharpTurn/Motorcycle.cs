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
            Brush bodyBrush = Brushes.OrangeRed;

            if (Model == MotorcycleModel.Chopper)
                bodyBrush = Brushes.SaddleBrown;

            int wheelSize = Height - 2;

            g.DrawEllipse(Pens.Black, X, Y + 1, wheelSize, wheelSize);
            g.DrawEllipse(Pens.Black, X + Width - wheelSize, Y + 1,
                wheelSize, wheelSize);

            g.FillRectangle(bodyBrush, X + 7, Y + 3, Width - 14, Height - 6);
            g.DrawRectangle(Pens.Black, X + 7, Y + 3,
                Width - 14, Height - 6);
        }

        public override string ToString()
        {
            return "Motorcycle - " + Model.ToString();
        }
    }
}
