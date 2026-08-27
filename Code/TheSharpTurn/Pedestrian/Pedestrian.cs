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
            string spritePath = "Pedestrians/Male_Right.png";

            if (Model == PedestrianModel.Female)
                spritePath = "Pedestrians/Female_Right.png";

            if (SpriteLibrary.Draw(g, spritePath, Bounds, Direction))
                return;

            Brush bodyBrush = Brushes.SteelBlue;

            if (Model == PedestrianModel.Female)
                bodyBrush = Brushes.MediumPurple;

            g.FillRectangle(bodyBrush, Bounds);
            g.DrawRectangle(Pens.Black, Bounds);
        }

        public override string ToString()
        {
            return "Pedestrian - " + Model.ToString();
        }
    }
}
