using System;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using SecureFileShare.App.Messages;
using SecureFileShare.App.ViewModels.Contacts;
using SecureFileShare.App.Views.Interfaces;

namespace SecureFileShare.App.Views.Contacts
{
    /// <summary>
    ///     Interaction logic for AddEditContactView.xaml
    /// </summary>
    public partial class AddEditContactView : ICloseable
    {
        private readonly IMessenger _messenger;

        public AddEditContactView(IMessenger messenger)
        {
            InitializeComponent();

            _messenger = messenger;
            _messenger.Register<AddEditContactRequestMsg>(this, OnAddEditContactRequestMsg);
            _messenger.Register<SaveContactMsg>(this, OnSaveContactMsg);
            _messenger.Register<ImportPublicKeyFileSuccuessMsg>(this, OnImportPublicKeyFileSuccuessMsg);
            _messenger.Register<ImportPublicKeyFileFailedMsg>(this, OnImportPublicKeyFileFailedMsg);
        }
        
        [Dependency]
        public AddEditContactViewModel ViewModel
        {
            set { DataContext = value; }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //unregister messages
            var viewmodel = (AddEditContactViewModel) DataContext;
            viewmodel.OnClosing();

            _messenger.Unregister<AddEditContactRequestMsg>(this, OnAddEditContactRequestMsg);
            _messenger.Unregister<SaveContactMsg>(this, OnSaveContactMsg);
            _messenger.Unregister<ImportPublicKeyFileSuccuessMsg>(this, OnImportPublicKeyFileSuccuessMsg);
            _messenger.Unregister<ImportPublicKeyFileFailedMsg>(this, OnImportPublicKeyFileFailedMsg);

            //send close msg
            _messenger.Send(new AddEditContactViewClosedMsg());
        }

        #region Private Methods

        private void OnAddEditContactRequestMsg(AddEditContactRequestMsg obj)
        {
            var openFileDialog = new OpenFileDialog { DefaultExt = "key", Filter = "Public Key File (*.key)| *.key" };
            var dialogResult = openFileDialog.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                var filename = openFileDialog.FileName;
                _messenger.Send(new AddEditContactConfirmMsg(filename));
            }
        }

        private void OnSaveContactMsg(SaveContactMsg obj)
        {
            MessageBox.Show("Contact data saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void OnImportPublicKeyFileFailedMsg(ImportPublicKeyFileFailedMsg obj)
        {
            MessageBox.Show("Something went wrong, please try again and start the application with admin access!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnImportPublicKeyFileSuccuessMsg(ImportPublicKeyFileSuccuessMsg obj)
        {
            MessageBox.Show("Public key file imported", "Success", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        #endregion Private Methods
    }
}