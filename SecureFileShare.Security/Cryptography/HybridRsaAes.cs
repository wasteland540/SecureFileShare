using System;
using System.IO;
using System.Security.Cryptography;
using log4net;

namespace SecureFileShare.Security.Cryptography
{
    public class HybridRsaAes : AbstractSecureCompareBase
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(HybridRsaAes));

        private RSAParameters _privateKey;
        private RSAParameters _publicKey;
        private string _publicKeyAsXml;
        private string _privateKeyAsXml;

        // ReSharper disable once InconsistentNaming
        public byte[] EncryptAES(byte[] dataToEncrypt, byte[] key, byte[] iv)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                _logger.Info("mode: CBC");
                aes.Mode = CipherMode.CBC;
                _logger.Info("padding: PKCS7");
                aes.Padding = PaddingMode.PKCS7;

                aes.Key = key;
                aes.IV = iv;

                _logger.Info("create stream");
                using (var memoryStream = new MemoryStream())
                {
                    var cryptoStream = new CryptoStream(memoryStream,
                        aes.CreateEncryptor(), CryptoStreamMode.Write);

                    _logger.Info("write data to stream");
                    cryptoStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public byte[] DecryptAES(byte[] dataToDecrypt, byte[] key, byte[] iv)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                _logger.Info("mode: CBC");
                aes.Mode = CipherMode.CBC;
                _logger.Info("padding: PKCS7");
                aes.Padding = PaddingMode.PKCS7;

                aes.Key = key;
                aes.IV = iv;

                _logger.Info("create stream");
                using (var memoryStream = new MemoryStream())
                {
                    var cryptoStream = new CryptoStream(memoryStream,
                        aes.CreateDecryptor(), CryptoStreamMode.Write);

                    _logger.Info("write data to stream");
                    cryptoStream.Write(dataToDecrypt, 0, dataToDecrypt.Length);
                    cryptoStream.FlushFinalBlock();

                    byte[] decryptBytes = memoryStream.ToArray();

                    return decryptBytes;
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public void AssignNewRSAKeys()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                _publicKey = rsa.ExportParameters(false);
                _privateKey = rsa.ExportParameters(true);

                _publicKeyAsXml = rsa.ToXmlString(false);
                _privateKeyAsXml = rsa.ToXmlString(true);
            }
        }

        // ReSharper disable once InconsistentNaming
        public byte[] EncryptRSA(byte[] dataToEncrypt, RSAParameters publicKey)
        {
            byte[] cipherBytes;

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportParameters(publicKey);

                cipherBytes = rsa.Encrypt(dataToEncrypt, false);
            }

            return cipherBytes;
        }

        // ReSharper disable once InconsistentNaming
        public byte[] DecryptRSA(byte[] dataToDecrypt, RSAParameters privateKey)
        {
            byte[] plainBytes = null;

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportParameters(privateKey);

                try
                {
                    plainBytes = rsa.Decrypt(dataToDecrypt, false);
                }
                catch (Exception e)
                {
                    _logger.Error("error by decrypting data! --> " + e.Message);
                }
                
            }

            return plainBytes;
        }

        public byte[] ComputeHmac(byte[] sessionKey, byte[] encrpytedData)
        {
            using (var hmac = new HMACSHA256(sessionKey))
            {
                return hmac.ComputeHash(encrpytedData);
            }
        }

        public bool VerifyHmac(byte[] decrpytedSessionKey, byte[] encryptedData, byte[] hmacToCheck)
        {
            using (var hmac = new HMACSHA256(decrpytedSessionKey))
            {
                byte[] computedHmac = hmac.ComputeHash(encryptedData);

                return Compare(computedHmac, hmacToCheck);
            }
        }

        // ReSharper disable once InconsistentNaming
        public RSAParameters GetPublicRSAKey()
        {
            return _publicKey;
        }

        // ReSharper disable once InconsistentNaming
        public RSAParameters GetPrivateRSAKey()
        {
            return _privateKey;
        }

        // ReSharper disable once InconsistentNaming
        public string GetPublicRSAKeyAsXml()
        {
            return _publicKeyAsXml;
        }

        // ReSharper disable once InconsistentNaming
        public string GetPrivateRSAKeyAsXml()
        {
            return _privateKeyAsXml;
        }
    }
}