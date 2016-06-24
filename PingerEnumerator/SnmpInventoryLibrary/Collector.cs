using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryModel;
using InventoryModel.Model;

namespace SnmpInventoryLibrary
{
    //библиотека сбора данных по устройству
    public class Collector
    {
        string Sysname="1.3.6.1.2.1.1.5.0";
        string Sysdescription="1.3.6.1.2.1.1.1.0";
        string ifDescription="1.3.6.1.2.1.2.2.1.2";

        public void Discovery(Node node)
        {
            if (node.isDiscovered == false)
            {
                SnmpExpect expect=new SnmpExpect();
                Detail sysDetail=new Detail();
                sysDetail.Properties=expect.SnmpGet(node.Credential.RoCommunity, node.Address,Sysname);
                Detail descrDetail=new Detail();
                descrDetail.Properties=expect.SnmpGet(node.Credential.RoCommunity, node.Address, Sysdescription);
                Detail interfaceDetail=new Detail();
                interfaceDetail.Properties=expect.SnmpWalk(node.Credential.RoCommunity, node.Address, ifDescription);
                node.Details.Add(sysDetail);
                node.Details.Add(descrDetail);
                node.Details.Add(interfaceDetail);
                //using (InventoryContext context=new InventoryContext())
                //{
                //    context.Nodes.Attach(node);
                //    context.SaveChanges();
                //}
                node.isDiscovered = true;

                Console.WriteLine(sysDetail.Properties);
                Console.WriteLine(descrDetail.Properties);
                Console.WriteLine(interfaceDetail.Properties);
            }
        }
    }
}
