using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryModel.Model;
using SnmpInventoryLibrary;

namespace ConsoleInventory
{
    class Program
    {
        static void Main(string[] args)
        {
            Collector collect=new Collector();
            Node node=new Node();
            node.Address = "10.10.1.1";
            node.RoCommunity = "public";
            collect.DiscoveryByNode(node);
            Console.ReadKey();
        }
    }
}
