using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TheSharpTurn
{
    public static class TrafficObjectFile
    {
        public const string FileExtension = "tst";
        public const string FileFilter = "The Sharp Turn files (*.tst)|*.tst|All files (*.*)|*.*";

        public static void Save(string fileName, TrafficObjectList trafficObjects)
        {
            IFormatter formatter = new BinaryFormatter();

            using (Stream stream = new FileStream(fileName, FileMode.Create,
                FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(stream, trafficObjects);
            }
        }

        public static TrafficObjectList Load(string fileName)
        {
            IFormatter formatter = new BinaryFormatter();

            using (Stream stream = new FileStream(fileName, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                return (TrafficObjectList)formatter.Deserialize(stream);
            }
        }
    }
}
