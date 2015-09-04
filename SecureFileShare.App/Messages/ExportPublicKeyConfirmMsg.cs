namespace SecureFileShare.App.Messages
{
    public class ExportPublicKeyConfirmMsg
    {
        public ExportPublicKeyConfirmMsg(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; private set; }
    }
}