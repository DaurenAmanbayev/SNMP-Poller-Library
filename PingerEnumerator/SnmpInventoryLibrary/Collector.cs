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
        
        public void Discovery(Poller poller)
        {
            foreach (var node in poller.Nodes)
            {
                Node temp=DiscoveryByNode(node);
                //сохранение результатов в БД
                using (InventoryContext context = new InventoryContext())
                {
                    context.Nodes.Attach(temp);
                    context.SaveChanges();
                }
            }
        }

        public Node DiscoveryByNode(Node node)
        {
            SnmpExpect expect = new SnmpExpect();
            List<OidKey> list = BuildOidList(node);

            foreach (var oid in list)
            {
                Detail detail = new Detail
                {
                    OidKey = oid,
                    Node = node,
                    Properties = expect.SnmpGet(node.RoCommunity, node.Address, oid.Key)
                };
                node.AddDetail(detail);
            }
            return node;
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
