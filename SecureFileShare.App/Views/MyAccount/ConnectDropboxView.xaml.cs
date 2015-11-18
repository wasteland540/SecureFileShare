using System;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Messages;
using SecureFileShare.App.ViewModels.MyAccount;
using SecureFileShare.App.Views.Interfaces;

namespace SecureFileShare.App.Views.MyAccount
{
    /// <summary>
    ///     Interaction logic for ConnectDropboxView.xaml
    /// </summary>
    public partial class ConnectDropboxView : ICloseable
    {
        private readonly IMessenger _messenger;

        public ConnectDropboxView(IMessenger messenger)
        {
            InitializeComponent();

            _messenger = messenger;
            _messenger.Register<DropboxAccessTokenEmptyMsg>(this, OnDropboxAccessTokenEmptyMsg);
            _messenger.Register<DropboxAccessTokenSavedMsg>(this, OnDropboxAccessTokenSavedMsg);
            _messenger.Register<DropboxProcessCodeFlowErrorMsg>(this, OnDropboxProcessCodeFlowErrorMsg);
            _messenger.Register<DropboxAPIAccessFaildMsg>(this, OnDropboxAPIAccessFaildMsg);
        }

        [Dependency]
        public ConnectDropboxViewModel ViewModel
        {
            set { DataContext = value; }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //unregister messages

            _messenger.Unregister<DropboxAccessTokenEmptyMsg>(this, OnDropboxAccessTokenEmptyMsg);
            _messenger.Unregister<DropboxAccessTokenSavedMsg>(this, OnDropboxAccessTokenSavedMsg);
            _messenger.Unregister<DropboxProcessCodeFlowErrorMsg>(this, OnDropboxProcessCodeFlowErrorMsg);
            _messenger.Unregister<DropboxAPIAccessFaildMsg>(this, OnDropboxAPIAccessFaildMsg);

            _messenger.Send(new ConnectDropboxViewClosedMsg());
        }

        #region Private Method

        private void OnDropboxAccessTokenEmptyMsg(DropboxAccessTokenEmptyMsg msg)
        {
            MessageBox.Show(
                "You did not paste the code from Dropbox in the textbox!", "Failed",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void OnDropboxAccessTokenSavedMsg(DropboxAccessTokenSavedMsg msg)
        {
            MessageBox.Show(
                "Your Dropbox access token was saved!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnDropboxProcessCodeFlowErrorMsg(DropboxProcessCodeFlowErrorMsg msg)
        {
            MessageBox.Show(
                "Something went wrong! \nMaybe you entered a wrong code from Dropbox? Please try again and start with step one.",
                "Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnDropboxAPIAccessFaildMsg(DropboxAPIAccessFaildMsg msg)
        {
            MessageBox.Show(
                "Dropbox API access failed! \nPlease contact the developer via http://sourceforge.net/projects/securefileshare/.",
                "Dropbox API access failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion Private Method
    }
}