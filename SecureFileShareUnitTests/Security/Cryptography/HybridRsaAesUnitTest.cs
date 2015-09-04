using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureFileShare.Security.Cryptography;

namespace SecureFileShareUnitTests.Security.Cryptography
{
    /// <summary>
    ///     Summary description for HybridRsaAesUnitTest
    /// </summary>
    [TestClass]
    public class HybridRsaAesUnitTest
    {
        [TestMethod]
        public void TestFullrun()
        {
            const string messageToEncrypt = "Important important!";
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(messageToEncrypt);

            var hybridEncryper = new HybridRsaAes();
            hybridEncryper.AssignNewRSAKeys();
            RSAParameters publicKey = hybridEncryper.GetPublicRSAKey(); //public key receiver
            RSAParameters privateKey = hybridEncryper.GetPrivateRSAKey(); //private key receiver

            //Encrypt
            byte[] sessionKey = AbstractSecureCompareBase.GenerateRandomNumber(32); //256-bit
            byte[] iv = AbstractSecureCompareBase.GenerateRandomNumber(16); //128-bit

            byte[] encryptedData = hybridEncryper.EncryptAES(dataToEncrypt, sessionKey, iv);
            byte[] hmac = hybridEncryper.ComputeHmac(sessionKey, encryptedData);
            byte[] encryptedSessionKey = hybridEncryper.EncryptRSA(sessionKey, publicKey);

            //sending encrpyted file....(encryptedData, iv, hmac, encryptedSessionKey)

            //Decrypt
            byte[] decryptedSessionKey = hybridEncryper.DecryptRSA(encryptedSessionKey, privateKey);
            Assert.IsTrue(hybridEncryper.VerifyHmac(decryptedSessionKey, encryptedData, hmac));
            byte[] decryptedData = hybridEncryper.DecryptAES(encryptedData, decryptedSessionKey, iv);
            Assert.IsTrue(Encoding.UTF8.GetString(decryptedData) == messageToEncrypt);
        }

        [TestMethod]
        public void TestConvertPublicKey()
        {
            var hybridEncryper = new HybridRsaAes();
            hybridEncryper.AssignNewRSAKeys();

            string keyString = hybridEncryper.GetPublicRSAKeyAsXml();

            //sending public key string to friend....

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(keyString);

            Assert.IsTrue(hybridEncryper.GetPublicRSAKeyAsXml() == rsa.ToXmlString(false));
        }

        [TestMethod]
        public void TestFullrunWithKeyShare()
        {
            const string messageToEncrypt = "Important important!";
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(messageToEncrypt);

            var hybridEncryper = new HybridRsaAes();
            hybridEncryper.AssignNewRSAKeys();

            string keyString = hybridEncryper.GetPublicRSAKeyAsXml();

            //sending public key string to friend....

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(keyString);

            Assert.IsTrue(hybridEncryper.GetPublicRSAKeyAsXml() == rsa.ToXmlString(false));

            //Encrypt
            RSAParameters publicKey = rsa.ExportParameters(false); //public key receiver

            byte[] sessionKey = AbstractSecureCompareBase.GenerateRandomNumber(32); //256-bit
            byte[] iv = AbstractSecureCompareBase.GenerateRandomNumber(16); //128-bit

            byte[] encryptedData = hybridEncryper.EncryptAES(dataToEncrypt, sessionKey, iv);
            byte[] hmac = hybridEncryper.ComputeHmac(sessionKey, encryptedData);
            byte[] encryptedSessionKey = hybridEncryper.EncryptRSA(sessionKey, publicKey);

            //sending encrpyted file....(encryptedData, iv, hmac, encryptedSessionKey)

            //Decrypt
            RSAParameters privateKey = hybridEncryper.GetPrivateRSAKey(); //private key receiver

            byte[] decryptedSessionKey = hybridEncryper.DecryptRSA(encryptedSessionKey, privateKey);
            Assert.IsTrue(hybridEncryper.VerifyHmac(decryptedSessionKey, encryptedData, hmac));
            byte[] decryptedData = hybridEncryper.DecryptAES(encryptedData, decryptedSessionKey, iv);
            Assert.IsTrue(Encoding.UTF8.GetString(decryptedData) == messageToEncrypt);
        }
    }
}