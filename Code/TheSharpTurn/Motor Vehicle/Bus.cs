using System;
using System.Drawing;

namespace TheSharpTurn
{
    [Serializable]
    public class Bus : RoadUser
    {
        BusModel model;

        public Bus()
            : this(0, 0, 0, TravelDirection.Right, 0, BusModel.CityBus)
        { }

        public Bus(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            BusModel modelVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  desiredSpeedVal, 64, 20)
        {
            Model = modelVal;
        }

        public BusModel Model
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
                    case BusModel.CityBus:
                        Width = 64;
                        Height = 20;
                        break;

                    case BusModel.IntercityBus:
                        Width = 88;
                        Height = 20;
                        break;
                }
            }
        }

        public override void Draw(Graphics g)
        {
            if (SpriteLibrary.Draw(g, "Buses/Bus_Right.png", Bounds,
                Direction))
                return;

            Brush bodyBrush = Brushes.Goldenrod;

            if (Model == BusModel.IntercityBus)
                bodyBrush = Brushes.DarkOrange;

            g.FillRectangle(bodyBrush, Bounds);
            g.DrawRectangle(Pens.Black, Bounds);
        }

        public override string ToString()
        {
            return "Bus - " + Model.ToString();
        }
    }
}
