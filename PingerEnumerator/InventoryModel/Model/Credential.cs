using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    public class Credential
    {
        public Credential()
        {
            Nodes=new HashSet<Node>();
        }

        public int Id { get; set; }
        public string RoCommunity { get; set; }
        public string RwCommunity { get; set; }
        public virtual ICollection<Node> Nodes { get; set; }
    }
}
