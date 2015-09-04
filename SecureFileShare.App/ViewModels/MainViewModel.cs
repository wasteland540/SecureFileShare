using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Commands;
using SecureFileShare.App.Messages;
using SecureFileShare.App.Services;
using SecureFileShare.App.Views.Contacts;
using SecureFileShare.App.Views.MyAccount;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.Model;

namespace SecureFileShare.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (MainViewModel));
        private readonly ICryptographyService _cryptographyService;
        private readonly IMessenger _messenger;
        private readonly IDataAccessLayer _database;

        private ContactsView _contactsView;

        private ICommand _exitCommand;
        private ICommand _openContactsCommand;
        private ICommand _changePasswordCommand;
        private ICommand _assignNewKeysCommand;
        private ICommand _exportPublicKeyCommand;

        public MainViewModel(ICryptographyService cryptographyService, IMessenger messenger, IDataAccessLayer database)
        {
            _cryptographyService = cryptographyService;
            _messenger = messenger;
            _database = database;

            //register for messages
            _messenger.Register<AssignNewKeysConfirmMsg>(this, OnAssignNewKeysConfirmMsg);
            _messenger.Register<ExportPublicKeyConfirmMsg>(this, OnExportPublicKeyConfirmMsg);
            _messenger.Register<ContactsViewClosedMsg>(this, OnContactsViewClosedMsg);
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

        #endregion Properties

        #region Private Methods

        private void Exit(object obj)
        {
            //TODO: check if is en-/decryption/export key is in progress? 

            _logger.Info("shutdown application");
            Application.Current.Shutdown();
        }

        private void OpenContacts(object obj)
        {
            if (_contactsView == null)
            {
                _logger.Info("show contacts view");
                _contactsView = Container.Resolve<ContactsView>();
                _contactsView.Show();
            }
            else
            {
                _logger.Warn("contacts view already open");
                _logger.Info("push view in foreground");

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
            var login = _database.GetAll<MasterLogin>().FirstOrDefault();

            if (login != null)
            {
                _logger.Info("assign new keys");
                _cryptographyService.AssignNewKeys();

                login.PublicKey = _cryptographyService.GetPublicKey();
                login.PrivateKey = _cryptographyService.GetPrivateKey();

                _logger.Info("update login with new keys");
                _database.Update(login);
                _logger.Info("login with new keys updated");

                _messenger.Send(new AssignNewKeysSuccsess());
            }
            else
            {
                _logger.Error("login is null!");
            }
        }

        private void OnExportPublicKeyConfirmMsg(ExportPublicKeyConfirmMsg msg)
        {
            _logger.Info("export public key confirmed --> export key file");
            _logger.Info("export path: " + msg.Filename);

            var login = _database.GetAll<MasterLogin>().FirstOrDefault();

            if (login != null)
            {
                var success = _cryptographyService.ExportPublicKeyFile(msg.Filename, login.PublicKey);

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
        }

        private void OnContactsViewClosedMsg(ContactsViewClosedMsg obj)
        {
            _logger.Info("set contacts view to null");
            _contactsView = null;
        }

        #endregion Private Methods

        
    }
}