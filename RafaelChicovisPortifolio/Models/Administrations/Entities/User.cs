using System.ComponentModel.DataAnnotations.Schema;
using RafaelChicovisPortifolio.Models.Entities;

namespace RafaelChicovisPortifolio.Models.Administrations.Entities
{
    [Table("User")]
    public class User : FullAuditedEntity
    {
        public string Password { get; set; }
        public string Key { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        
        public User()
        {
        }
    }
}
    
