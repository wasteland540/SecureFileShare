namespace SecureFileShare.App.Messages
{
    public class ChooseSourceConfirmMsg
    {
        public ChooseSourceConfirmMsg(string[] filenames)
        {
            Filenames = filenames;
        }

        public string[] Filenames { get; private set; }
    }
}