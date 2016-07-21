using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryModel.Model;

namespace InventoryModel
{
    public class InventoryContext : DbContext
    {
        public InventoryContext() :
            base(@"data source=.\SQLEXPRESS;initial catalog=InventoryDB;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")
        {

        }

        public DbSet<Template> Templates { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<OidKey> OidKeys { get; set; }
        public DbSet<Poller> Pollers { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<Node> Nodes { get; set; }
    }
}
