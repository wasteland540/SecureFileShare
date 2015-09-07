using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using log4net;
using SecureFileShare.Model;
using SecureFileShare.Security.Cryptography;
using SecureFileShare.Util.FileIO;

namespace SecureFileShare.App.Services
{
    public class CryptographyService : ICryptographyService
    {
        private readonly HybridRsaAes _hybridEncrypter;
        private readonly ILog _logger = LogManager.GetLogger(typeof (CryptographyService));

        public CryptographyService()
        {
            _hybridEncrypter = new HybridRsaAes();
        }

        public bool Compare(byte[] array1, byte[] array2)
        {
            _logger.Info("compare byte arrays");
            return AbstractSecureCompareBase.Compare(array1, array2);
        }

        public byte[] GenerateSalt()
        {
            _logger.Info("generate salt");
            return PBKDF2Impl.GenerateSalt();
        }

        public byte[] HashPassword(string plainPassword, byte[] salt)
        {
            _logger.Info("hash password");
            return PBKDF2Impl.HashPassword(Encoding.UTF8.GetBytes(plainPassword), salt);
        }

        public void AssignNewKeys()
        {
            _logger.Info("assign new keys");
            _hybridEncrypter.AssignNewRSAKeys();
        }

        public RSAParameters GetPublicKey()
        {
            _logger.Info("get public key");
            return _hybridEncrypter.GetPublicRSAKey();
        }

        public RSAParameters GetPrivateKey()
        {
            _logger.Info("get prviate key");
            return _hybridEncrypter.GetPrivateRSAKey();
        }

        public string GetPrivateKeyAsXml()
        {
            return _hybridEncrypter.GetPrivateRSAKeyAsXml();
        }

        public bool ExportPublicKeyFile(string destinationFilename, RSAParameters publicKey)
        {
            bool success = true;

            _logger.Info("get key as xml");
            var rsa = new RSACryptoServiceProvider(2048);
            rsa.ImportParameters(publicKey);
            string keyAsXml = rsa.ToXmlString(false);

            _logger.Info("create file stream");
            using (FileStream fs = File.Create(destinationFilename))
            {
                _logger.Info("get bytes");
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

            using (FileStream fs = File.OpenRead(sourceFilename))
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

            var rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(keyAsXml);

            return rsa.ExportParameters(false);
        }

        public void EncryptFile(string sourceFilename, string destinationFilename, RSAParameters publicKey)
        {
            _logger.Info("get data from file");
            byte[] dataToEncrypt = GetDataFromFile(sourceFilename);

            if (dataToEncrypt != null)
            {
                _logger.Info("generate session key");
                byte[] sessionKey = AbstractSecureCompareBase.GenerateRandomNumber(32); //256-bit
                _logger.Info("generate IV");
                byte[] iv = AbstractSecureCompareBase.GenerateRandomNumber(16); //128-bit

                _logger.Info("encrypt data");
                byte[] encryptedData = _hybridEncrypter.EncryptAES(dataToEncrypt, sessionKey, iv);
                _logger.Info("generate HMAC");
                byte[] hmac = _hybridEncrypter.ComputeHmac(sessionKey, encryptedData);
                _logger.Info("encrypt session key");
                byte[] encryptedSessionKey = _hybridEncrypter.EncryptRSA(sessionKey, publicKey);

                _logger.Info("encrypt file extension");
                string fileExtension = Path.GetExtension(sourceFilename);

                if (fileExtension != null)
                {
                    byte[] encryptedFileExtension = _hybridEncrypter.EncryptAES(Encoding.UTF8.GetBytes(fileExtension),
                        sessionKey, iv);

                    _logger.Info("write data to file");
                    WriteDataToFile(encryptedData, hmac, encryptedSessionKey, iv, encryptedFileExtension,
                        destinationFilename + ".sfs");
                }
                else
                {
                    _logger.Error("file extenstion is null!");
                }
            }
            else
            {
                _logger.Error("data from file are null!");
            }
        }

        public bool DecryptFile(string sourceFilename, string destinationFilename, string privateKey)
        {
            bool isDecrpyt = true;

            _logger.Info("get encrypted file");
            var encryptedFile = BinarySerializer.Deserialize<EncryptedFile>(sourceFilename);

            _logger.Info("decrypt session key");
            var rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(privateKey);

            byte[] decryptedSessionKey = _hybridEncrypter.DecryptRSA(encryptedFile.SessionKey,
                rsa.ExportParameters(true));

            if (decryptedSessionKey != null)
            {
                _logger.Info("verify HMAC");
                if (_hybridEncrypter.VerifyHmac(decryptedSessionKey, encryptedFile.Data, encryptedFile.Hmac))
                {
                    _logger.Info("HMAC verified");

                    _logger.Info("decrypt data");
                    byte[] decryptedData = _hybridEncrypter.DecryptAES(encryptedFile.Data, decryptedSessionKey,
                        encryptedFile.Iv);
                    _logger.Info("decrypt file extension");
                    byte[] decryptedFileExtension = _hybridEncrypter.DecryptAES(encryptedFile.FileExtension,
                        decryptedSessionKey, encryptedFile.Iv);

                    string fileExtension = Encoding.UTF8.GetString(decryptedFileExtension);

                    _logger.Info("write decrypted data to file");
                    WriteDataToFile(decryptedData, destinationFilename + "." + fileExtension);
                }
                else
                {
                    _logger.Error("verify HMAC failed!");
                    isDecrpyt = false;
                }
            }
            else
            {
                _logger.Error("Session key can not decrypt!");
                isDecrpyt = false;
            }

            return isDecrpyt;
        }

        private byte[] GetDataFromFile(string filepath)
        {
            byte[] fileContent;

            _logger.Info("create file stream");
            using (FileStream fs = File.OpenRead(filepath))
            {
                fileContent = new byte[fs.Length];

                try
                {
                    _logger.Info("try reading file content");
                    fs.Read(fileContent, 0, fileContent.Length);
                    _logger.Info("reading file content successful");
                }
                catch (Exception e)
                {
                    _logger.Error("error reading file content! --> " + e.Message);
                }
                finally
                {
                    _logger.Info("close stream");
                    fs.Close();
                }
            }

            return fileContent;
        }

        private void WriteDataToFile(byte[] encryptedData, byte[] hmac, byte[] encryptedSessionKey, byte[] iv,
            byte[] encryptedFileExtension, string filepath)
        {
            var encrpytedFile = new EncryptedFile
            {
                Data = encryptedData,
                FileExtension = encryptedFileExtension,
                Hmac = hmac,
                Iv = iv,
                SessionKey = encryptedSessionKey
            };

            _logger.Info("serialize encrypted file");
            BinarySerializer.Serialize(filepath, encrpytedFile);
            _logger.Info("finished serialization");
        }

        private void WriteDataToFile(byte[] decryptedData, string filepath)
        {
            _logger.Info("destination: " + filepath);
            _logger.Info("create file stream");
            using (FileStream fs = File.Create(filepath))
            {
                try
                {
                    _logger.Info("try writing file content");
                    fs.Write(decryptedData, 0, decryptedData.Length);
                    _logger.Info("writing file content successful");
                }
                catch (Exception e)
                {
                    _logger.Error("error writing file content! --> " + e.Message);
                }
                finally
                {
                    _logger.Info("close stream");
                    fs.Close();
                }
            }
        }
    }
}