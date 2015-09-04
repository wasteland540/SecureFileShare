namespace SecureFileShare.App.Messages
{
    public class AddEditContactConfirmMsg
    {
        public AddEditContactConfirmMsg(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; private set; }
    }
}