using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Commands;
using SecureFileShare.App.Properties;
using SecureFileShare.App.Services;
using SecureFileShare.App.Views;
using SecureFileShare.App.Views.Interfaces;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.Model;

namespace SecureFileShare.App.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly ICryptographyService _cryptographyService;
        private readonly ILog _logger = LogManager.GetLogger(typeof (LoginViewModel));

        private bool _isFirstLogin;
        private bool _isStart = true;
        private string _login;
        private ICommand _loginCommand;
        private string _errorMsg;

        public LoginViewModel(ICryptographyService cryptographyService)
        {
            _cryptographyService = cryptographyService;
        }

        #region Properties

        public bool IsExpanded
        {
            get
            {
                if (_isStart)
                {
                    _isFirstLogin = Settings.Default.FirstLogin;
                    _isStart = false;
                }

                return _isFirstLogin;
            }

            set { _isFirstLogin = value; }
        }

        public string Login
        {
            get { return _login; }
            set
            {
                if (value != null && value != _login)
                {
                    _login = value;
                    RaisePropertyChanged("Login");
                }
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                _loginCommand = _loginCommand ?? new DelegateCommand(DoLogin);
                return _loginCommand;
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

        #region Private Methods

        private void DoLogin(object parameter)
        {
            UIServices.SetBusyState();
            _logger.Info("reset error msg");
            ErrorMsg = string.Empty;

            _logger.Info("starting with login process");

            if (!string.IsNullOrEmpty(_login) && _login.Length >= 6)
            {
                var values = (object[])parameter;
                var passwordBox = values[0] as PasswordBox;

                if (passwordBox != null)
                {
                    _logger.Info("password parameter is not null");

                    string password = passwordBox.Password;

                    if (!string.IsNullOrEmpty(password))
                    {
                        _logger.Info("password is not null or empty");

                        _logger.Info("check for exsisting login");
                        var database = Container.Resolve<IDataAccessLayer>();
                        List<MasterLogin> logins = database.GetAll<MasterLogin>();

                        if (logins.Count == 1)
                        {
                            var login = database.GetSingleByName<MasterLogin>(_login);

                            if (login != null)
                            {
                                _logger.Info("login do exsits");
                                byte[] hashedPassword = _cryptographyService.HashPassword(password, login.Salt);

                                _logger.Info("compare passwords...");
                                if (_cryptographyService.Compare(hashedPassword, login.Password))
                                {
                                    _logger.Info("...login verified.");

                                    OpenMainView(values);
                                }
                                else
                                {
                                    _logger.Error("...login failed.");
                                    ErrorMsg = "Login failed!";
                                }
                            }
                            else
                            {
                                _logger.Error("master login already exists");
                                ErrorMsg = "Master login already exists!";
                            }
                        }
                        else if (logins.Count == 0)
                        {
                            _logger.Info("login do not exsits");

                            byte[] salt = _cryptographyService.GenerateSalt();
                            byte[] hashedPassword = _cryptographyService.HashPassword(password, salt);

                            _cryptographyService.AssignNewKeys();

                            var masterLogin = new MasterLogin
                            {
                                Name = _login,
                                Password = hashedPassword,
                                Salt = salt,
                                PrivateKey = _cryptographyService.GetPrivateKey(),
                                PublicKey = _cryptographyService.GetPublicKey(),
                            };

                            database.Insert(masterLogin);

                            OpenMainView(values);
                        }
                    }
                    else
                    {
                        _logger.Error("password is null or empty!");
                        ErrorMsg = "Password is empty!";
                    }
                }
            }
            else
            {
                _logger.Error("login is null, empty or have less then 6 characters!");
                ErrorMsg = "Login have less then 6 characters!";
            }
        }

        private void OpenMainView(object[] values)
        {
            Settings.Default.FirstLogin = false;
            Settings.Default.Save();

            //open main view
            _logger.Info("open main view");
            var mainView = Container.Resolve<MainView>();
            mainView.Show();

            //close login view
            var loginWindow = values[1] as ICloseable;
            if (loginWindow != null)
            {
                _logger.Info("second parameter is not null");
                _logger.Info("close login view");
                loginWindow.Close();
            }
        }

        #endregion Private Methods
    }
}