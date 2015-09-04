namespace SecureFileShare.App.Messages
{
    public class ChooseTargetConfirmMsg
    {
        public ChooseTargetConfirmMsg(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; private set; }
    }
}