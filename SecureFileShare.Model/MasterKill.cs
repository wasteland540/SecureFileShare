namespace SecureFileShare.Model
{
    public class MasterKill : IDbObject
    {
        public byte[] Password { get; set; }
        public byte[] Salt { get; set; }
        public string Name { get; set; }
    }
}