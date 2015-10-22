using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureFileShare.App.Services;

namespace SecureFileShareUnitTests.App.Services
{
    [TestClass]
    public class CryptographyServiceUnitTest
    {
        private ICryptographyService _cryptographyService;

        [TestInitialize]
        public void SetupTest()
        {
            _cryptographyService = new CryptographyService();
        }

        [TestMethod]
        public void CompareTest()
        {
            var array1 = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5};
            var array2 = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5};

            bool result = _cryptographyService.Compare(array1, array2);
            Assert.IsTrue(result);

            var array3 = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5};
            var array4 = new byte[] {0x1, 0x2, 0x3, 0x4, 0x4};

            bool result2 = _cryptographyService.Compare(array3, array4);
            Assert.IsFalse(result2);

            var array5 = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5};
            var array6 = new byte[] {0x0, 0x2, 0x3, 0x4, 0x4};

            bool result3 = _cryptographyService.Compare(array5, array6);
            Assert.IsFalse(result3);
        }

        [TestMethod]
        public void GenerateSalt()
        {
            //128 bit
            byte[] salt = _cryptographyService.GenerateSalt();

            Assert.IsTrue(salt != null);
            Assert.IsTrue(salt.Length == 128);
        }

        [TestMethod]
        public void HashPassword()
        {
            const string password = "password1adsgffghgkjhlhhfpassword1adsgffghgkjhlhhf!!%&/(&/(%&/(%&/(%&/(";
            var salt = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8};

            byte[] hashedPassword = _cryptographyService.HashPassword(password, salt);

            Assert.IsTrue(hashedPassword != null);
            Assert.IsTrue(hashedPassword.Length == 128);

            const string password2 = "password1adsgffghgkjhlhhfpassword1adsgffghgkjhlhhf!!%&/(&/(%&/(%&/(%&/(";

            byte[] hashedPassword2 = _cryptographyService.HashPassword(password2, salt);

            Assert.IsTrue(hashedPassword2 != null);
            Assert.IsTrue(hashedPassword2.Length == 128);

            Assert.IsTrue(_cryptographyService.Compare(hashedPassword, hashedPassword2));
        }

        [TestMethod]
        public void AssignNewKeysTest()
        {
            _cryptographyService.AssignNewKeys();

            RSAParameters privateKey = _cryptographyService.GetPrivateKey();
            RSAParameters publicKey = _cryptographyService.GetPublicKey();

            _cryptographyService.AssignNewKeys();

            RSAParameters privateKey2 = _cryptographyService.GetPrivateKey();
            RSAParameters publicKey2 = _cryptographyService.GetPublicKey();

            Assert.IsFalse(Equals(privateKey, privateKey2));
            Assert.IsFalse(Equals(publicKey, publicKey2));
        }

        [TestMethod]
        public void GetPrivateKeyAsXml()
        {
            _cryptographyService.AssignNewKeys();

            RSAParameters privateKey = _cryptographyService.GetPrivateKey();
            string privateKeyAsXml = _cryptographyService.GetPrivateKeyAsXml();

            var rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(privateKeyAsXml);

            RSAParameters privateKey2 = rsa.ExportParameters(true);

            Assert.IsTrue(_cryptographyService.Compare(privateKey.D, privateKey2.D));
            Assert.IsTrue(_cryptographyService.Compare(privateKey.DP, privateKey2.DP));
            Assert.IsTrue(_cryptographyService.Compare(privateKey.DQ, privateKey2.DQ));
            Assert.IsTrue(_cryptographyService.Compare(privateKey.Exponent, privateKey2.Exponent));
            Assert.IsTrue(_cryptographyService.Compare(privateKey.InverseQ, privateKey2.InverseQ));
            Assert.IsTrue(_cryptographyService.Compare(privateKey.Modulus, privateKey2.Modulus));
            Assert.IsTrue(_cryptographyService.Compare(privateKey.P, privateKey2.P));
            Assert.IsTrue(_cryptographyService.Compare(privateKey.Q, privateKey2.Q));
        }

        [TestMethod]
        public void ExportPublicKeyFile()
        {
            string destinationFilename = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            _cryptographyService.AssignNewKeys();
            RSAParameters publicKey = _cryptographyService.GetPublicKey();

            _cryptographyService.ExportPublicKeyFile(destinationFilename, publicKey);

            Assert.IsTrue(File.Exists(destinationFilename));

            File.Delete(destinationFilename);
        }

        [TestMethod]
        public void ImportPublicKeyFile()
        {
            string destinationFilename = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            _cryptographyService.AssignNewKeys();
            RSAParameters publicKey = _cryptographyService.GetPublicKey();

            _cryptographyService.ExportPublicKeyFile(destinationFilename, publicKey);
            Assert.IsTrue(File.Exists(destinationFilename));

            RSAParameters publicKey2 = _cryptographyService.ImportPublicKeyFile(destinationFilename);

            Assert.IsTrue(_cryptographyService.Compare(publicKey.Exponent, publicKey2.Exponent));
            Assert.IsTrue(_cryptographyService.Compare(publicKey.Modulus, publicKey2.Modulus));

            File.Delete(destinationFilename);
        }

        [TestMethod]
        public void EncryptFile()
        {
            //create txt file
            const string fileContent = "hello world of encryption!";
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            File.WriteAllText(filePath, fileContent);

            _cryptographyService.AssignNewKeys();

            //encrypt
            var destinationFilename = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            _cryptographyService.EncryptFile(filePath, destinationFilename, _cryptographyService.GetPublicKey());
            
            //check content
            var fileContent2 = File.ReadAllText(destinationFilename + ".sfs");

            Assert.IsFalse(fileContent == fileContent2);

            File.Delete(filePath);
            File.Delete(destinationFilename + ".sfs");
        }

        [TestMethod]
        public void DecryptFile()
        {
            //create txt file
            const string fileContent = "hello world of encryption!";
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            File.WriteAllText(filePath, fileContent);

            _cryptographyService.AssignNewKeys();

            //encrypt
            var destinationFilename = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            _cryptographyService.EncryptFile(filePath, destinationFilename, _cryptographyService.GetPublicKey());

            //check content
            var fileContent2 = File.ReadAllText(destinationFilename + ".sfs");
            Assert.IsFalse(fileContent == fileContent2);

            //decrypt
            var destinationFilename2 = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var result = _cryptographyService.DecryptFile(destinationFilename + ".sfs", destinationFilename2,
                _cryptographyService.GetPrivateKeyAsXml());

            Assert.IsTrue(result);

            //check content
            var fileContent3 = File.ReadAllText(destinationFilename2 + "..tmp");
            Assert.IsTrue(fileContent == fileContent3);

            File.Delete(filePath);
            File.Delete(destinationFilename + ".sfs");
            File.Delete(destinationFilename2 + "..tmp");
        }
    }
}