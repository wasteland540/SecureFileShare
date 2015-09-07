using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;

namespace SecureFileShare.Util.FileIO
{
    public class BinarySerializer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (BinarySerializer));

        public static void Serialize(string filePath, object obj)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                var binaryFormatter = new BinaryFormatter();

                try
                {
                    binaryFormatter.Serialize(fileStream, obj);
                }
                catch (SerializationException e)
                {
                    Logger.Error("Failed to serialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fileStream.Close();
                }
            }
        }

        public static T Deserialize<T>(string filePath)
        {
            T obj;

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var binaryFormatter = new BinaryFormatter();

                try
                {
                    obj = (T) binaryFormatter.Deserialize(fileStream);
                }
                catch (SerializationException e)
                {
                    Logger.Error("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fileStream.Close();
                }
            }

            return obj;
        }
    }
}