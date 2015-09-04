using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Commands;
using SecureFileShare.App.Messages;
using SecureFileShare.App.Services;
using SecureFileShare.App.Views.Interfaces;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.Model;

namespace SecureFileShare.App.ViewModels.MyAccount
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        private readonly ICryptographyService _cryptographyService;
        private readonly IDataAccessLayer _database;
        private readonly ILog _logger = LogManager.GetLogger(typeof (ChangePasswordViewModel));

        private ICommand _cancelCommand;
        private ICommand _confirmCommand;
        private string _errorMsg;

        public ChangePasswordViewModel(IDataAccessLayer database, ICryptographyService cryptographyService)
        {
            _database = database;
            _cryptographyService = cryptographyService;
        }

        #region Properties

        public ICommand ConfirmCommand
        {
            get
            {
                _confirmCommand = _confirmCommand ?? new DelegateCommand(Confirm);
                return _confirmCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                _cancelCommand = _cancelCommand ?? new DelegateCommand(Cancel);
                return _cancelCommand;
            }
        }

        public string ErrorMsg
        {
            get { return _errorMsg; }
            set
            {
                if (value != null && value != _errorMsg)
                {
                    _errorMsg = value;
                    RaisePropertyChanged("ErrorMsg");
                }
            }
        }

        #endregion Properties

        #region Privaate Methods

        private void Confirm(object obj)
        {
            UIServices.SetBusyState();
            _logger.Info("password change confirmed");

            _logger.Info("parsing parameters");
            var parameters = (object[]) obj;
            var oldPasswordBox = parameters[0] as PasswordBox;
            var newPasswordBox = parameters[1] as PasswordBox;
            var newPassword2Box = parameters[2] as PasswordBox;

            if (oldPasswordBox != null && newPasswordBox != null && newPassword2Box != null)
            {
                _logger.Info("parameters not null");

                _logger.Info("get login data");
                MasterLogin login = _database.GetAll<MasterLogin>().First();

                _logger.Info("hash entered password");
                byte[] hashedPassword = _cryptographyService.HashPassword(oldPasswordBox.Password, login.Salt);

                _logger.Info("compare entered and stored passwords");
                if (_cryptographyService.Compare(hashedPassword, login.Password))
                {
                    _logger.Info("password are correct");

                    string newPassword1 = newPasswordBox.Password;
                    string newPassword2 = newPassword2Box.Password;

                    _logger.Info("compare new passwords");
                    if (newPassword1 == newPassword2)
                    {
                        _logger.Info("hash new password");
                        byte[] newSalt = _cryptographyService.GenerateSalt();
                        byte[] newHashedPassword = _cryptographyService.HashPassword(newPassword1, newSalt);

                        login.Salt = newSalt;
                        login.Password = newHashedPassword;

                        _logger.Info("save changes");
                        _database.Update(login);
                        _logger.Info("changes are saved");

                        InformUserAndClose(parameters);
                    }
                    else
                    {
                        _logger.Error("new passwords do not match");
                        ErrorMsg = "Passwords do not match!";
                    }
                }
                else
                {
                    _logger.Error("current password is wrong!");
                    ErrorMsg = "Old Password is wrong!";
                }
            }
            else
            {
                _logger.Error("parameters are null!");
                ErrorMsg = "Something went wrong, pleas try again.";
            }
        }

        private void InformUserAndClose(object[] parameters)
        {
            _logger.Info("inform user");
            var messenger = Container.Resolve<IMessenger>();
            messenger.Send(new PasswordChangeMsg());

            _logger.Info("try to close view");
            var window = parameters[3] as ICloseable;

            if (window != null)
            {
                window.Close();
                _logger.Info("view closed");
            }
        }

        private void Cancel(object obj)
        {
            var window = obj as ICloseable;

            if (window != null)
            {
                window.Close();
            }
        }

        #endregion Privaate Methods
    }
}