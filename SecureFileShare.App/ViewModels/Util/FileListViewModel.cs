using System;
using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using SecureFileShare.App.Commands;
using SecureFileShare.App.Messages;
using SecureFileShare.App.Services;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.Model;

namespace SecureFileShare.App.ViewModels.Util
{
    public class FileListViewModel : ViewModelBase
    {
        public enum CloudServiceType
        {
            Dropbox,
        }

        private readonly IDataAccessLayer _database;
        private readonly IDropboxService _dropboxService;
        private readonly ILog _logger = LogManager.GetLogger(typeof (FileListViewModel));
        private readonly IMessenger _messenger;
        private bool _block;
        private ICommand _cancelCommand;
        private ICommand _chooseCommand;
        private CloudServiceType _cloudService;
        private List<string> _fileList;
        private string _hint;
        private string _selectedFile;

        public FileListViewModel(IDataAccessLayer database, IDropboxService dropboxService, IMessenger messenger)
        {
            _database = database;
            _dropboxService = dropboxService;
            _messenger = messenger;
        }

        #region Properties

        public List<string> FileList
        {
            get
            {
                if (!_block)
                {
                    switch (CloudService)
                    {
                        case CloudServiceType.Dropbox:
                            _block = true;
                            _logger.Info("get dropbox access from database");
                            var dropboxAccess = _database.GetSingleByName<DropboxAccess>(DropboxAccess.ObjectName);

                            if (dropboxAccess != null)
                            {
                                _logger.Info("get file list from dropbox");
                                _dropboxService.GetFileList(dropboxAccess.AccessToken, OnGetDropboxFileListSuccess);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return _fileList;
            }
            set
            {
                if (value != null)
                {
                    _fileList = value;
                    RaisePropertyChanged("FileList");
                }
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                _cancelCommand = _cancelCommand ?? new DelegateCommand(Cancel);
                return _cancelCommand;
            }
        }

        public ICommand ChooseCommand
        {
            get
            {
                _chooseCommand = _chooseCommand ?? new DelegateCommand(Choose);
                return _chooseCommand;
            }
        }

        public string Hint
        {
            get { return _hint; }
            set
            {
                if (value != _hint)
                {
                    _hint = value;
                    RaisePropertyChanged("Hint");
                }
            }
        }

        public CloudServiceType CloudService
        {
            get { return _cloudService; }
            set
            {
                _cloudService = value;
                SetHint();
            }
        }

        public string SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                if (value != _selectedFile)
                {
                    _selectedFile = value;
                    RaisePropertyChanged("SelectedFile");
                }
            }
        }

        #endregion Properties

        #region Private Methods

        private void OnGetDropboxFileListSuccess(List<string> fileList)
        {
            FileList = fileList;
        }

        private void Cancel(object obj)
        {
            _messenger.Send(new FileListCancelMsg());
        }

        private void Choose(object obj)
        {
            if (!string.IsNullOrEmpty(_selectedFile))
            {
                switch (_cloudService)
                {
                    case CloudServiceType.Dropbox:
                        _logger.Info("get dropbox access from database");
                        var dropboxAccess = _database.GetSingleByName<DropboxAccess>(DropboxAccess.ObjectName);

                        if (dropboxAccess != null)
                        {
                            _logger.Info("download file from dropbox");
                            _dropboxService.DownloadFile(dropboxAccess.AccessToken, _selectedFile);

                            _messenger.Send(new FileListCancelMsg());
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                _messenger.Send(new SelectedFileIsNullOrEmptyMsg());
            }
        }

        private void SetHint()
        {
            switch (_cloudService)
            {
                case CloudServiceType.Dropbox:
                    Hint = "Dropbox\nNote: Only the files from our 'SecureFileShare' folder are listed!";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion Private Methods
    }
}