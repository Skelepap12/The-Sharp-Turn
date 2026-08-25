using System;
using System.Drawing;

namespace TheSharpTurn
{
    [Serializable]
    public class Bus : RoadUser
    {
        BusModel model;

        public Bus()
            : this(0, 0, 0, TravelDirection.Right, 0, BusModel.Regular)
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
            }
        }

        public override void Draw(Graphics g)
        {
            Brush bodyBrush = Brushes.Goldenrod;

            if (Model == BusModel.Articulated)
                bodyBrush = Brushes.DarkOrange;

            g.FillRectangle(bodyBrush, Bounds);
            g.DrawRectangle(Pens.Black, Bounds);

            for (int i = 6; i < Width - 6; i += 12)
                g.FillRectangle(Brushes.LightBlue, X + i, Y + 3, 7, 6);

            if (Model == BusModel.Articulated)
                g.DrawLine(Pens.Black, X + Width / 2, Y,
                    X + Width / 2, Y + Height);
        }

        public override string ToString()
        {
            return "Bus - " + Model.ToString();
        }
    }
}
