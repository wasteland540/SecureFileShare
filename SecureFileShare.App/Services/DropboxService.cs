using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using SecureFileShare.App.Messages;
using SecureFileShare.Security.APIs;
using SecureFileShare.Util.Dropbox;

namespace SecureFileShare.App.Services
{
    public class DropboxService : IDropboxService
    {
        private readonly DropboxWrapper _dropbox;
        private readonly ILog _logger = LogManager.GetLogger(typeof (DropboxService));
        private readonly IMessenger _messenger;

        private string _appKey = "";
        private string _appSecret = "";

        public DropboxService(IMessenger messenger)
        {
            _messenger = messenger;
            _dropbox = new DropboxWrapper();
        }

        public void OpenDefaultBrowserAndAskForAccess()
        {
            if (LoadApiAccess())
            {
                _logger.Info("opnen default browser and ask for access");
                _dropbox.OpenDefaultBrowserAndAskForAccess(_appKey);
            }
            else
            {
                _messenger.Send(new DropboxAPIAccessFaildMsg());
            }
        }

        public void ProcessCodeFlow(string accessCode, Action<string> onSuccess,
            Action<Exception> onFailure)
        {
            if (LoadApiAccess())
            {
                _dropbox.AccessToken = accessCode;

                _logger.Info("process code flow");
                _dropbox.ProcessCodeFlow(_appKey, _appSecret, onSuccess, onFailure);
            }
            else
            {
                _messenger.Send(new DropboxAPIAccessFaildMsg());
            }
        }

        public async void Upload(string accessToken, string filename, byte[] content, Action onUploadSuccess)
        {
            _logger.Info("set access token");
            _dropbox.AccessToken = accessToken;

            _logger.Info("start uploading to dropbox");
            await _dropbox.Upload("", filename, content, onUploadSuccess);
        }

        public string GetAccessToken()
        {
            return _dropbox.AccessToken;
        }

        public async void GetFileList(string accessToken, Action<List<string>> onSuccess)
        {
            _logger.Info("set access token");
            _dropbox.AccessToken = accessToken;

            _logger.Info("start uploading to dropbox");
            await _dropbox.GetFileList(onSuccess);
        }

        public async void DownloadFile(string accessToken, string selectedFile)
        {
            await _dropbox.Download("", selectedFile);

            _messenger.Send(new DropboxFileDownloadFinishedMsg(selectedFile, _dropbox.LastDownloadedFile));
        }

        private bool LoadApiAccess()
        {
            bool apiAccessLoaded = false;

            _logger.Info("load api access from encrypted file");

            try
            {
                var secretClassBuilder = new MyManagementClass();
                _appKey =
                    (string)
                        secretClassBuilder.CallMethod(
                            new byte[] {00},
                            " SecureFileShare.Secrets.DropboxApi", "GetKey", new object[0]);
                _appSecret =
                    (string)
                        secretClassBuilder.CallMethod(
                            new byte[] {00},
                            " SecureFileShare.Secrets.DropboxApi", "GetSecret", new object[0]);

                _logger.Info("api access loaded");
                apiAccessLoaded = true;
            }
            catch (Exception e)
            {
                _logger.Info("error loading api access from encrypted file: " + e.Message);
            }

            return apiAccessLoaded;
        }
    }
}