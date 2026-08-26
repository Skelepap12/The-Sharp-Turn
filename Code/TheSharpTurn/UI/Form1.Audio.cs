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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (manualObject == null || e.KeyCode != Keys.H)
                return;

            if (manualObject is EmergencyVehicle)
            {
                currentMovementSound = "";
                actionSoundPauseTicks = 0;
                return;
            }

            if (manualObject is Bicycle)
                PlayActionSound("bike_horn.wav");
            else if (manualObject is Pedestrian)
                PlayActionSound("pedestrian_shout.wav");
            else if (manualObject is RoadUser)
                PlayActionSound("car_horn.wav");
        }

        private void AudioTimer_Tick(object sender, EventArgs e)
        {
            if (actionSoundPauseTicks > 0)
            {
                actionSoundPauseTicks--;
                return;
            }

            if (manualObject == null || manualObject.ActualSpeed == 0)
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

                return "engine_loop.wav";
            }

            if (obj is RoadUser)
                return "engine_loop.wav";

            if (obj is Bicycle)
                return "bike_loop.wav";

            if (obj is Pedestrian)
                return "footsteps.wav";

            return "";
        }

        private void PlayLoopingSound(string fileName)
        {
            string filePath = GetAudioPath(fileName);

            if (filePath == "")
                return;

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
                currentMovementSound = "";
            }
        }

        private void PlayActionSound(string fileName)
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
                actionSoundPauseTicks = 15;
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
