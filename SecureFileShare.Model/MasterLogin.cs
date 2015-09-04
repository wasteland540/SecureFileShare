using System.Security.Cryptography;

namespace SecureFileShare.Model
{
    public class MasterLogin : IDbObject
    {
        public byte[] Password { get; set; }
        public byte[] Salt { get; set; }
        public string Name { get; set; }
        public RSAParameters PublicKey { get; set; }
        public RSAParameters PrivateKey { get; set; }
    }
}