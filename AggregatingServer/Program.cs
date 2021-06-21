using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;

namespace AggregatingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AggregatingServer.Clients.InterfaceClient client = new AggregatingServer.Clients.InterfaceClient(1);

            client.Initialize(endPointUrl: "opc.tcp://192.168.0.110:40503");
            client.LoadConfiguration(configSectionName: "DefaultClient");
            client.CheckCertificate();

            client.Run().Wait();

            // Test aggragating server 
            AggregatingServer.Servers.AggregatingServer aggregatingServer = new AggregatingServer.Servers.AggregatingServer();
            aggregatingServer.Initialize(applicationName: "phi-ware Test Server");
            aggregatingServer.LoadConfiguration(configSectionName: "TestServer");
            aggregatingServer.CheckCertificate();
            aggregatingServer.Run().GetAwaiter().GetResult();


            aggregatingServer.baseAddresses.ForEach(s => Console.WriteLine("endpoint url: " + s));

            // Post-creation actions
            //aggregatingServer.PostCreate();

            // Aggregating 
            ushort ns0 = aggregatingServer.AddNamespaceUri("ns0:http://opcfoundation.org/UA/");
            ushort ns1 = aggregatingServer.AddNamespaceUri("ns1:urn:phi-ware.OPCUA-ICP.IOT2040");

            //ns1 = 2;

            // Create server object 
            //Opc.Ua.NodeId.Create(Opc.Ua.ObjectTypes.ServerType, Opc.Ua.Namespaces.OpcUa, namespaceUris);


            BaseInstanceState baseInstanceState = aggregatingServer.AddNode(ObjectIds.ObjectsFolder,
                "VM-UBU18-IOT:40503",
                "VM-UBU18-IOT:40503", 0, ns1, NodeClass.Object);

            Console.WriteLine("Hit any key to exit...");
            Console.ReadKey();

        }
    }
}
