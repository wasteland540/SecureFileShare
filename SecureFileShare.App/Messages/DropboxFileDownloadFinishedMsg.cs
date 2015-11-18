namespace SecureFileShare.App.Messages
{
    public class DropboxFileDownloadFinishedMsg
    {
        public DropboxFileDownloadFinishedMsg(string filename, byte[] downloadedFile)
        {
            Filename = filename;
            DownloadedFile = downloadedFile;
        }

        public string Filename { get; set; }
        public byte[] DownloadedFile { get; set; }
    }
}