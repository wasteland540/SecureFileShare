using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SecureFileShare.Util.FileIO
{
    public class BinarySerializer
    {
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
                    //TODO: loggen!!!!
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
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
                    //TODO: loggen!!!!
                    Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
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