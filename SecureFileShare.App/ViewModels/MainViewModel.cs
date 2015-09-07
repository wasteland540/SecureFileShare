using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using log4net;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Commands;
using SecureFileShare.App.Messages;
using SecureFileShare.App.Services;
using SecureFileShare.App.ViewModels.Contacts;
using SecureFileShare.App.Views.Contacts;
using SecureFileShare.App.Views.MyAccount;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.Model;
using SecureFileShare.Util.FileIO;

namespace SecureFileShare.App.ViewModels
{
    public class MainViewModel : ViewModelBase, IDropTarget
    {
        private const string SecureFileShareExtension = ".sfs";
        private readonly ICryptographyService _cryptographyService;
        private readonly IDataAccessLayer _database;
        private readonly ILog _logger = LogManager.GetLogger(typeof (MainViewModel));
        private readonly IMessenger _messenger;

        private ICommand _assignNewKeysCommand;
        private ICommand _changePasswordCommand;
        private ICommand _chooseContactCommand;
        private ICommand _chooseSourceCommand;
        private ICommand _chooseTargetCommand;
        private string _contactName;
        private ContactsView _contactsView;
        private ICommand _decryptCommand;
        private bool _disableForEncryption = true;
        private ICommand _encryptCommand;
        private ICommand _exitCommand;
        private ICommand _exportPublicKeyCommand;
        private bool _isProcessInProgress;
        private ICommand _openContactsCommand;
        private string _sourceFilepath;
        private string _targetFilepath;

        public MainViewModel(ICryptographyService cryptographyService, IMessenger messenger, IDataAccessLayer database)
        {
            _cryptographyService = cryptographyService;
            _messenger = messenger;
            _database = database;

            //register for messages
            _messenger.Register<AssignNewKeysConfirmMsg>(this, OnAssignNewKeysConfirmMsg);
            _messenger.Register<ExportPublicKeyConfirmMsg>(this, OnExportPublicKeyConfirmMsg);
            _messenger.Register<ContactsViewClosedMsg>(this, OnContactsViewClosedMsg);
            _messenger.Register<ChooseSourceConfirmMsg>(this, OnChooseSourceConfirmMsg);
            _messenger.Register<ChooseTargetConfirmMsg>(this, OnChooseTargetConfirmMsg);
            _messenger.Register<ContactSelectedMsg>(this, OnContactSelectedMsg);
        }

        #region Properties

        public ICommand ExitCommand
        {
            get
            {
                _exitCommand = _exitCommand ?? new DelegateCommand(Exit);
                return _exitCommand;
            }
        }

        public ICommand OpenContactsCommand
        {
            get
            {
                _openContactsCommand = _openContactsCommand ?? new DelegateCommand(OpenContacts);
                return _openContactsCommand;
            }
        }

        public ICommand ChangePasswordCommand
        {
            get
            {
                _changePasswordCommand = _changePasswordCommand ?? new DelegateCommand(ChangePassword);
                return _changePasswordCommand;
            }
        }

        public ICommand AssignNewKeysCommand
        {
            get
            {
                _assignNewKeysCommand = _assignNewKeysCommand ?? new DelegateCommand(AssignNewKeys);
                return _assignNewKeysCommand;
            }
        }

        public ICommand ExportPublicKeyCommand
        {
            get
            {
                _exportPublicKeyCommand = _exportPublicKeyCommand ?? new DelegateCommand(ExportPublicKey);
                return _exportPublicKeyCommand;
            }
        }

        public string SourceFilepath
        {
            get { return _sourceFilepath; }
            set
            {
                if (value != null && value != _sourceFilepath)
                {
                    _sourceFilepath = value;
                    DisableForEncryption = !IsVaildFile();
                    RaisePropertyChanged("SourceFilepath");
                }
            }
        }

        public string TargetFilepath
        {
            get { return _targetFilepath; }
            set
            {
                if (value != null && value != _targetFilepath)
                {
                    _targetFilepath = value;
                    RaisePropertyChanged("TargetFilepath");
                }
            }
        }

        public ICommand ChooseSourceCommand
        {
            get
            {
                _chooseSourceCommand = _chooseSourceCommand ?? new DelegateCommand(ChooseSource);
                return _chooseSourceCommand;
            }
        }

        public ICommand ChooseTargetCommand
        {
            get
            {
                _chooseTargetCommand = _chooseTargetCommand ?? new DelegateCommand(ChooseTarget);
                return _chooseTargetCommand;
            }
        }

        public ICommand ChooseContactCommand
        {
            get
            {
                _chooseContactCommand = _chooseContactCommand ?? new DelegateCommand(ChooseContact);
                return _chooseContactCommand;
            }
        }

        public ICommand EncryptCommand
        {
            get
            {
                _encryptCommand = _encryptCommand ?? new DelegateCommand(Encrypt);
                return _encryptCommand;
            }
        }

        public ICommand DecryptCommand
        {
            get
            {
                _decryptCommand = _decryptCommand ?? new DelegateCommand(Decrypt);
                return _decryptCommand;
            }
        }

        public string ContactName
        {
            get { return _contactName; }
            set
            {
                if (value != null && value != _contactName)
                {
                    _contactName = value;
                    RaisePropertyChanged("ContactName");
                }
            }
        }

        public bool DisableForEncryption
        {
            get { return _disableForEncryption; }
            set
            {
                _disableForEncryption = value;
                RaisePropertyChanged("DisableForEncryption");
            }
        }

        #endregion Properties

        #region Private Methods

        public void DragOver(IDropInfo dropInfo)
        {
            IEnumerable<string> dragFileList = ((DataObject) dropInfo.Data).GetFileDropList().Cast<string>();
            IList<string> fileList = dragFileList as IList<string> ?? dragFileList.ToList();

            if (fileList.Count() == 1)
            {
                dropInfo.Effects = fileList.Any(item =>
                {
                    string extension = Path.GetExtension(item);
                    return extension != null;
                })
                    ? DragDropEffects.Copy
                    : DragDropEffects.None;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            IEnumerable<string> dragFileList = ((DataObject) dropInfo.Data).GetFileDropList().Cast<string>();
            IList<string> fileList = dragFileList as IList<string> ?? dragFileList.ToList();

            if (fileList.Count() == 1)
            {
                dropInfo.Effects = fileList.Any(item =>
                {
                    string extension = Path.GetExtension(item);
                    return extension != null;
                })
                    ? DragDropEffects.Copy
                    : DragDropEffects.None;

                if (dropInfo.Effects == DragDropEffects.Copy)
                {
                    SourceFilepath = fileList.FirstOrDefault();
                }
            }
        }

        private void Exit(object obj)
        {
            if (!_isProcessInProgress)
            {
                _logger.Info("shutdown application");
                Application.Current.Shutdown();
            }
        }

        private void OpenContacts(object obj)
        {
            OpenContactView(false);
        }

        private void OpenContactView(bool isSelectionMode)
        {
            if (_contactsView == null)
            {
                _logger.Info("show contacts view");
                _contactsView = Container.Resolve<ContactsView>();
                _contactsView.Show();

                var viewmodel = (ContactsViewModel) _contactsView.DataContext;
                viewmodel.IsSelectionMode = isSelectionMode;
            }
            else
            {
                _logger.Warn("contacts view already open");
                _logger.Info("push view in foreground");

                var viewmodel = (ContactsViewModel) _contactsView.DataContext;
                viewmodel.IsSelectionMode = isSelectionMode;

                _contactsView.Focus();
            }
        }

        private void ChangePassword(object obj)
        {
            _logger.Info("show change password view");
            var changePasswordView = Container.Resolve<ChangePasswordView>();
            changePasswordView.ShowDialog();
        }

        private void AssignNewKeys(object obj)
        {
            _messenger.Send(new AssignNewKeysRequestMsg());
        }

        private void ExportPublicKey(object obj)
        {
            _messenger.Send(new ExportPublicKeyRequestMsg());
        }

        private void OnAssignNewKeysConfirmMsg(AssignNewKeysConfirmMsg msg)
        {
            _logger.Info("assign new keys confirmed --> assing new keys");
            MasterLogin login = _database.GetAll<MasterLogin>().FirstOrDefault();

            if (login != null)
            {
                _logger.Info("assign new keys");
                _cryptographyService.AssignNewKeys();

                login.PublicKey = _cryptographyService.GetPublicKey();
                login.PrivateKey = _cryptographyService.GetPrivateKeyAsXml();

                _logger.Info("update login with new keys");
                _database.Update(login);
                _logger.Info("login with new keys updated");

                _messenger.Send(new AssignNewKeysSuccsessMsg());
            }
            else
            {
                _logger.Error("login is null!");
            }
        }

        private void OnExportPublicKeyConfirmMsg(ExportPublicKeyConfirmMsg msg)
        {
            _isProcessInProgress = true;
            UIServices.SetBusyState();
            _logger.Info("export public key confirmed --> export key file");
            _logger.Info("export path: " + msg.Filename);

            MasterLogin login = _database.GetAll<MasterLogin>().FirstOrDefault();

            if (login != null)
            {
                bool success = _cryptographyService.ExportPublicKeyFile(msg.Filename, login.PublicKey);

                if (success)
                {
                    _logger.Info("export successful");
                    _messenger.Send(new ExportPublicKeySuccsessMsg());
                }
                else
                {
                    _logger.Error("export failed");
                    _messenger.Send(new ExportPublicKeyFailedMsg());
                }
            }
            else
            {
                _logger.Error("login is null!");
            }

            _isProcessInProgress = false;
        }

        private void OnContactsViewClosedMsg(ContactsViewClosedMsg msg)
        {
            _logger.Info("set contacts view to null");
            _contactsView = null;
        }

        private void ChooseSource(object obj)
        {
            _messenger.Send(new ChooseSourceRequestMsg());
        }

        private void ChooseTarget(object obj)
        {
            _messenger.Send(new ChooseTargetRequestMsg());
        }

        private void ChooseContact(object obj)
        {
            OpenContactView(true);
        }

        private void Encrypt(object obj)
        {
            _isProcessInProgress = true;
            UIServices.SetBusyState();

            _logger.Info("start encryption");
            _logger.Info("source: " + _sourceFilepath);
            _logger.Info("destination: " + _targetFilepath);
            _logger.Info("receipient: " + _contactName);

            _cryptographyService.EncryptFile(_sourceFilepath, _targetFilepath, PublicKey);
            _messenger.Send(new EncryptionSuccsessMsg());

            //reset inputs
            SourceFilepath = string.Empty;
            TargetFilepath = string.Empty;
            ContactName = string.Empty;

            _disableForEncryption = true;
            _isProcessInProgress = false;
        }

        private void Decrypt(object obj)
        {
            _isProcessInProgress = true;
            UIServices.SetBusyState();

            _logger.Info("start decryption");
            _logger.Info("source: " + SourceFilepath);
            _logger.Info("destination: " + TargetFilepath);
            _logger.Info("sender: " + ContactName);

            if (IsVaildFile())
            {
                MasterLogin login = _database.GetAll<MasterLogin>().FirstOrDefault();

                if (login != null)
                {
                    bool isDecryped = _cryptographyService.DecryptFile(_sourceFilepath, _targetFilepath,
                        login.PrivateKey);

                    if (isDecryped)
                    {
                        _messenger.Send(new DecryptionSuccsessMsg());
                    }
                    else
                    {
                        _logger.Error("file can not decrypt!");
                        _messenger.Send(new DecryptionFailedMsg());
                    }

                    //reset inputs
                    SourceFilepath = string.Empty;
                    TargetFilepath = string.Empty;
                    ContactName = string.Empty;
                }
                else
                {
                    _logger.Error("login is null!");
                }
            }
            else
            {
                _logger.Error("file is not vaild for encryption! maybe encrpytion?");
                _messenger.Send(new DecryptionFailedMsg());
            }

            _disableForEncryption = true;
            _isProcessInProgress = false;
        }

        private void OnChooseTargetConfirmMsg(ChooseTargetConfirmMsg msg)
        {
            _logger.Info("setting target paht to: " + msg.Filename);
            TargetFilepath = msg.Filename;
        }

        private void OnChooseSourceConfirmMsg(ChooseSourceConfirmMsg msg)
        {
            _logger.Info("setting source paht to: " + msg.Filename);
            SourceFilepath = msg.Filename;
        }

        private void OnContactSelectedMsg(ContactSelectedMsg msg)
        {
            _logger.Info("contact selected for en-/decryption: " + msg.Contact.Name);
            ContactName = msg.Contact.Name;
            PublicKey = msg.Contact.PublicKey;
        }

        private bool IsVaildFile()
        {
            bool isVaild = false;
            string extension = Path.GetExtension(_sourceFilepath);

            if (extension != null)
            {
                _logger.Info("check extension");
                if (extension.ToLower() == SecureFileShareExtension.ToLower())
                {
                    _logger.Info("extension check success");

                    _logger.Info("content check");
                    var encryptedFile = BinarySerializer.Deserialize<EncryptedFile>(_sourceFilepath);

                    if (encryptedFile != null)
                    {
                        _logger.Info("content check success");
                        isVaild = true;
                    }
                    else
                    {
                        _logger.Error("content check failed");
                    }
                }
                else
                {
                    _logger.Info("extension check faild");
                }
            }
            else
            {
                _logger.Error("extension is null!");
            }

            return isVaild;
        }

        #endregion Private Methods

        public RSAParameters PublicKey { get; set; }
    }
}