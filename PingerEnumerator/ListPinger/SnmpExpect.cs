using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ListPinger
{
    class SnmpExpect
    {
        OctetString community;
        IPEndPoint server;
        List<Variable> variableList = new List<Variable>();
        List<string> Messages=new List<string>();
        public SnmpExpect(string community, string ipAddress)
        {
           this.community = new OctetString(community);
           server = new IPEndPoint(IPAddress.Parse(ipAddress), 161);
        }
        //-----------------------------------------------------------------------------
        //SNMPGET
        public void SnmpGet(string oidKey)
        {
            var result = Messenger.Get(VersionCode.V2,
                server,
                community,
                new List<Variable> { new Variable(new ObjectIdentifier(oidKey)) },
                 60000);

            variableList = result.ToList();
        }
        public void SnmpGet(string community, string ipAddress, string oidKey)
        {
            var result = Messenger.Get(VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(ipAddress), 161),
                new OctetString(community),
                new List<Variable> { new Variable(new ObjectIdentifier(oidKey)) },
                 60000);

            variableList = result.ToList();
        }
        //----------------------------------------------------------------------------
        //SNMPGETBULK
        public void SnmpGetBulk(string oidKey)
        {

            GetBulkRequestMessage message = new GetBulkRequestMessage(0,
                              VersionCode.V2,
                              community,
                              0,
                              10,
                              new List<Variable> { new Variable(new ObjectIdentifier(oidKey)) });
            ISnmpMessage response = message.GetResponse(6000, server);
            if (response.Pdu().ErrorStatus.ToInt32() != 0)
            {
                throw ErrorException.Create(
                    "Error in response",
                    server.Address,
                    response);
            }

            var result = response.Pdu().Variables;

            variableList = result.ToList();
            
        }
        public void SnmpGetBulk(string community, string ipAddress, string oidKey)
        {

            GetBulkRequestMessage message = new GetBulkRequestMessage(0,
                              VersionCode.V2,
                              new OctetString(community),
                              0,
                              10,
                              new List<Variable> { new Variable(new ObjectIdentifier(oidKey)) });
            ISnmpMessage response = message.GetResponse(6000, new IPEndPoint(IPAddress.Parse(ipAddress), 161));
            if (response.Pdu().ErrorStatus.ToInt32() != 0)
            {
                throw ErrorException.Create(
                    "Error in response!",
                    IPAddress.Parse(ipAddress),
                    response);
            }

            var result = response.Pdu().Variables;

            variableList = result.ToList();

        }
        //-----------------------------------------------------------------------------
        //SNMPGETNEXT
        public void SnmpGetNext(string oidKey)
        {

            GetNextRequestMessage message = new GetNextRequestMessage(0,
                              VersionCode.V2,
                              community,
                              new List<Variable> { new Variable(new ObjectIdentifier(oidKey)) });
            ISnmpMessage response = message.GetResponse(6000, server);
            if (response.Pdu().ErrorStatus.ToInt32() != 0)
            {
                throw ErrorException.Create(
                    "Error in response!",
                    server.Address,
                    response);
            }

            var result = response.Pdu().Variables;

            variableList = result.ToList();
        }
        public void SnmpGetNext(string community, string ipAddress, string oidKey)
        {

            GetNextRequestMessage message = new GetNextRequestMessage(0,
                              VersionCode.V2,
                              new OctetString(community),
                              new List<Variable> { new Variable(new ObjectIdentifier(oidKey)) });
            ISnmpMessage response = message.GetResponse(6000, new IPEndPoint(IPAddress.Parse(ipAddress), 161));
            if (response.Pdu().ErrorStatus.ToInt32() != 0)
            {
                throw ErrorException.Create(
                    "Error in response!",
                    IPAddress.Parse(ipAddress),
                    response);
            }

            var result = response.Pdu().Variables;

            variableList = result.ToList();
        }
        //---------------------------------------------------------------------------
        //SNMPWALK
        public void SnmpWalk(string oidKey)
        {
            var result = new List<Variable>();
            Messenger.Walk(VersionCode.V2,
                           server,
                           community,
                           new ObjectIdentifier(oidKey),
                           result,
                           6000,
                           WalkMode.WithinSubtree);

            variableList = result.ToList();
        }
        public void SnmpWalk(string community, string ipAddress, string oidKey)
        {
            var result = new List<Variable>();
            Messenger.Walk(VersionCode.V2,
                           new IPEndPoint(IPAddress.Parse(ipAddress), 161),
                           new OctetString(community),
                           new ObjectIdentifier(oidKey),
                           result,
                           6000,
                           WalkMode.WithinSubtree);

            variableList = result.ToList();
        }
        //--------------------------------------------------------------------
        //SNMPBULKWALK
        //better perforamnce than snmpwalk
        //but loaded  more devices resources for response to this request
        public void SnmpBulkWalk(string oidKey)
        {
            var result = new List<Variable>();
            Messenger.BulkWalk(VersionCode.V2,
                               server,
                               community,
                               new ObjectIdentifier(oidKey),
                               result,
                               6000,
                               10,
                               WalkMode.WithinSubtree,
                               null,
                               null);
            variableList = result.ToList();
        }
        public void SnmpBulkWalk(string community, string ipAddress, string oidKey)
        {
            var result = new List<Variable>();
            Messenger.BulkWalk(VersionCode.V2,
                                new IPEndPoint(IPAddress.Parse(ipAddress), 161),
                               new OctetString(community),
                               new ObjectIdentifier(oidKey),
                               result,
                               6000,
                               10,
                               WalkMode.WithinSubtree,
                               null,
                               null);
            variableList = result.ToList();
        }
        //----------------------------------------------------------------
        //TRAP SENDING
        //необходимо реализовать передачу данных
        public void SendTrapV1(string community, string ipAddress, string oidKey)
        {
            Messenger.SendTrapV1(new IPEndPoint(IPAddress.Parse(ipAddress), 162),
                     IPAddress.Parse(ipAddress),
                     new OctetString(community),
                     new ObjectIdentifier(oidKey),
                     GenericCode.ColdStart,
                     0,
                     0,
                     new List<Variable>());

        }
        
        public void SendTrapV2(string community, string ipAddress, string oidKey)
        {
            Messenger.SendTrapV2(0,
                     VersionCode.V2,
                     new IPEndPoint(IPAddress.Parse(ipAddress), 162),
                     new OctetString(community),
                     new ObjectIdentifier(ipAddress),
                     0,
                     new List<Variable>());
        }

        public void SendInform(string community, string ipAddress, string oidKey)
        {
            Messenger.SendInform(0,
                     VersionCode.V2,
                     new IPEndPoint(IPAddress.Parse(ipAddress), 162),
                     new OctetString(community),
                     new ObjectIdentifier(oidKey),
                     0,
                     new List<Variable>(),
                     2000,
                     null,
                     null);
        }

        public void TrapListen()
        {
            Listener server = new Listener();           
            server.Start();
            server.MessageReceived += Message;

            Thread.Sleep(10000);
            server.Stop();        

        }
        private void Message(object sender, EventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(sender.ToString());
            builder.Append(" ");
            builder.Append(e);
            Messages.Add(builder.ToString());
        }
    }
}
