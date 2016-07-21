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
  
        //public void Discovery(Node node)
        //{
        //    if (node.isDiscovered == false)
        //    {
        //        SnmpExpect expect=new SnmpExpect();
        //        Detail sysDetail=new Detail();
        //        sysDetail.Properties=expect.SnmpGet(node.Credential.RoCommunity, node.Address, Sysname);
        //        Detail descrDetail=new Detail();
        //        descrDetail.Properties=expect.SnmpGet(node.Credential.RoCommunity, node.Address, Sysdescription);
        //        Detail interfaceDetail=new Detail();
        //        interfaceDetail.Properties=expect.SnmpWalk(node.Credential.RoCommunity, node.Address, ifDescription);
        //        node.Details.Add(sysDetail);
        //        node.Details.Add(descrDetail);
        //        node.Details.Add(interfaceDetail);
        //        //using (InventoryContext context=new InventoryContext())
        //        //{
        //        //    context.Nodes.Attach(node);
        //        //    context.SaveChanges();
        //        //}
        //        node.isDiscovered = true;

        //        Console.WriteLine(sysDetail.Properties);
        //        Console.WriteLine(descrDetail.Properties);
        //        Console.WriteLine(interfaceDetail.Properties);
        //    }
        //}

        public void Discovery(Poller poller)
        {
            foreach (var node in poller.Nodes)
            {
                DiscoveryByNode(node);
            }
        }

        public void DiscoveryByNode(Node node)
        {
            SnmpExpect expect = new SnmpExpect();
            List<OidKey> list = BuildOidList(node);

            foreach (var oid in list)
            {
                Detail detail=new Detail();
                detail.OidKey = oid;
                detail.Node = node;
                detail.Properties = expect.SnmpGet(node.RoCommunity, node.Address, oid.Key);
                node.AddDetail(detail);
            }
            
            //using (InventoryContext context=new InventoryContext())
            //{
            //    context.Nodes.Attach(node);
            //    context.SaveChanges();
            //}
        }

        private List<OidKey> BuildOidList(Node node)
        {
            List<OidKey> list=new List<OidKey>();
            foreach (var template in node.Templates)
            {
                foreach (var key in template.Keys)
                {
                    if (list.Any(k => k.Key == key.Key))
                    {
                       //------------------------
                    }
                    else
                    {
                        list.Add(key);
                    }
                }
            }
        }
    }
}
