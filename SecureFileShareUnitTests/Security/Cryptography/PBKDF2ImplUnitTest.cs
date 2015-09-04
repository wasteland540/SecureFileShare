using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureFileShare.Security.Cryptography;

namespace SecureFileShareUnitTests.Security.Cryptography
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class PBKDF2ImplUnitTest
    {
        [TestMethod]
        public void TestHashPassword()
        {
            const string password = "********";
            const string rightPassword = "********";
            const string wrongPassword = "password";

            byte[] salt = PBKDF2Impl.GenerateSalt();
            byte[] encrypedPassword = PBKDF2Impl.HashPassword(Encoding.UTF8.GetBytes(password), salt);

            byte[] encrypedRightPassword = PBKDF2Impl.HashPassword(Encoding.UTF8.GetBytes(rightPassword), salt);
            byte[] encrypedWrongPassword = PBKDF2Impl.HashPassword(Encoding.UTF8.GetBytes(wrongPassword), salt);

            Assert.IsTrue(AbstractSecureCompareBase.Compare(encrypedPassword, encrypedRightPassword));
            Assert.IsFalse(AbstractSecureCompareBase.Compare(encrypedPassword, encrypedWrongPassword));
        }
    }
}