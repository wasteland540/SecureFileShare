using System;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Messages;
using SecureFileShare.App.ViewModels.Util;
using SecureFileShare.App.Views.Interfaces;

namespace SecureFileShare.App.Views.Util
{
    /// <summary>
    ///     Interaction logic for FileListView.xaml
    /// </summary>
    public partial class FileListView : ICloseable
    {
        private readonly IMessenger _messenger;

        public FileListView(IMessenger messenger)
        {
            InitializeComponent();

            _messenger = messenger;
            _messenger.Register<FileListCancelMsg>(this, OnFileListCancelMsg);
            _messenger.Register<SelectedFileIsNullOrEmptyMsg>(this, OnSelectedFileIsNullOrEmptyMsg);
        }

        [Dependency]
        public FileListViewModel ViewModel
        {
            set { DataContext = value; }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //unregister messages
            _messenger.Unregister<FileListCancelMsg>(this, OnFileListCancelMsg);

            //send close msg
            _messenger.Send(new FileListViewClosedMsg());
        }

        #region Private Methods

        private void OnFileListCancelMsg(FileListCancelMsg msg)
        {
            Close();
        }

        private void OnSelectedFileIsNullOrEmptyMsg(SelectedFileIsNullOrEmptyMsg msg)
        {
            MessageBox.Show(
                "You have not select a file.\nPlease select a file or click on cancel.", "Hint",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        #endregion Private Methods
    }
}