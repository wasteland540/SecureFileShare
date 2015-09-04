using System.Security.Cryptography;

namespace SecureFileShare.Security.Cryptography
{
    public abstract class AbstractSecureCompareBase
    {
        public static bool Compare(byte[] array1, byte[] array2)
        {
            bool result = array1.Length == array2.Length;

            for (int i = 0; i < array1.Length && i < array2.Length; ++i)
            {
                result &= array1[i] == array2[i];
            }

            return result;
        }

        public static byte[] GenerateRandomNumber(int length)
        {
            using (var randomNumberGenerator = new RNGCryptoServiceProvider())
            {
                var randomNumber = new byte[length];
                randomNumberGenerator.GetBytes(randomNumber);

                return randomNumber;
            }
        }
    }
}