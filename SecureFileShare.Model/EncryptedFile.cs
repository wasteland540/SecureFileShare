using System;

namespace SecureFileShare.Model
{
    [Serializable]
    public class EncryptedFile
    {
        public string Filename { get; set; }
        public byte[] FileExtension { get; set; }
        public byte[] Data { get; set; }
        public byte[] SessionKey { get; set; }
        public byte[] Iv { get; set; }
        public byte[] Hmac { get; set; }
    }
}