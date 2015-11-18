using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Babel;
using Dropbox.Api.Files;
using Dropbox.Api.Users;

namespace SecureFileShare.Util.Dropbox
{
    public class DropboxWrapper
    {
        public FullAccount Account { get; private set; }
        public IList<Metadata> Files { get; private set; }
        public byte[] LastDownloadedFile { get; private set; }

        public string AccessToken { get; set; }

        public void OpenDefaultBrowserAndAskForAccess(string appKey)
        {
            //start code flow
            Uri authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(appKey);

            Process.Start(authorizeUri.ToString());
        }

        public async void ProcessCodeFlow(string appKey, string appSecret, Action<string> onSuccess,
            Action<Exception> onFailure)
        {
            //continue code flow
            try
            {
                OAuth2Response result = await DropboxOAuth2Helper.ProcessCodeFlowAsync(AccessToken, appKey, appSecret);

                AccessToken = result.AccessToken;
                onSuccess(AccessToken);
            }
            catch (Exception e)
            {
                onFailure(e);
            }
        }

        public async Task GetAccount()
        {
            using (var client = new DropboxClient(AccessToken))
            {
                Account = await client.Users.GetCurrentAccountAsync();
            }
        }

        public async Task Upload(string folder, string file, byte[] content, Action onSuccess)
        {
            using (var client = new DropboxClient(AccessToken))
            {
                using (var mem = new MemoryStream(content))
                {
                    await client.Files.UploadAsync(
                        folder + "/" + file,
                        WriteMode.Overwrite.Instance,
                        body: mem);

                    onSuccess();
                }
            }
        }

        public async Task LoadFileList()
        {
            using (var client = new DropboxClient(AccessToken))
            {
                ListFolderResult list = await client.Files.ListFolderAsync(string.Empty);
                Files = list.Entries;
            }
        }

        public async Task Download(string folder, string file)
        {
            using (var client = new DropboxClient(AccessToken))
            {
                using (IDownloadResponse<FileMetadata> response = await client.Files.DownloadAsync(folder + "/" + file))
                {
                    LastDownloadedFile = await response.GetContentAsByteArrayAsync();
                }
            }
        }

        public async Task GetFileList(Action<List<string>> onSuccess)
        {
            using (var client = new DropboxClient(AccessToken))
            {
                var fileList = await client.Files.ListFolderAsync("");

                var resultList = fileList.Entries.Where(i => i.IsFile).Select(item => item.Name).ToList();

                onSuccess(resultList);
            }
        }
    }
}