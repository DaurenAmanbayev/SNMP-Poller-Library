using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    public class Category
    {
        public Category()
        {
            Templates=new HashSet<Template>();
            Nodes=new HashSet<Node>();
        }

        //для категоризации списка устройств
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Template> Templates { get; set; }
        public virtual ICollection<Node> Nodes { get; set; }
    }
}
