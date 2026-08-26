using System;
using System.Drawing;

namespace TheSharpTurn
{
    [Serializable]
    public class EmergencyVehicle : RoadUser
    {
        EmergencyVehicleModel model;
        bool sirenOn;

        public EmergencyVehicle()
            : this(0, 0, 0, TravelDirection.Right, 0,
                  EmergencyVehicleModel.Ambulance)
        { }

        public EmergencyVehicle(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            EmergencyVehicleModel modelVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  desiredSpeedVal, 48, 20)
        {
            Model = modelVal;
            SirenOn = false;
        }

        public EmergencyVehicleModel Model
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
                    case EmergencyVehicleModel.PoliceCar:
                        Width = 38;
                        Height = 18;
                        break;

                    case EmergencyVehicleModel.Ambulance:
                        Width = 48;
                        Height = 20;
                        break;

                    case EmergencyVehicleModel.FireTruck:
                        Width = 62;
                        Height = 22;
                        break;
                }
            }
        }

        public bool SirenOn
        {
            get
            {
                return sirenOn;
            }
            set
            {
                sirenOn = value;
            }
        }

        public override void Draw(Graphics g)
        {
            Brush bodyBrush = Brushes.White;

            switch (Model)
            {
                case EmergencyVehicleModel.FireTruck:
                    bodyBrush = Brushes.Firebrick;
                    break;

                case EmergencyVehicleModel.PoliceCar:
                    bodyBrush = Brushes.LightSteelBlue;
                    break;
            }

            g.FillRectangle(bodyBrush, Bounds);
            g.DrawRectangle(Pens.Black, Bounds);

            Brush lightBrush;
            if (SirenOn)
                lightBrush = Brushes.Red;
            else
                lightBrush = Brushes.Gray;

            g.FillRectangle(lightBrush, X + Width / 2 - 5, Y + 2, 10, 4);
        }

        public override string ToString()
        {
            return "Emergency Vehicle - " + Model.ToString();
        }
    }
}
