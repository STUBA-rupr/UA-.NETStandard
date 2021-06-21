using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AggregatingServer.Core;
using Opc.Ua;
using Opc.Ua.Server;

namespace AggregatingServer.Servers
{
    /// <summary>
    /// Represents aggregating server.
    /// </summary>
    public class AggregatingServer : Server
    {
        
        /// <summary>
        /// Server OPC UA base addresses
        /// </summary>
        public List<string> baseAddresses
        {
            get
            {
                return application.ApplicationConfiguration.ServerConfiguration.BaseAddresses;
            }
        }
        //protected ServerType serverType;
        public AggregatingServer()
        {
            ServerType serverType = new ServerType();
           
            server = serverType;            
        }

        /// <summary>
        /// Create address space structure for aggregating server
        /// </summary>
        public void PostCreate()
        {            
            AddNode(((ServerType)server).nodeManager.FindNodeInAddressSpace(ObjectIds.ObjectsFolder), (string)ObjectIdsAS.AggregatedServersFolder.Identifier,
                (string)ObjectIdsAS.AggregatedServersFolder.Identifier, ObjectIds.ObjectsFolder, ((ServerType)server).nodeManager.NamespaceIndex, NodeClass.Object);
        }

        /// <summary>
        /// Add uri to server namespace
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public ushort AddNamespaceUri(string uri)
        {
            ServerType pserver = (ServerType)server;

            // nodeManager = new ServerTypeNodeManager(server, configuration);            
            // nodeManagers.Add(nodeManager);
            ServerTypeNodeManager nodeManager;
            nodeManager = new ServerTypeNodeManager(pserver.CurrentInstance, uri);

            
            // this adds node manager to private MasterNodeManager.m_namespaceManagers
            //pserver.masterNodeManager.RegisterNamespaceManager(uri, nodeManager);
            // added according to https://github.com/OPCFoundation/UA-.NETStandard/issues/1080
            pserver.masterNodeManager.NodeManagers.Add(nodeManager);
            int index = pserver.CurrentInstance.NamespaceUris.GetIndex(uri);

            return (ushort)index;

            /*MasterNodeManager.RegisterNamespaceManager             
            return pserver.nodeManager.AddNamespaceUri(uri);*/
        }

        // TODO : prepisat podla PostCreate()
        public BaseInstanceState AddNode(NodeState parentNode, string path, string name, NodeId nodeType, ushort namespaceIndex, NodeClass nodeClass = NodeClass.Unspecified)
        {
            Dictionary<NodeId, IList<IReference>> externalReferences = new Dictionary<NodeId, IList<IReference>>();

            ServerType serverType = (ServerType)server;

            // get approprite node manager
            INodeManager nodemanager = serverType.masterNodeManager.NodeManagers[namespaceIndex];
            if(nodemanager.GetType() == typeof(ServerTypeNodeManager))
            {
                ServerTypeNodeManager serverTypeNodeManager = nodemanager as ServerTypeNodeManager;
            }

            

            // Create Folder for Aggregated servers
            BaseInstanceState root = serverType.nodeManager.AddNode(parentNode,
                    path, name, nodeType, namespaceIndex, nodeClass);

            // add references     
            // link root to objects folder.
            IList<IReference> references = null;
            if (!externalReferences.TryGetValue(parentNode.NodeId, out references))
            {
                externalReferences[parentNode.NodeId] = references = new List<IReference>();
            }
            references.Add(new NodeStateReference(parentNode.NodeId, false, root.NodeId));

            ((ServerType)server).AddExternalRefferences(externalReferences);            

            return root;
        }

        public BaseInstanceState AddNode (NodeId parentNodeId, string path, string name, NodeId nodeType, ushort namespaceIndex, NodeClass nodeClass = NodeClass.Unspecified)
        {
            Dictionary<NodeId, IList<IReference>> externalReferences = new Dictionary<NodeId, IList<IReference>>();

            ServerType serverType = (ServerType)server;
            //serverType.CurrentInstance.NodeManager

            /*
            // get approprite node manager
            INodeManager nodemanager = null;
            NodeId nodeId = new NodeId("ns=" + namespaceIndex + ";s=dummy");
            

            serverType.masterNodeManager.GetManagerHandle(nodeId, out nodemanager);
            if (nodemanager.GetType() == typeof(ServerTypeNodeManager))
            {
                ServerTypeNodeManager serverTypeNodeManager = nodemanager as ServerTypeNodeManager;
            }

            */

            NodeState parentNode = serverType.nodeManager.FindNodeInAddressSpace(parentNodeId);

            // Create Folder for Aggregated servers
            BaseInstanceState root = serverType.nodeManager.AddNode(parentNode,
                    path, name, nodeType, namespaceIndex, nodeClass);            

            
            // link root to objects folder.
            IList<IReference> references = null;
            if (!externalReferences.TryGetValue(parentNodeId, out references))
            {
                externalReferences[parentNodeId] = references = new List<IReference>();
            }
            references.Add(new NodeStateReference(parentNodeId, false, root.NodeId));

            ((ServerType)server).AddExternalRefferences(externalReferences);

            return root;

        }

    }

    /// <summary>
    /// Contains Aggregated server well known nodes
    /// </summary>
    public static  partial class ObjectIdsAS
    {
        public static readonly NodeId AggregatedServersFolder = new NodeId("ns=2;s=AggregatedServers");
    }
}
