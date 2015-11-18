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
    ///     Interaction logic for ChangePasswordView.xaml
    /// </summary>
    public partial class ChangePasswordView : ICloseable
    {
        private readonly IMessenger _messenger;

        public ChangePasswordView(IMessenger messenger)
        {
            InitializeComponent();

            _messenger = messenger;
            _messenger.Register<PasswordChangeMsg>(this, OnPasswordChangeMsgMessage);
        }

        [Dependency]
        public ChangePasswordViewModel ViewModel
        {
            set { DataContext = value; }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //unregister messages
            _messenger.Unregister<PasswordChangeMsg>(this, OnPasswordChangeMsgMessage);

            //send close msg
            _messenger.Send(new AddEditContactViewClosedMsg());
        }

        #region Private Methods

        private void OnPasswordChangeMsgMessage(PasswordChangeMsg passwordChangeMsg)
        {
            MessageBox.Show("Your changes are saved!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion Private Methods
    }
}