using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;
using log4net;
using SecureFileShare.Security.Cryptography;

namespace SecureFileShare.App.Services
{
    public class CryptographyService : ICryptographyService
    {
        //TODO:
        private readonly ILog _logger = LogManager.GetLogger(typeof(CryptographyService));
        private readonly HybridRsaAes _hybridEncrypter;

        public CryptographyService()
        {
            _hybridEncrypter = new HybridRsaAes();
        }

        public bool Compare(byte[] array1, byte[] array2)
        {
            return AbstractSecureCompareBase.Compare(array1, array2);
        }

        public byte[] GenerateSalt()
        {
            return PBKDF2Impl.GenerateSalt();
        }

        public byte[] HashPassword(string plainPassword, byte[] salt)
        {
            return PBKDF2Impl.HashPassword(Encoding.UTF8.GetBytes(plainPassword), salt);
        }

        public void AssignNewKeys()
        {
            _hybridEncrypter.AssignNewRSAKeys();
        }

        public RSAParameters GetPublicKey()
        {
            return _hybridEncrypter.GetPublicRSAKey();
        }

        public RSAParameters GetPrivateKey()
        {
            return _hybridEncrypter.GetPrivateRSAKey();
        }

        public bool ExportPublicKeyFile(string destinationFilename, RSAParameters publicKey)
        {
            bool success = true;

            _logger.Info("get key as xml");
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(publicKey);
            var keyAsXml = rsa.ToXmlString(false);

            _logger.Info("create file stream");
            using (var fs = File.Create(destinationFilename))
            {
                byte[] content = Encoding.UTF8.GetBytes(keyAsXml);

                try
                {
                    _logger.Info("try writing xml content to file");
                    fs.Write(content, 0, content.Length);
                    _logger.Info("writing xml content to file successful");
                }
                catch (Exception e)
                {
                    success = false;
                    _logger.Error("error export public key file! --> " + e.Message);
                }
                finally
                {
                    _logger.Info("close stream");
                    fs.Close();
                }
            }

            return success;
        }

        public RSAParameters ImportPublicKeyFile(string sourceFilename)
        {
            _logger.Info("get key from xml");
            string keyAsXml;

            using (var fs = File.OpenRead(sourceFilename))
            {
                var fileContent = new byte[fs.Length];

                try
                {
                    _logger.Info("try reading key file");
                    fs.Read(fileContent, 0, fileContent.Length);
                    _logger.Info("reading key file successful");
                }
                catch (Exception e)
                {
                    _logger.Error("error reading key file! --> " + e.Message);
                    throw;
                }
                finally
                {
                    _logger.Info("close stream");
                    fs.Close();
                }

                keyAsXml = Encoding.UTF8.GetString(fileContent);
            }

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(keyAsXml);

            return rsa.ExportParameters(false);
        }

        public void EncryptFile(string sourceFilename, string destinationFilename, RSAParameters publicKey)
        {
            throw new NotImplementedException();
        }

        public void DecryptFile(string sourceFilename, string destinationFilename, RSAParameters privateKey)
        {
            throw new NotImplementedException();
        }
    }
}