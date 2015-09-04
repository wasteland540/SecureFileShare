using System;
using System.Security.Cryptography;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using SecureFileShare.App.Commands;
using SecureFileShare.App.Messages;
using SecureFileShare.App.Services;
using SecureFileShare.App.Views.Interfaces;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.Model;

namespace SecureFileShare.App.ViewModels.Contacts
{
    public class AddEditContactViewModel : ViewModelBase
    {
        private readonly ICryptographyService _cryptographyService;
        private readonly IDataAccessLayer _database;
        private readonly ILog _logger = LogManager.GetLogger(typeof (AddEditContactViewModel));
        private readonly IMessenger _messenger;
        private ICommand _chooseCommand;
        private bool _importKeyFileSuccess = true;

        private bool _isNew;
        private string _keyFilePath;
        private string _name;
        private ICommand _saveCommand;
        private string _title;

        public AddEditContactViewModel(IMessenger messenger, IDataAccessLayer database,
            ICryptographyService cryptographyService)
        {
            _messenger = messenger;
            _database = database;
            _cryptographyService = cryptographyService;

            _messenger.Register<AddEditContactConfirmMsg>(this, OnAddEditContactConfirmMsg);
        }

        public void OnClosing()
        {
            _messenger.Unregister<AddEditContactConfirmMsg>(this, OnAddEditContactConfirmMsg);
        }

        #region Properties

        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                _isNew = value;
                Title = _isNew ? "New Contact" : "Edit Contact";
                _importKeyFileSuccess = false;
                RaisePropertyChanged("IsNew");
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (value != null && value != _title)
                {
                    _title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != null && value != _name)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public string KeyFilePath
        {
            get { return _keyFilePath; }
            set
            {
                if (value != null && value != _keyFilePath)
                {
                    _keyFilePath = value;
                    RaisePropertyChanged("KeyFilePath");
                }
            }
        }

        public ICommand ChooseCommand
        {
            get
            {
                _chooseCommand = _chooseCommand ?? new DelegateCommand(Choose);
                return _chooseCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                _saveCommand = _saveCommand ?? new DelegateCommand(Save);
                return _saveCommand;
            }
        }

        #endregion Properties

        #region Private Methods

        private void Choose(object obj)
        {
            _logger.Info("open file chooser");
            _messenger.Send(new AddEditContactRequestMsg());
        }

        private void Save(object obj)
        {
            _logger.Info("save contact");
            if (_importKeyFileSuccess)
            {
                _logger.Info("check database for contact");
                var contact = _database.GetSingleByName<Contact>(OriginalName);

                if (contact != null)
                {
                    _logger.Info("contact exists");

                    contact.Name = Name;
                    contact.PublicKey = PublicKey;

                    _logger.Info("update in database");
                    _database.Update(contact);
                }
                else
                {
                    _logger.Info("contact do not exists -> create new one");

                    contact = new Contact
                    {
                        Name = Name,
                        PublicKey = PublicKey
                    };

                    _logger.Info("insert in database");
                    _database.Insert(contact);
                }

                _messenger.Send(new SaveContactMsg());

                _logger.Info("try to close view");
                var view = obj as ICloseable;

                if (view != null)
                {
                    view.Close();
                    _logger.Info("view closed");
                }
            }
            else
            {
                _logger.Error("something went wrong with the public key");
            }
        }

        private void OnAddEditContactConfirmMsg(AddEditContactConfirmMsg msg)
        {
            string filename = msg.Filename;
            KeyFilePath = filename;

            _logger.Info("try import public key file");

            try
            {
                PublicKey = _cryptographyService.ImportPublicKeyFile(filename);
                _messenger.Send(new ImportPublicKeyFileSuccuessMsg());
                _importKeyFileSuccess = true;
            }
            catch (Exception e)
            {
                _logger.Error("can not import public key file, cause: " + e.Message);
                _messenger.Send(new ImportPublicKeyFileFailedMsg());
                _importKeyFileSuccess = false;
            }
        }

        #endregion Private Methods

        public string OriginalName { get; set; }
        public RSAParameters PublicKey { get; set; }
    }
}