using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    public class Node
    {
        public Node()
        {
            Details=new HashSet<Detail>();
            Templates=new HashSet<Template>();
        }

        public int Id { get; set; }
        public string Address { get; set; }
        public string RoCommunity { get; set; }
        public virtual ICollection<Template> Templates{ get; set; }
        public virtual ICollection<Detail> Details { get; set; }

        public void AddDetail(Detail detail)
        {
            //если у нас уже содержаться данные в списке, то заменить их
            if (Details.Any(d=>d.OidKey==detail.OidKey))
            {
                var temp= Details.FirstOrDefault(d => d.OidKey == detail.OidKey);
                temp.Properties = detail.Properties;
            }
            else
            {
                Details.Add(detail);
            }
        }
    }
}
