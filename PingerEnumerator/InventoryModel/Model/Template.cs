using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModel.Model
{
    public class Template
    {
        public Template()
        {
            Nodes=new HashSet<Node>();
            Keys=new HashSet<OidKey>();
        }

        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        [Required]
        public int TimeOut { get; set; }
        public virtual ICollection<Node> Nodes { get; set; }
        public virtual ICollection<OidKey> Keys { get; set; }
    }
}
