using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models
{
    public class VillaNumber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)] //primary key but NOT an identity column (need 101,102, etc)
        public int VillaNo { get; set; }

        [ForeignKey("Villa")]
        public int VillaID { get; set; }
        public Villa Villa { get; set; }
        public string SpecialDetails { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
