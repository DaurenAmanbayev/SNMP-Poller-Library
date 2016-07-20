using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    public class OidKey
    {
        [Key]
        public int Id { get; set; }
        //уникальность ключа в таблице, добавить!!!
        [Index("IX_UniqueOidKey", 1, IsUnique = true), Required, MaxLength(100)]
        public string Key { get; set; }
        [Required, MaxLength(100)]
        public string Description { get; set; }
        [Index("IX_UniqueOidKey", 2, IsUnique = true), Required]
        public virtual Template Template { get; set; }
    }
}
