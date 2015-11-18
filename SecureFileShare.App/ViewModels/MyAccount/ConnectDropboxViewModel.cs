using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using SecureFileShare.App.Commands;
using SecureFileShare.App.Messages;
using SecureFileShare.App.Services;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.Model;

namespace SecureFileShare.App.ViewModels.MyAccount
{
    public class ConnectDropboxViewModel : ViewModelBase
    {
        private readonly IDataAccessLayer _database;
        private readonly IDropboxService _dropboxService;
        private readonly ILog _logger = LogManager.GetLogger(typeof (ConnectDropboxViewModel));
        private readonly IMessenger _messenger;

        private ICommand _accessRequestCommand;
        private string _accessToken;

        private ICommand _saveCommand;

        public ConnectDropboxViewModel(IDataAccessLayer database, IMessenger messenger, IDropboxService dropboxService)
        {
            _database = database;
            _messenger = messenger;
            _dropboxService = dropboxService;
        }

        #region Properties

        public ICommand AccessRequestCommand
        {
            get
            {
                _accessRequestCommand = _accessRequestCommand ?? new DelegateCommand(AccessRequest);
                return _accessRequestCommand;
            }
        }

        public string AccessToken
        {
            get { return _accessToken; }
            set
            {
                if (value != null && value != _accessToken)
                {
                    _accessToken = value;
                    RaisePropertyChanged("AccessToken");
                }
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

        #endregion  Properties

        #region Private Methods

        private void AccessRequest(object obj)
        {
            _logger.Info("open default browser and ask for access via dropbox API2");
            _dropboxService.OpenDefaultBrowserAndAskForAccess();
        }

        private void Save(object obj)
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _logger.Info("process code flow via dropbox API2");
                _dropboxService.ProcessCodeFlow(_accessToken, OnSuccess, OnFailure);
            }
            else
            {
                _messenger.Send(new DropboxAccessTokenEmptyMsg());
            }
        }

        private void OnFailure(Exception exception)
        {
            _logger.Error("error by process code flow via dropbox API2: " + exception.Message);
            _logger.Error(exception.StackTrace);

            _messenger.Send(new DropboxProcessCodeFlowErrorMsg());
        }

        private void OnSuccess(string accessToken)
        {
            var dropboxAccess = _database.GetSingleByName<DropboxAccess>(DropboxAccess.ObjectName);

            if (dropboxAccess == null)
            {
                var dbAccess = new DropboxAccess
                {
                    AccessToken = _dropboxService.GetAccessToken()
                };

                _logger.Info("insert dropbox access token");
                _database.Insert(dbAccess);

                _messenger.Send(new DropboxAccessTokenSavedMsg());
            }
            else
            {
                _logger.Info("update dropbox access token");
                dropboxAccess.AccessToken = _dropboxService.GetAccessToken();
                _database.Update(dropboxAccess);

                _messenger.Send(new DropboxAccessTokenSavedMsg());
            }
        }

        #endregion Private Methods
    }
}