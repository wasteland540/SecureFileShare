using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using SecureFileShare.App.Messages;
using SecureFileShare.App.ViewModels;

namespace SecureFileShare.App.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView
    {
        private readonly IMessenger _messenger;

        public MainView(IMessenger messenger)
        {
            InitializeComponent();

            _messenger = messenger;
            _messenger.Register<AssignNewKeysRequestMsg>(this, OnAssignNewKeysRequestMsg);
            _messenger.Register<AssignNewKeysSuccsess>(this, OnAssignNewKeysSuccsess);
            _messenger.Register<ExportPublicKeyRequestMsg>(this, OnExportPublicKeyRequestMsg);
            _messenger.Register<ExportPublicKeySuccsessMsg>(this, OnExportPublicKeySuccsessMsg);
            _messenger.Register<ExportPublicKeyFailedMsg>(this, OnExportPublicKeyFailedMsg);
        }
        
        [Dependency]
        public MainViewModel ViewModel
        {
            set { DataContext = value; }
        }

        #region Private Methods

        private void OnExportPublicKeyFailedMsg(ExportPublicKeyFailedMsg obj)
        {
            MessageBox.Show(
                "Something went wrong, please try again and start the application with admin access!", "Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnExportPublicKeySuccsessMsg(ExportPublicKeySuccsessMsg obj)
        {
            MessageBox.Show(
                "Key file exported!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void OnExportPublicKeyRequestMsg(ExportPublicKeyRequestMsg obj)
        {
            var saveFileDialog = new SaveFileDialog { DefaultExt = "key", Filter = "Public Key File (*.key)| *.key" };
            var dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                var filename = saveFileDialog.FileName;
                _messenger.Send(new ExportPublicKeyConfirmMsg(filename));
            }
        }

        private void OnAssignNewKeysSuccsess(AssignNewKeysSuccsess obj)
        {
            MessageBox.Show(
                "Now, you have new keys!\nPlease inform your friend and send them your new public key file!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void OnAssignNewKeysRequestMsg(AssignNewKeysRequestMsg obj)
        {
            var result = MessageBox.Show(
                "Are you sure, you want a new key pari?\nYou have to inform your friend and have to send them your new public key file.",
                "Security Check", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _messenger.Send(new AssignNewKeysConfirmMsg());
            }
        }

        #endregion Private Methods
    }
}