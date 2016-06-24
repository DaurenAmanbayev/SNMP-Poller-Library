using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    public class Node
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public virtual Credential Credential { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual ICollection<Detail> Details { get; set; }
    }
}
