using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    public class Area
    {
        public Area()
        {
            Nodes=new HashSet<Node>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Node> Nodes { get; set; }
    }
}
