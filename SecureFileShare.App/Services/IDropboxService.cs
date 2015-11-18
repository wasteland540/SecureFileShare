using System;
using System.Collections.Generic;

namespace SecureFileShare.App.Services
{
    public interface IDropboxService
    {
        void OpenDefaultBrowserAndAskForAccess();

        void ProcessCodeFlow(string accessCode, Action<string> onSuccess, Action<Exception> onFailure);

        void Upload(string accessToken, string filename, byte[] content, Action onUploadSuccess);

        string GetAccessToken();

        void GetFileList(string accessToken, Action<List<string>> onSuccess);
        
        void DownloadFile(string accessToken, string selectedFile);
    }
}