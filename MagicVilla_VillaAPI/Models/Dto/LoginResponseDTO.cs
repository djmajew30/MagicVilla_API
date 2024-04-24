namespace MagicVilla_VillaAPI.Models.Dto
{
    public class LoginResponseDTO
    {
        //public LocalUser User { get; set; } //removed for identity net
        
        //user and role are new in identity.net
        public UserDTO User { get; set; }
        //public string Role { get; set; } //removed 125
        public string Token { get; set; }
    }
}
