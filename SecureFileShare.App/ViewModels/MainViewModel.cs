using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using log4net;
using Microsoft.Practices.Unity;
using SecureFileShare.App.Commands;
using SecureFileShare.App.Messages;
using SecureFileShare.App.Services;
using SecureFileShare.App.ViewModels.Contacts;
using SecureFileShare.App.ViewModels.Util;
using SecureFileShare.App.Views.Contacts;
using SecureFileShare.App.Views.Help;
using SecureFileShare.App.Views.MyAccount;
using SecureFileShare.App.Views.Util;
using SecureFileShare.DataAccessLayer;
using SecureFileShare.Model;
using SecureFileShare.Util.FileIO;

namespace SecureFileShare.App.ViewModels
{
    public class MainViewModel : ViewModelBase, IDropTarget
    {
        private const string SecureFileShareExtension = ".sfs";
        private const string ZipFileExtension = "zip";
        private readonly ICryptographyService _cryptographyService;
        private readonly IDataAccessLayer _database;
        private readonly IDropboxService _dropboxService;
        private readonly ILog _logger = LogManager.GetLogger(typeof (MainViewModel));
        private readonly IMessenger _messenger;

        private ICommand _assignNewKeysCommand;
        private ICommand _changePasswordCommand;
        private ICommand _chooseContactCommand;
        private ICommand _chooseSourceCommand;
        private ICommand _chooseTargetCommand;
        private ICommand _connectDropboxCommand;
        private ConnectDropboxView _connectDropboxView;
        private string _contactName;
        private ContactsView _contactsView;
        private ICommand _decryptCommand;
        private bool _deleteTmpFile;
        private bool _disableForEncryption = true;
        private ICommand _encryptCommand;
        private ICommand _exitCommand;
        private ICommand _exportPublicKeyCommand;
        private FileListView _fileListView;
        private bool _hasAccessToDropbox;
        private HelpView _helpView;
        private bool _isProcessInProgress;
        private ICommand _openContactsCommand;
        private ICommand _openHelpCommand;
        private ICommand _showDropboxFilesCommand;
        private string _sourceFilepath;
        private string _targetFilepath;
        private bool _uploadToDropboxChecked;

        public MainViewModel(ICryptographyService cryptographyService, IMessenger messenger, IDataAccessLayer database,
            IDropboxService dropboxService)
        {
            _cryptographyService = cryptographyService;
            _messenger = messenger;
            _database = database;
            _dropboxService = dropboxService;

            //register for messages
            _messenger.Register<AssignNewKeysConfirmMsg>(this, OnAssignNewKeysConfirmMsg);
            _messenger.Register<ExportPublicKeyConfirmMsg>(this, OnExportPublicKeyConfirmMsg);
            _messenger.Register<ContactsViewClosedMsg>(this, OnContactsViewClosedMsg);
            _messenger.Register<ChooseSourceConfirmMsg>(this, OnChooseSourceConfirmMsg);
            _messenger.Register<ChooseTargetConfirmMsg>(this, OnChooseTargetConfirmMsg);
            _messenger.Register<ContactSelectedMsg>(this, OnContactSelectedMsg);
            _messenger.Register<ConnectDropboxViewClosedMsg>(this, OnConnectDropboxViewClosedMsg);
            _messenger.Register<DropboxAccessTokenSavedMsg>(this, OnDropboxAccessTokenSavedMsg);
            _messenger.Register<FileListViewClosedMsg>(this, OnFileListViewClosedMsg);
            _messenger.Register<DropboxFileDownloadFinishedMsg>(this, OnDropboxFileDownloadFinishedMsg);

            _messenger.Register<EncryptionSuccsessMsg>(this, OnEncryptionSuccsessMsg);

            CheckDropboxAccess();
        }

        #region Properties

        public ICommand ExitCommand
        {
            get
            {
                _exitCommand = _exitCommand ?? new DelegateCommand(Exit);
                return _exitCommand;
            }
        }

        public ICommand OpenContactsCommand
        {
            get
            {
                _openContactsCommand = _openContactsCommand ?? new DelegateCommand(OpenContacts);
                return _openContactsCommand;
            }
        }

        public ICommand ChangePasswordCommand
        {
            get
            {
                _changePasswordCommand = _changePasswordCommand ?? new DelegateCommand(ChangePassword);
                return _changePasswordCommand;
            }
        }

        public ICommand AssignNewKeysCommand
        {
            get
            {
                _assignNewKeysCommand = _assignNewKeysCommand ?? new DelegateCommand(AssignNewKeys);
                return _assignNewKeysCommand;
            }
        }

        public ICommand ExportPublicKeyCommand
        {
            get
            {
                _exportPublicKeyCommand = _exportPublicKeyCommand ?? new DelegateCommand(ExportPublicKey);
                return _exportPublicKeyCommand;
            }
        }

        public ICommand ConnectDropboxCommand
        {
            get
            {
                _connectDropboxCommand = _connectDropboxCommand ?? new DelegateCommand(OpenConnectDropboxView);
                return _connectDropboxCommand;
            }
        }

        public ICommand ShowDropboxFilesCommand
        {
            get
            {
                _showDropboxFilesCommand = _showDropboxFilesCommand ?? new DelegateCommand(ShowDropboxFilesView);
                return _showDropboxFilesCommand;
            }
        }

        public string SourceFilepath
        {
            get { return _sourceFilepath; }
            set
            {
                if (value != null && value != _sourceFilepath)
                {
                    _sourceFilepath = value;
                    DisableForEncryption = !IsVaildFile();
                    RaisePropertyChanged("SourceFilepath");
                }
            }
        }

        public string TargetFilepath
        {
            get { return _targetFilepath; }
            set
            {
                if (value != null && value != _targetFilepath)
                {
                    _targetFilepath = value;
                    RaisePropertyChanged("TargetFilepath");
                }
            }
        }

        public ICommand ChooseSourceCommand
        {
            get
            {
                _chooseSourceCommand = _chooseSourceCommand ?? new DelegateCommand(ChooseSource);
                return _chooseSourceCommand;
            }
        }

        public ICommand ChooseTargetCommand
        {
            get
            {
                _chooseTargetCommand = _chooseTargetCommand ?? new DelegateCommand(ChooseTarget);
                return _chooseTargetCommand;
            }
        }

        public ICommand ChooseContactCommand
        {
            get
            {
                _chooseContactCommand = _chooseContactCommand ?? new DelegateCommand(ChooseContact);
                return _chooseContactCommand;
            }
        }

        public ICommand EncryptCommand
        {
            get
            {
                _encryptCommand = _encryptCommand ?? new DelegateCommand(Encrypt);
                return _encryptCommand;
            }
        }

        public ICommand DecryptCommand
        {
            get
            {
                _decryptCommand = _decryptCommand ?? new DelegateCommand(Decrypt);
                return _decryptCommand;
            }
        }

        public string ContactName
        {
            get { return _contactName; }
            set
            {
                if (value != null && value != _contactName)
                {
                    _contactName = value;
                    RaisePropertyChanged("ContactName");
                }
            }
        }

        public bool DisableForEncryption
        {
            get { return _disableForEncryption; }
            set
            {
                _disableForEncryption = value;
                RaisePropertyChanged("DisableForEncryption");
            }
        }

        public ICommand OpenHelpCommand
        {
            get
            {
                _openHelpCommand = _openHelpCommand ?? new DelegateCommand(OpenHelp);
                return _openHelpCommand;
            }
        }

        public bool HasAccessToDropbox
        {
            get { return _hasAccessToDropbox; }
            set
            {
                _hasAccessToDropbox = value;
                RaisePropertyChanged("HasAccessToDropbox");
            }
        }

        public bool UploadToDropboxChecked
        {
            get { return _uploadToDropboxChecked; }
            set
            {
                _uploadToDropboxChecked = value;
                RaisePropertyChanged("UploadToDropboxChecked");
            }
        }

        #endregion Properties

        #region Private Methods

        public void DragOver(IDropInfo dropInfo)
        {
            IEnumerable<string> dragFileList = ((DataObject) dropInfo.Data).GetFileDropList().Cast<string>();
            IList<string> fileList = dragFileList as IList<string> ?? dragFileList.ToList();

            dropInfo.Effects = fileList.Any(item =>
            {
                string extension = Path.GetExtension(item);
                return extension != null;
            })
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        public void Drop(IDropInfo dropInfo)
        {
            IEnumerable<string> dragFileList = ((DataObject) dropInfo.Data).GetFileDropList().Cast<string>();
            IList<string> fileList = dragFileList as IList<string> ?? dragFileList.ToList();

            dropInfo.Effects = fileList.Any(item =>
            {
                string extension = Path.GetExtension(item);
                return extension != null;
            })
                ? DragDropEffects.Copy
                : DragDropEffects.None;

            if (dropInfo.Effects == DragDropEffects.Copy)
            {
                string fileName = fileList.FirstOrDefault();

                if (fileList.Count() > 1)
                {
                    fileName = HandlMultipleFiles(fileList, fileName);
                }

                if (CheckFileSize(fileName))
                {
                    SourceFilepath = fileName;
                }
                else
                {
                    if (_deleteTmpFile)
                    {
                        if (fileName != null)
                        {
                            _logger.Info("delete tmp zip file!");
                            File.Delete(fileName);
                        }
                    }

                    _logger.Warn("file size is greater than 100MB --> currently not supported");
                    _messenger.Send(new FileSizeNotSupportedMsg());
                }
            }
        }

        private void Exit(object obj)
        {
            if (!_isProcessInProgress)
            {
                _logger.Info("shutdown application");
                Application.Current.Shutdown();
            }
        }

        private void OpenContacts(object obj)
        {
            OpenContactView(false);
        }

        private void OpenContactView(bool isSelectionMode)
        {
            if (_contactsView == null)
            {
                _logger.Info("show contacts view");
                _contactsView = Container.Resolve<ContactsView>();
                _contactsView.Show();

                var viewmodel = (ContactsViewModel) _contactsView.DataContext;
                viewmodel.IsSelectionMode = isSelectionMode;
            }
            else
            {
                _logger.Warn("contacts view already open");
                _logger.Info("push view in foreground");

                var viewmodel = (ContactsViewModel) _contactsView.DataContext;
                viewmodel.IsSelectionMode = isSelectionMode;

                _contactsView.Focus();
            }
        }

        private void OpenHelp(object obj)
        {
            if (_helpView == null)
            {
                _logger.Info("show help view");
                _helpView = Container.Resolve<HelpView>();
                _helpView.Closed += delegate { _helpView = null; };
                _helpView.Show();
            }
            else
            {
                _logger.Warn("help view already open");
                _logger.Info("push view in foreground");

                _helpView.Focus();
            }
        }

        private void ChangePassword(object obj)
        {
            _logger.Info("show change password view");
            var changePasswordView = Container.Resolve<ChangePasswordView>();
            changePasswordView.ShowDialog();
        }

        private void AssignNewKeys(object obj)
        {
            _messenger.Send(new AssignNewKeysRequestMsg());
        }

        private void ExportPublicKey(object obj)
        {
            _messenger.Send(new ExportPublicKeyRequestMsg());
        }

        private void OnAssignNewKeysConfirmMsg(AssignNewKeysConfirmMsg msg)
        {
            _logger.Info("assign new keys confirmed --> assing new keys");
            MasterLogin login = _database.GetAll<MasterLogin>().FirstOrDefault();

            if (login != null)
            {
                _logger.Info("assign new keys");
                _cryptographyService.AssignNewKeys();

                login.PublicKey = _cryptographyService.GetPublicKey();
                login.PrivateKey = _cryptographyService.GetPrivateKeyAsXml();

                _logger.Info("update login with new keys");
                _database.Update(login);
                _logger.Info("login with new keys updated");

                _messenger.Send(new AssignNewKeysSuccsessMsg());
            }
            else
            {
                _logger.Error("login is null!");
            }
        }

        private void OnExportPublicKeyConfirmMsg(ExportPublicKeyConfirmMsg msg)
        {
            _isProcessInProgress = true;
            UIServices.SetBusyState();
            _logger.Info("export public key confirmed --> export key file");
            _logger.Info("export path: " + msg.Filename);

            MasterLogin login = _database.GetAll<MasterLogin>().FirstOrDefault();

            if (login != null)
            {
                bool success = _cryptographyService.ExportPublicKeyFile(msg.Filename, login.PublicKey);

                if (success)
                {
                    _logger.Info("export successful");
                    _messenger.Send(new ExportPublicKeySuccsessMsg());
                }
                else
                {
                    _logger.Error("export failed");
                    _messenger.Send(new ExportPublicKeyFailedMsg());
                }
            }
            else
            {
                _logger.Error("login is null!");
            }

            _isProcessInProgress = false;
        }

        private void OnContactsViewClosedMsg(ContactsViewClosedMsg msg)
        {
            _logger.Info("set contacts view to null");
            _contactsView = null;
        }

        private void ChooseSource(object obj)
        {
            _messenger.Send(new ChooseSourceRequestMsg());
        }

        private void ChooseTarget(object obj)
        {
            _messenger.Send(new ChooseTargetRequestMsg());
        }

        private void ChooseContact(object obj)
        {
            OpenContactView(true);
        }

        private void Encrypt(object obj)
        {
            _isProcessInProgress = true;
            UIServices.SetBusyState();

            _logger.Info("start encryption");
            _logger.Info("source: " + _sourceFilepath);
            _logger.Info("destination: " + _targetFilepath);
            _logger.Info("receipient: " + _contactName);

            if (CheckSoureAndTargetPath())
            {
                if (string.IsNullOrEmpty(ContactName))
                {
                    MasterLogin login = _database.GetAll<MasterLogin>().FirstOrDefault();

                    if (login != null)
                    {
                        _logger.Info("encrpyt file for myself");

                        _cryptographyService.EncryptFile(_sourceFilepath, _targetFilepath, login.PublicKey);
                        _messenger.Send(new EncryptionSuccsessMsg {TargetPath = _targetFilepath + ".sfs"});
                    }
                    else
                    {
                        _logger.Error("login is null!");
                    }
                }
                else
                {
                    _logger.Info("encrpyt file for: " + ContactName);

                    _cryptographyService.EncryptFile(_sourceFilepath, _targetFilepath, PublicKey);
                    _messenger.Send(new EncryptionSuccsessMsg {TargetPath = _targetFilepath});
                }

                _logger.Info("check if source file in temp directory and when it is, then delete");
                CheckSourceFileInTempDirectory();

                //reset inputs
                SourceFilepath = string.Empty;
                TargetFilepath = string.Empty;
                ContactName = string.Empty;
            }
            else
            {
                _logger.Error("source and/or target path are not vaild!");
                _messenger.Send(new SourceTargetInvaildMsg());
            }

            _disableForEncryption = true;
            _isProcessInProgress = false;
        }

        private void Decrypt(object obj)
        {
            _isProcessInProgress = true;
            UIServices.SetBusyState();

            _logger.Info("start decryption");
            _logger.Info("source: " + SourceFilepath);
            _logger.Info("destination: " + TargetFilepath);
            _logger.Info("sender: " + ContactName);

            if (CheckSoureAndTargetPath())
            {
                if (IsVaildFile())
                {
                    MasterLogin login = _database.GetAll<MasterLogin>().FirstOrDefault();

                    if (login != null)
                    {
                        bool isDecryped = _cryptographyService.DecryptFile(_sourceFilepath, _targetFilepath,
                            login.PrivateKey);

                        if (isDecryped)
                        {
                            _messenger.Send(new DecryptionSuccsessMsg());
                        }
                        else
                        {
                            _logger.Error("file can not decrypt!");
                            _messenger.Send(new DecryptionFailedMsg());
                        }

                        _logger.Info("check if source file in temp directory and when it is, then delete");
                        CheckSourceFileInTempDirectory();

                        //reset inputs
                        SourceFilepath = string.Empty;
                        TargetFilepath = string.Empty;
                        ContactName = string.Empty;
                    }
                    else
                    {
                        _logger.Error("login is null!");
                    }
                }
                else
                {
                    _logger.Error("file is not vaild for encryption! maybe encrpytion?");
                    _messenger.Send(new DecryptionFailedMsg());
                }
            }
            else
            {
                _logger.Error("source and/or target path are not vaild!");
                _messenger.Send(new SourceTargetInvaildMsg());
            }

            _disableForEncryption = true;
            _isProcessInProgress = false;
        }

        private void OnChooseTargetConfirmMsg(ChooseTargetConfirmMsg msg)
        {
            _logger.Info("setting target paht to: " + msg.Filename);
            TargetFilepath = msg.Filename;
        }

        private void OnChooseSourceConfirmMsg(ChooseSourceConfirmMsg msg)
        {
            string fileName = msg.Filenames.FirstOrDefault();

            if (msg.Filenames.Count() > 1)
            {
                fileName = HandlMultipleFiles(msg.Filenames, fileName);
            }

            if (CheckFileSize(fileName))
            {
                _logger.Info("setting source paht to: " + fileName);
                SourceFilepath = fileName;
            }
            else
            {
                _logger.Warn("file size is greater than 100MB --> currently not supported");
                _messenger.Send(new FileSizeNotSupportedMsg());
            }
        }

        private void OnContactSelectedMsg(ContactSelectedMsg msg)
        {
            _logger.Info("contact selected for en-/decryption: " + msg.Contact.Name);
            ContactName = msg.Contact.Name;
            PublicKey = msg.Contact.PublicKey;
        }

        private bool IsVaildFile()
        {
            bool isVaild = false;
            string extension = Path.GetExtension(_sourceFilepath);

            if (extension != null)
            {
                _logger.Info("check extension");
                if (extension.ToLower() == SecureFileShareExtension.ToLower())
                {
                    _logger.Info("extension check success");

                    _logger.Info("content check");
                    var encryptedFile = BinarySerializer.Deserialize<EncryptedFile>(_sourceFilepath);

                    if (encryptedFile != null)
                    {
                        _logger.Info("content check success");
                        isVaild = true;
                    }
                    else
                    {
                        _logger.Error("content check failed");
                    }
                }
                else
                {
                    _logger.Info("extension check faild");
                }
            }
            else
            {
                _logger.Error("extension is null!");
            }

            return isVaild;
        }

        private bool CheckFileSize(string filename)
        {
            var fileInfo = new FileInfo(filename);

            return fileInfo.Length <= (1024*1024*100); //100MB
        }

        private bool CheckSoureAndTargetPath()
        {
            bool isVaild = File.Exists(_sourceFilepath);

            if (isVaild)
            {
                string dir = Path.GetDirectoryName(_targetFilepath);

                isVaild = !string.IsNullOrEmpty(dir) && Directory.Exists(dir);
            }

            return isVaild;
        }

        private void OpenConnectDropboxView(object obj)
        {
            if (_connectDropboxView == null)
            {
                _logger.Info("show connect dropbox view");
                _connectDropboxView = Container.Resolve<ConnectDropboxView>();
                _connectDropboxView.Show();
            }
            else
            {
                _logger.Warn("connect dropbox view already open");
                _logger.Info("push view in foreground");

                _connectDropboxView.Focus();
            }
        }

        private void OnConnectDropboxViewClosedMsg(ConnectDropboxViewClosedMsg msg)
        {
            _logger.Info("set connect dropbox view to null");
            _connectDropboxView = null;
        }

        private void CheckDropboxAccess()
        {
            var dropboxAccess = _database.GetSingleByName<DropboxAccess>(DropboxAccess.ObjectName);

            if (dropboxAccess != null)
            {
                HasAccessToDropbox = true;
            }
        }

        private void OnDropboxAccessTokenSavedMsg(DropboxAccessTokenSavedMsg msg)
        {
            CheckDropboxAccess();
        }

        private void OnEncryptionSuccsessMsg(EncryptionSuccsessMsg msg)
        {
            if (_uploadToDropboxChecked)
            {
                _logger.Info("upload to Dropbox checked");

                _logger.Info("get dropbox access from database");
                var dropboxAccess = _database.GetSingleByName<DropboxAccess>(DropboxAccess.ObjectName);

                if (dropboxAccess != null)
                {
                    string filename = Path.GetFileName(msg.TargetPath);

                    _logger.Info("read file content in byte array");
                    byte[] content;
                    using (FileStream fileStream = File.OpenRead(msg.TargetPath))
                    {
                        content = new byte[fileStream.Length];

                        fileStream.Read(content, 0, (int) fileStream.Length);
                        fileStream.Close();
                    }

                    _messenger.Send(new StartUploadToDropboxMsg());
                    _logger.Info(string.Format("start uploading file:{0}", filename));

                    _dropboxService.Upload(dropboxAccess.AccessToken, filename, content, OnUploadSuccess);
                }
            }
        }

        private void OnUploadSuccess()
        {
            _messenger.Send(new UploadToDropboxSuccessfulMsg());
        }

        private void ShowDropboxFilesView(object obj)
        {
            if (_fileListView == null)
            {
                _logger.Info("show file list view");
                _fileListView = Container.Resolve<FileListView>();
                var viewModel = (FileListViewModel) _fileListView.DataContext;
                viewModel.CloudService = FileListViewModel.CloudServiceType.Dropbox;

                _fileListView.Show();
            }
            else
            {
                _logger.Warn("file list view already open");
                _logger.Info("push view in foreground");

                _fileListView.Focus();
            }
        }

        private void OnFileListViewClosedMsg(FileListViewClosedMsg msg)
        {
            _logger.Info("set file list view to null");
            _fileListView = null;
        }

        private void OnDropboxFileDownloadFinishedMsg(DropboxFileDownloadFinishedMsg msg)
        {
            _logger.Info("download from dropbox finished");
            string tmpFilePath = Path.Combine(Path.GetTempPath(), msg.Filename);
            _logger.Info("write file in temp directory: " + tmpFilePath);
            File.WriteAllBytes(tmpFilePath, msg.DownloadedFile);

            SourceFilepath = tmpFilePath;
        }

        private void CheckSourceFileInTempDirectory()
        {
            string directory = Path.GetDirectoryName(_sourceFilepath);

            if (directory == Path.GetDirectoryName(Path.GetTempPath()))
            {
                _logger.Info("file is in temp directory. delete it!");
                File.Delete(_sourceFilepath);
                _logger.Info("file deleted!");
            }
        }

        private string ZipFiles(IEnumerable<string> fileList)
        {
            _logger.Info("create tmp zip file");
            string tmpZipFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            //Creates a new, blank zip file to work with - the file will be
            //finalized when the using statement completes
            using (ZipArchive zipFile = ZipFile.Open(tmpZipFile, ZipArchiveMode.Create))
            {
                foreach (string filename in fileList)
                {
                    _logger.Info("add file to zip archive: " + Path.GetFileName(filename));
                    zipFile.CreateEntryFromFile(filename, Path.GetFileName(filename), CompressionLevel.Optimal);
                }
            }

            return tmpZipFile;
        }

        private static void CursorDefault()
        {
            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
        }

        private static void CursorWait()
        {
            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
        }

        private string HandlMultipleFiles(IList<string> fileList, string fileName)
        {
            if (fileName != null)
            {
                _logger.Info("check file list");

                if (CheckFileList(fileList))
                {
                    _logger.Info("file list ok");
                    _messenger.Send(new ZipFilesStartMsg());

                    CursorWait();
                    _logger.Info("start zipping files");
                    fileName = ZipFiles(fileList);
                    CursorDefault();

                    string oldFilename = fileName;
                    _logger.Info("change extension of tmp file to zip file");
                    fileName = Path.ChangeExtension(fileName, ZipFileExtension);
                    File.Move(oldFilename, fileName);

                    _deleteTmpFile = true;
                }
                else
                {
                    _messenger.Send(new NoDirectorySupportedMsg());
                }
            }

            return fileName;
        }

        private bool CheckFileList(IEnumerable<string> fileList)
        {
            return !fileList.Any(f => File.GetAttributes(f).HasFlag(FileAttributes.Directory));
        }

        #endregion Private Methods

        public RSAParameters PublicKey { get; set; }
    }
}