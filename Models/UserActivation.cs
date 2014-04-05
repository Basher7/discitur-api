using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mag14.discitur.Models
{
    [Table("discitur.UserActivation")]
    public class UserActivation
    {
        [Key]
        public string UserName { get; set; }
        [Required]
        public string Key { get; set; }

    }
}