using System;
using System.Drawing;

namespace TheSharpTurn
{
    [Serializable]
    public class Pedestrian : SidewalkUser
    {
        PedestrianModel model;

        public Pedestrian()
            : this(0, 0, 0, TravelDirection.Right, 0,
                  PedestrianModel.Male)
        { }

        public Pedestrian(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            PedestrianModel modelVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  desiredSpeedVal, 12, 20)
        {
            Model = modelVal;
        }

        public PedestrianModel Model
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

            if (Model == PedestrianModel.Female)
                bodyBrush = Brushes.MediumPurple;

            int headSize = 8;
            int headX = X + (Width - headSize) / 2;

            g.FillEllipse(Brushes.PeachPuff, headX, Y, headSize, headSize);
            g.DrawEllipse(Pens.Black, headX, Y, headSize, headSize);

            g.FillRectangle(bodyBrush, X + 2, Y + headSize,
                Width - 4, Height - headSize);
            g.DrawRectangle(Pens.Black, X + 2, Y + headSize,
                Width - 4, Height - headSize);
        }

        public override string ToString()
        {
            return "Pedestrian - " + Model.ToString();
        }
    }
}
