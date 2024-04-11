namespace MagicVilla_VillaAPI.Models
{
    public class LocalUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; } //usually encrypted, but we just need to know basic of authenticating
        public string Role { get; set; }
    }
}
