namespace SecureFileShare.App.Messages
{
    public class ChooseSourceConfirmMsg
    {
        public ChooseSourceConfirmMsg(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; private set; }
    }
}