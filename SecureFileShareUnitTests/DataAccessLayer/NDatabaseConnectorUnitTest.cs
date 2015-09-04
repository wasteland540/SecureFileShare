using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.DataAccessLayer.NDatabase;
using SecureFileShare.Model;
using SecureFileShare.Security.Cryptography;

namespace SecureFileShareUnitTests.DataAccessLayer
{
    [TestClass]
    public class NDatabaseConnectorUnitTest
    {
        private static IDataAccessLayer _dataAccess;

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            _dataAccess = new NDatabaseConnector();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
            _dataAccess.Close();
        }

        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            List<Contact> contacts = _dataAccess.GetAll<Contact>();

            foreach (Contact contact in contacts)
            {
                _dataAccess.Delete(contact);
            }

            List<MasterLogin> masterLogins = _dataAccess.GetAll<MasterLogin>();

            foreach (MasterLogin login in masterLogins)
            {
                _dataAccess.Delete(login);
            }
        }

        #endregion

        [TestMethod]
        public void Insert()
        {
            List<Contact> contacts = _dataAccess.GetAll<Contact>();
            Assert.IsTrue(contacts.Count == 0);

            List<MasterLogin> materLogins = _dataAccess.GetAll<MasterLogin>();
            Assert.IsTrue(materLogins.Count == 0);

            var hybridEncrypter = new HybridRsaAes();
            hybridEncrypter.AssignNewRSAKeys();

            var contact = new Contact
            {
                Name = "Marcel",
                PublicKey = hybridEncrypter.GetPublicRSAKey()
            };
            _dataAccess.Insert(contact);

            contacts = _dataAccess.GetAll<Contact>();
            Assert.IsTrue(contacts.Count == 1);

            var salt = PBKDF2Impl.GenerateSalt();
            hybridEncrypter = new HybridRsaAes();
            hybridEncrypter.AssignNewRSAKeys();

            var materLogin = new MasterLogin
            {
                Name = "MasterMan",
                Password = PBKDF2Impl.HashPassword(Encoding.UTF8.GetBytes("password123"), salt),
                Salt = salt,
                PrivateKey = hybridEncrypter.GetPrivateRSAKey(),
                PublicKey = hybridEncrypter.GetPublicRSAKey()
            };
            _dataAccess.Insert(materLogin);

            materLogins = _dataAccess.GetAll<MasterLogin>();
            Assert.IsTrue(materLogins.Count == 1);
        }

        [TestMethod]
        public void Update()
        {
            List<Contact> contacts = _dataAccess.GetAll<Contact>();
            Assert.IsTrue(contacts.Count == 0);

            var hybridEncrypter = new HybridRsaAes();
            hybridEncrypter.AssignNewRSAKeys();

            var contact = new Contact
            {
                Name = "Marcel",
                PublicKey = hybridEncrypter.GetPublicRSAKey()
            };
            _dataAccess.Insert(contact);

            contacts = _dataAccess.GetAll<Contact>();
            Assert.IsTrue(contacts.Count == 1);

            Contact contact2 = contacts[0];
            hybridEncrypter.AssignNewRSAKeys();
            contact2.Name = "Marcel.Elz";
            contact2.PublicKey = hybridEncrypter.GetPublicRSAKey();
            _dataAccess.Update(contact2);

            contacts = _dataAccess.GetAll<Contact>();
            Assert.IsTrue(contacts.Count == 1);
            Assert.IsTrue(contacts[0].Name == "Marcel.Elz");
        }

        [TestMethod]
        public void GeSingleByName()
        {
            List<Contact> contacts = _dataAccess.GetAll<Contact>();
            Assert.IsTrue(contacts.Count == 0);

            List<MasterLogin> masterLogins = _dataAccess.GetAll<MasterLogin>();
            Assert.IsTrue(masterLogins.Count == 0);

            var hybridEncrypter = new HybridRsaAes();
            hybridEncrypter.AssignNewRSAKeys();

            var contact = new Contact
            {
                Name = "Marcel",
                PublicKey = hybridEncrypter.GetPublicRSAKey()
            };
            _dataAccess.Insert(contact);

            hybridEncrypter.AssignNewRSAKeys();
            var contact2 = new Contact
            {
                Name = "Mario",
                PublicKey = hybridEncrypter.GetPublicRSAKey()
            };
            _dataAccess.Insert(contact2);

            contacts = _dataAccess.GetAll<Contact>();
            Assert.IsTrue(contacts.Count == 2);

            var contact3 = _dataAccess.GetSingleByName<Contact>("Marcel");
            Assert.IsTrue(contact3 != null);
            Assert.IsTrue(contact3.Name == "Marcel");

            var salt = PBKDF2Impl.GenerateSalt();
            hybridEncrypter = new HybridRsaAes();
            hybridEncrypter.AssignNewRSAKeys();

            var materLogin = new MasterLogin
            {
                Name = "MasterMan",
                Password = PBKDF2Impl.HashPassword(Encoding.UTF8.GetBytes("password123"), salt),
                Salt = salt,
                PrivateKey = hybridEncrypter.GetPrivateRSAKey(),
                PublicKey = hybridEncrypter.GetPublicRSAKey()
            };
            _dataAccess.Insert(materLogin);

            masterLogins = _dataAccess.GetAll<MasterLogin>();
            Assert.IsTrue(masterLogins.Count == 1);

            var materLogin2 = _dataAccess.GetSingleByName<MasterLogin>("MasterMan");
            Assert.IsTrue(materLogin2 != null);
            Assert.IsTrue(materLogin2.Name == "MasterMan");
        }    
    }
}
