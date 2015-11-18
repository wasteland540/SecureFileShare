namespace SecureFileShare.Model
{
    public class DropboxAccess : IDbObject
    {
        public static string ObjectName = "SecureFileShare.Model.DropboxAccess";

        public DropboxAccess()
        {
            Name = ObjectName;
        }

        public string AccessToken { get; set; }
        public string Name { get; set; }
    }
}