using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureFileShare.Model;
using SecureFileShare.Util.FileIO;

namespace SecureFileShareUnitTests.Util.FileIO
{
    [TestClass]
    public class BinarySerializerUnitTest
    {
        [TestMethod]
        public void TestSerialize()
        {
            const string filePath = "BinarySerializerTest.sfs";
            
            var objToSerialize = new EncryptedFile
            {
                Data = new byte[] { 1, 2 },
                FileExtension = new byte[] { 3, 4},
                Filename = "forInternalUse.sfs",
                Hmac = new byte[] { 5, 6},
                Iv = new byte[] { 7, 8},
                SessionKey = new byte[] { 9, 10}
            };

            BinarySerializer.Serialize(filePath, objToSerialize);
            Assert.IsTrue(File.Exists(filePath));

            //clean up file system
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [TestMethod]
        public void TestDeserialize()
        {
            const string filePath = "BinarySerializerTest.sfs";

            var objToSerialize = new EncryptedFile
            {
                Data = new byte[] { 1, 2 },
                FileExtension = new byte[] { 3, 4 },
                Filename = "forInternalUse.sfs",
                Hmac = new byte[] { 5, 6 },
                Iv = new byte[] { 7, 8 },
                SessionKey = new byte[] { 9, 10 }
            };

            BinarySerializer.Serialize(filePath, objToSerialize);
            Assert.IsTrue(File.Exists(filePath));

            var deserializedObj = BinarySerializer.Deserialize<EncryptedFile>(filePath);
            Assert.IsTrue(deserializedObj.Filename == objToSerialize.Filename);
            Assert.IsTrue(deserializedObj.Data[0] == objToSerialize.Data[0]);
            Assert.IsTrue(deserializedObj.Data[1] == objToSerialize.Data[1]);
            Assert.IsTrue(deserializedObj.FileExtension[0] == objToSerialize.FileExtension[0]);
            Assert.IsTrue(deserializedObj.FileExtension[1] == objToSerialize.FileExtension[1]);
            Assert.IsTrue(deserializedObj.Hmac[0] == objToSerialize.Hmac[0]);
            Assert.IsTrue(deserializedObj.Hmac[1] == objToSerialize.Hmac[1]);
            Assert.IsTrue(deserializedObj.Iv[0] == objToSerialize.Iv[0]);
            Assert.IsTrue(deserializedObj.Iv[1] == objToSerialize.Iv[1]);
            Assert.IsTrue(deserializedObj.SessionKey[0] == objToSerialize.SessionKey[0]);
            Assert.IsTrue(deserializedObj.SessionKey[1] == objToSerialize.SessionKey[1]);

            //clean up file system
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
