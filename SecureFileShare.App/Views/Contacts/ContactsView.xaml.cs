using System;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Messages;
using SecureFileShare.App.ViewModels.Contacts;
using SecureFileShare.App.Views.Interfaces;

namespace SecureFileShare.App.Views.Contacts
{
    /// <summary>
    ///     Interaction logic for ContactsView.xaml
    /// </summary>
    public partial class ContactsView : ICloseable
    {
        private readonly IMessenger _messenger;

        public ContactsView(IMessenger messenger)
        {
            InitializeComponent();

            _messenger = messenger;

            _messenger.Register<DeleteContactRequestMsg>(this, OnDeleteContactRequestMsg);
        }

        [Dependency]
        public ContactsViewModel ViewModel
        {
            set { DataContext = value; }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //unregister messages
            var viewmodel = (ContactsViewModel) DataContext;
            viewmodel.OnClosing();

            _messenger.Unregister<DeleteContactRequestMsg>(this, OnDeleteContactRequestMsg);

            _messenger.Send(new ContactsViewClosedMsg());
        }

        #region Private Methods

        private void OnDeleteContactRequestMsg(DeleteContactRequestMsg obj)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Are you sure, that you want to delete that contact?",
                "Security Check",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialogResult == MessageBoxResult.Yes)
            {
                _messenger.Send(new DeleteContactConfirmMsg());
            }
        }

        #endregion Private Methods
    }
}