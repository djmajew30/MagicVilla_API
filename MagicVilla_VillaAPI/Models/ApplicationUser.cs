using Microsoft.AspNetCore.Identity;

namespace MagicVilla_VillaAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        //these columns will be added to Identity user default 
        public string Name { get; set; }
    }
}
