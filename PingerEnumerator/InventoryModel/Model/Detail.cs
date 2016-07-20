using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    //сдвоенный ключ для опроса устройств
    public class Detail
    {
        public int Id { get;set;}
        [Index("IX_UniqueDetails", 1, IsUnique = true)]
        public virtual OidKey DataKey { get; set; }
        public DateTime CollectDate { get; set; }
        public string Properties { get; set; }
        [Index("IX_UniqueDetails", 2, IsUnique = true)]
        public virtual Node Node { get; set; }
    }
}
