using System;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace TheSharpTurn
{
    public partial class Form1
    {
        SoundPlayer manualSoundPlayer = new SoundPlayer();
        string currentMovementSound = "";
        int actionSoundPauseTicks = 0;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            simulationTimer.Tick += new EventHandler(AudioTimer_Tick);
            buttonManual.Click += new EventHandler(AudioManualButton_Click);
            pictureBoxMap.MouseDown += new MouseEventHandler(AudioMap_MouseDown);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopManualSound();
            base.OnFormClosed(e);
        }

        private void HandleManualAudioAction()
        {
            if (manualObject == null)
                return;

            if (manualObject is EmergencyVehicle)
            {
                EmergencyVehicle emergency = (EmergencyVehicle)manualObject;
                emergency.SirenOn = !emergency.SirenOn;
                currentMovementSound = "";
                actionSoundPauseTicks = 0;

                if (emergency.SirenOn)
                    labelStatus.Text = "Emergency siren ON.";
                else
                    labelStatus.Text = "Emergency siren OFF.";

                return;
            }

            if (manualObject is Bicycle)
            {
                PlayActionSound("bike_horn.wav", 45);
                labelStatus.Text = "Bicycle horn.";
            }
            else if (manualObject is Pedestrian)
            {
                Pedestrian pedestrian = (Pedestrian)manualObject;

                if (pedestrian.Model == PedestrianModel.Female)
                    PlayActionSound("pedestrian_female_shout.wav", 32);
                else
                    PlayActionSound("pedestrian_shout.wav", 15);

                labelStatus.Text = "Pedestrian shout.";
            }
            else if (manualObject is RoadUser)
            {
                PlayActionSound("car_horn.wav", 55);
                labelStatus.Text = "Horn sounded.";
            }
        }

        private void AudioTimer_Tick(object sender, EventArgs e)
        {
            if (actionSoundPauseTicks > 0)
            {
                actionSoundPauseTicks--;
                return;
            }

            if (manualObject == null)
            {
                StopManualSound();
                return;
            }

            string soundFile = GetMovementSoundFile(manualObject);

            if (soundFile == "")
            {
                StopManualSound();
                return;
            }

            if (currentMovementSound != soundFile)
                PlayLoopingSound(soundFile);
        }

        private void AudioManualButton_Click(object sender, EventArgs e)
        {
            if (manualObject == null)
                StopManualSound();
            else
                currentMovementSound = "";
        }

        private void AudioMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (manualObject == null)
                StopManualSound();
        }

        private string GetMovementSoundFile(TrafficObject obj)
        {
            if (obj is EmergencyVehicle)
            {
                EmergencyVehicle emergency = (EmergencyVehicle)obj;

                if (emergency.SirenOn)
                    return "siren.wav";

                return GetEmergencyEngineSoundFile(emergency);
            }

            if (obj is Car)
                return GetCarSoundFile(obj.ActualSpeed);

            if (obj is Bus)
                return GetBusSoundFile(obj.ActualSpeed);

            if (obj is Motorcycle)
                return GetMotorcycleSoundFile(obj.ActualSpeed);

            if (obj is Bicycle)
                return GetBicycleSoundFile(obj.ActualSpeed);

            if (obj is Pedestrian)
                return GetPedestrianSoundFile(obj.ActualSpeed);

            return "";
        }

        private string GetCarSoundFile(int speed)
        {
            switch (speed)
            {
                case 0:
                    return "car_idle.wav";
                case 1:
                case 2:
                    return "car_low.wav";
                case 3:
                case 4:
                    return "car_mid.wav";
                case 5:
                    return "car_high.wav";
                default:
                    return "car_high.wav";
            }
        }

        private string GetMotorcycleSoundFile(int speed)
        {
            switch (speed)
            {
                case 0:
                    return "motorcycle_idle.wav";
                case 1:
                case 2:
                    return "motorcycle_low.wav";
                case 3:
                case 4:
                    return "motorcycle_mid.wav";
                case 5:
                    return "motorcycle_high.wav";
                default:
                    return "motorcycle_high.wav";
            }
        }

        private string GetBusSoundFile(int speed)
        {
            switch (speed)
            {
                case 0:
                    return "bus_idle.wav";
                case 1:
                case 2:
                    return "bus_low.wav";
                case 3:
                case 4:
                    return "bus_mid.wav";
                case 5:
                    return "bus_high.wav";
                default:
                    return "bus_high.wav";
            }
        }

        private string GetEmergencyEngineSoundFile(EmergencyVehicle emergency)
        {
            switch (emergency.Model)
            {
                case EmergencyVehicleModel.PoliceCar:
                    return GetCarSoundFile(emergency.ActualSpeed);

                case EmergencyVehicleModel.FireTruck:
                    if (emergency.ActualSpeed == 0)
                        return "firetruck_idle.wav";
                    return GetBusSoundFile(emergency.ActualSpeed);

                case EmergencyVehicleModel.Ambulance:
                    return GetBusSoundFile(emergency.ActualSpeed);
            }

            return GetBusSoundFile(emergency.ActualSpeed);
        }

        private string GetBicycleSoundFile(int speed)
        {
            switch (speed)
            {
                case 0:
                    return "";
                case 1:
                    return "bike_low.wav";
                case 2:
                    return "bike_mid.wav";
                case 3:
                    return "bike_high.wav";
                default:
                    return "bike_high.wav";
            }
        }

        private string GetPedestrianSoundFile(int speed)
        {
            switch (speed)
            {
                case 0:
                    return "";
                case 1:
                    return "walk_normal.wav";
                case 2:
                    return "walk_fast.wav";
                default:
                    return "walk_fast.wav";
            }
        }

        private void PlayLoopingSound(string fileName)
        {
            string filePath = GetAudioPath(fileName);

            if (filePath == "")
            {
                StopManualSound();
                return;
            }

            try
            {
                manualSoundPlayer.Stop();
                manualSoundPlayer.SoundLocation = filePath;
                manualSoundPlayer.Load();
                manualSoundPlayer.PlayLooping();
                currentMovementSound = fileName;
            }
            catch
            {
                manualSoundPlayer.Stop();
                currentMovementSound = fileName;
            }
        }

        private void PlayActionSound(string fileName, int pauseTicks)
        {
            string filePath = GetAudioPath(fileName);

            if (filePath == "")
                return;

            try
            {
                manualSoundPlayer.Stop();
                manualSoundPlayer.SoundLocation = filePath;
                manualSoundPlayer.Load();
                manualSoundPlayer.Play();
                currentMovementSound = "";
                actionSoundPauseTicks = pauseTicks;
            }
            catch
            {
                currentMovementSound = "";
                actionSoundPauseTicks = 0;
            }
        }

        private void StopManualSound()
        {
            manualSoundPlayer.Stop();
            currentMovementSound = "";
            actionSoundPauseTicks = 0;
        }

        private string GetAudioPath(string fileName)
        {
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Audio", fileName);

            if (File.Exists(outputPath))
                return outputPath;

            string repositoryPath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "Assets", "Audio", fileName));

            if (File.Exists(repositoryPath))
                return repositoryPath;

            return "";
        }
    }
}
