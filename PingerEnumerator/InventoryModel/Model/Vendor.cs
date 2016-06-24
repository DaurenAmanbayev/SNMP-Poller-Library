using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    public class Vendor
    {
        public Vendor()
        {
            Nodes=new HashSet<Node>();
          //  Keys=new HashSet<DataKey>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Node> Nodes { get; set; }
      //  public virtual ICollection<DataKey> Keys { get; set; }
    }
}
