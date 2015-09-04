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
        public ChangePasswordView(IMessenger messenger)
        {
            InitializeComponent();

            messenger.Register<PasswordChangeMsg>(this, OnPasswordChangeMsgMessage);
        }

        [Dependency]
        public ChangePasswordViewModel ViewModel
        {
            set { DataContext = value; }
        }

        private void OnPasswordChangeMsgMessage(PasswordChangeMsg passwordChangeMsg)
        {
            MessageBox.Show("Your changes are saved!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}