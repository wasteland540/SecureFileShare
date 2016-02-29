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
            _messenger.Register<AssignNewKeysSuccsessMsg>(this, OnAssignNewKeysSuccsess);
            _messenger.Register<ExportPublicKeyRequestMsg>(this, OnExportPublicKeyRequestMsg);
            _messenger.Register<ExportPublicKeySuccsessMsg>(this, OnExportPublicKeySuccsessMsg);
            _messenger.Register<ExportPublicKeyFailedMsg>(this, OnExportPublicKeyFailedMsg);
            _messenger.Register<ChooseSourceRequestMsg>(this, OnChooseSourceRequestMsg);
            _messenger.Register<ChooseTargetRequestMsg>(this, OnChooseTargetRequestMsg);
            _messenger.Register<EncryptionSuccsessMsg>(this, OnEncryptionSuccsessMsg);
            _messenger.Register<DecryptionSuccsessMsg>(this, OnDecryptionSuccsessMsg);
            _messenger.Register<DecryptionFailedMsg>(this, OnDecryptionFailedMsg);
            _messenger.Register<FileSizeNotSupportedMsg>(this, OnFileSizeNotSupportedMsg);
            _messenger.Register<SourceTargetInvaildMsg>(this, OnSourceTargetInvaildMsg);
            _messenger.Register<UploadToDropboxSuccessfulMsg>(this, OnUploadToDropboxSuccessfulMsg);
            _messenger.Register<StartUploadToDropboxMsg>(this, OnStartUploadToDropboxMsg);
            _messenger.Register<ZipFilesStartMsg>(this, OnZipFilesStartMsg);
            _messenger.Register<NoDirectorySupportedMsg>(this, OnNoDirectorySupportedMsg);
        }

        [Dependency]
        public MainViewModel ViewModel
        {
            set { DataContext = value; }
        }

        #region Private Methods

        private void OnExportPublicKeyFailedMsg(ExportPublicKeyFailedMsg msg)
        {
            MessageBox.Show(
                "Something went wrong, please try again and start the application with admin access!", "Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnExportPublicKeySuccsessMsg(ExportPublicKeySuccsessMsg msg)
        {
            MessageBox.Show(
                "Key file exported!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void OnExportPublicKeyRequestMsg(ExportPublicKeyRequestMsg msg)
        {
            var saveFileDialog = new SaveFileDialog {DefaultExt = "key", Filter = "Public Key File (*.key)| *.key"};
            bool? dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                string filename = saveFileDialog.FileName;
                _messenger.Send(new ExportPublicKeyConfirmMsg(filename));
            }
        }

        private void OnAssignNewKeysSuccsess(AssignNewKeysSuccsessMsg msg)
        {
            MessageBox.Show(
                "Now, you have new keys!\nPlease inform your friend and send them your new public key file!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void OnAssignNewKeysRequestMsg(AssignNewKeysRequestMsg msg)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure, you want a new key pair?\nYou have to inform your friends and have to send them your new public key file.",
                "Security Check", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _messenger.Send(new AssignNewKeysConfirmMsg());
            }
        }

        private void OnChooseTargetRequestMsg(ChooseTargetRequestMsg msg)
        {
            var saveFileDialog = new SaveFileDialog();
            bool? dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                string filename = saveFileDialog.FileName;
                _messenger.Send(new ChooseTargetConfirmMsg(filename));
            }
        }

        private void OnChooseSourceRequestMsg(ChooseSourceRequestMsg msg)
        {
            var openFileDialog = new OpenFileDialog {Multiselect = true};
            bool? dialogResult = openFileDialog.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                string[] filenames = openFileDialog.FileNames;
                _messenger.Send(new ChooseSourceConfirmMsg(filenames));
            }
        }

        private void OnDecryptionFailedMsg(DecryptionFailedMsg msg)
        {
            MessageBox.Show(
                "Something went wrong, please try again and start the application with admin access!", "Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnDecryptionSuccsessMsg(DecryptionSuccsessMsg msg)
        {
            MessageBox.Show(
                "File decrypted!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void OnEncryptionSuccsessMsg(EncryptionSuccsessMsg msg)
        {
            MessageBox.Show(
                "File encrypted!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void OnFileSizeNotSupportedMsg(FileSizeNotSupportedMsg msg)
        {
            MessageBox.Show(
                "You can only en-/decrypt files with a size less or equal 100MB!", "File size not supported",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void OnSourceTargetInvaildMsg(SourceTargetInvaildMsg msg)
        {
            MessageBox.Show(
                "Please check the source and target path!", "Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnUploadToDropboxSuccessfulMsg(UploadToDropboxSuccessfulMsg msg)
        {
            MessageBox.Show(
                "Uploading to Dropbox completed.", "Uploading completed",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnStartUploadToDropboxMsg(StartUploadToDropboxMsg msg)
        {
            MessageBox.Show(
                "Start uploading to Dropbox.", "Start uploading",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnZipFilesStartMsg(ZipFilesStartMsg msg)
        {
            MessageBox.Show(
                "We will zip your selected files now, this can take some moments.", "Zipping Files",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnNoDirectorySupportedMsg(NoDirectorySupportedMsg msg)
        {
            MessageBox.Show(
                "You can only encrypt multiple file, but no directories!", "Directory not supported",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        #endregion Private Methods
    }
}