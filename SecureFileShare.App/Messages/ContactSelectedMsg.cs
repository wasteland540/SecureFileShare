using SecureFileShare.Model;

namespace SecureFileShare.App.Messages
{
    public class ContactSelectedMsg
    {
        public ContactSelectedMsg(Contact contact)
        {
            Contact = contact;
        }

        public Contact Contact { get; private set; }
    }
}