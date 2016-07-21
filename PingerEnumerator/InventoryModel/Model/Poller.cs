using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    public class Poller
    {
        //для планирования поллинга устройств
        public int Id { get; set; }
        public string PollName { get; set; }
        //--------------------------------
        public bool byCategory { get; set; }
        public virtual ICollection<Node> Nodes { get; set; }
    }
}
