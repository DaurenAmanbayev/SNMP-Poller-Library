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

        public DbSet<Template> Areas { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<Node> Nodes { get; set; }
    }
}
