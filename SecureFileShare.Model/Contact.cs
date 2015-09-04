using System.Security.Cryptography;

namespace SecureFileShare.Model
{
    public class Contact : IDbObject
    {
        public RSAParameters PublicKey { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}