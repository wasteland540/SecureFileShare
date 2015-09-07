using System.Security.Cryptography;

namespace SecureFileShare.App.Services
{
    public interface ICryptographyService
    {
        bool Compare(byte[] array1, byte[] array2);

        byte[] GenerateSalt();

        byte[] HashPassword(string plainPassword, byte[] salt);

        void AssignNewKeys();

        RSAParameters GetPublicKey();

        RSAParameters GetPrivateKey();

        string GetPrivateKeyAsXml();

        bool ExportPublicKeyFile(string destinationFilename, RSAParameters publicKey);

        RSAParameters ImportPublicKeyFile(string sourceFilename);

        void EncryptFile(string sourceFilename, string destinationFilename, RSAParameters publicKey);

        bool DecryptFile(string sourceFilename, string destinationFilename, string privateKey);
    }
}