using System.Security.Cryptography;

namespace SecureFileShare.Security.Cryptography
{
    // ReSharper disable once InconsistentNaming
    public class PBKDF2Impl : AbstractSecureCompareBase
    {
        public static byte[] GenerateSalt()
        {
            return GenerateRandomNumber(128);
        }

        public static byte[] HashPassword(byte[] toBeHashed, byte[] salt, int
            numberOfRounds = 50000)
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(toBeHashed, salt,
                numberOfRounds))
            {
                return rfc2898.GetBytes(128);
            }
        }
    }
}