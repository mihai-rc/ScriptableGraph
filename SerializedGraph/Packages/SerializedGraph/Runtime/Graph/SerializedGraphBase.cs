using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GiftHorse.SerializedGraphs
{
    /// <summary>
    /// Serialized Graph components base class.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class SerializedGraphBase : MonoBehaviour
    {
        private const string k_SceneNotLoaded = "[SerializedGraph] The graph: {0} cannot be interacted with when its parent scene: {1} is not loaded!";
        private const string k_PortsAlreadyConnected = "[SerializedGraph] Trying to connect two ports that are already connected! Graph name: {0}, From Node Id: {1} To Node Id: {2}";
        private const string k_PortsNotConnected = "[SerializedGraph] Trying to disconnect two ports that are not connected! Graph name: {0}, From Node Id: {1} To Node Id: {2}";
        private const string k_PortsTypeMismatch = "[SerializedGraph] Trying to connect two ports that are not of the same type! Graph name: {0}, From Node Id: {1} To Node Id: {2}";
        private const string k_PortsOfTheSameNode = "[SerializedGraph] Trying to connect two ports that belong to the same node! Graph name: {0}, From Port Index: {1} To Port Index: {2}";
        private const string k_NodeNotFound = "[SerializedGraph] No node was found for Id: {0}! Graph name: {1}.";
        private const string k_ConnectionNotFound = "[SerializedGraph] No connection was found for Id: {0}! Graph name: {1}.";
        private const string k_NodeCastFailed = "[SerializedGraph] Cast failed! The node with Id: {0} is not of type: {1}! Graph name: {2}.";

        [SerializeReference] private List<ISerializedNode> m_Nodes;
        [SerializeReference] private List<Connection> m_Connections;

        private Dictionary<string, ISerializedNode> m_NodesById;
        private Dictionary<string, Connection> m_ConnectionsById;
        private readonly HashSet<string> m_VisitedNodes = new();

        /// <summary>
        /// The Assembly Qualified Name of the node's specialized base type that implements from <see cref="ISerializedNode"/>.
        /// </summary>
        public abstract string NodesBaseType { get; }

        /// <summary>
        /// Collection of all <see cref="ISerializedNode"/>s sorted in the order of execution.
        /// </summary>
        public IEnumerable<ISerializedNode> Nodes => m_Nodes;

        /// <summary>
        /// Collection of all <see cref="Connection"/>s.
        /// </summary>
        public IEnumerable<Connection> Connections => m_Connections;
        
        private bool IsSceneLoaded => gameObject.scene.isLoaded;
        
        private Dictionary<string, ISerializedNode> NodesById
        {
            get
            {
                if (m_NodesById is null)
                    m_NodesById = m_Nodes.ToDictionary(n => n.Id, n => n);
                
                return m_NodesById;
            }
        }
        
        private Dictionary<string, Connection> ConnectionsById
        {
            get
            {
                if (m_ConnectionsById is null)
                    m_ConnectionsById = m_Connections.ToDictionary(c => c.Id, c => c);
                
                return m_ConnectionsById;
            }
        }

        protected SerializedGraphBase()
        {
            m_Nodes = new List<ISerializedNode>();
            m_Connections = new List<Connection>();
        }

        private void Start()
        {
            foreach (var node in Nodes)
                node.Init(this);

            foreach (var connection in m_Connections)
                connection.Init(NodesById);

            OnStart();
        }

        private void OnDestroy()
        {
            foreach (var node in Nodes)
                node.Dispose();
        }

        /// <summary>
        /// Callback called when a <see cref="Connection"/> is formed.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the <see cref="Connection"/> goes to. </param>
        protected abstract void OnConnectionCreated(ISerializedNode fromNode, OutPort fromPort, ISerializedNode toNode, InPort toPort);

        /// <summary>
        /// Callback called when a <see cref="Connection"/> is removed.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the <see cref="Connection"/> goes to. </param>
        protected abstract void OnConnectionRemoved(ISerializedNode fromNode, OutPort fromPort, ISerializedNode toNode, InPort toPort);

        /// <summary>
        /// Callback called on the Start event of the <see cref="GameObject"/> the graph is attached to.
        /// </summary>
        protected virtual void OnStart() { }
        
        /// <summary>
        /// Executes all <see cref="ISerializedNode"/>s processes.
        /// </summary>
        public void Process()
        {
            foreach (var node in Nodes)
                node.Process();
        }

        /// <summary>
        /// Adds a <see cref="ISerializedNode"/> to the graph data structure.
        /// </summary>
        /// <param name="node"> The <see cref="ISerializedNode"/> to be added. </param>
        public void AddNode(ISerializedNode node)
        {
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return;
            }
            
            m_Nodes.Add(node);
            NodesById[node.Id] = node;
        }

        /// <summary>
        /// Removes the <see cref="ISerializedNode"/> from the graph data structure.
        /// </summary>
        /// <param name="node"> The <see cref="ISerializedNode"/> to be removed. </param>
        public void RemoveNode(ISerializedNode node)
        {
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return;
            }
            
            m_Nodes.Remove(node);
            NodesById.Remove(node.Id);
        }

        /// <summary>
        /// Connects two <see cref="ISerializedNode"/>s at the specified port indices and stores the <see cref="Connection"/> in the graph data structure.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPortIndex"> The index of the port the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPortIndex"> The index of the port the <see cref="Connection"/> goes to. </param>
        public void ConnectNodes(ISerializedNode fromNode, int fromPortIndex, ISerializedNode toNode, int toPortIndex)
        {
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return;
            }
            
            if (!fromNode.TryGetOutPortByIndex(fromPortIndex, out var fromPort)) 
                return;
            
            if (!toNode.TryGetInPortByIndex(toPortIndex, out var toPort)) 
                return;
            
            if (!TryConnectPorts(fromPort, toPort, out var connection)) 
                return;
            
            UpdateDependencyLevels(toNode);
            SortNodesByDepthLevel();
            OnConnectionCreated(fromNode, fromPort, toNode, toPort);
        }
        
        /// <summary>
        /// Disconnects two <see cref="ISerializedNode"/>s at the specified port indices and removes the <see cref="Connection"/> from the graph data structure.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPortIndex"> The index of the port the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPortIndex"> The index of the port the <see cref="Connection"/> goes to. </param>
        public void DisconnectNodes(ISerializedNode fromNode, int fromPortIndex, ISerializedNode toNode, int toPortIndex)
        {
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return;
            }
            
            if (!fromNode.TryGetOutPortByIndex(fromPortIndex, out var fromPort)) 
                return;
            
            if (!toNode.TryGetInPortByIndex(toPortIndex, out var toPort)) 
                return;
            
            if (!TryDisconnectPorts(fromPort, toPort))
                return;
            
            UpdateDependencyLevels(toNode);
            SortNodesByDepthLevel();
            OnConnectionRemoved(fromNode, fromPort, toNode, toPort);
        }

        /// <summary>
        /// Updates <see cref="ISerializedNode"/>s and <see cref="Connection"/>s mappings on editor undo.
        /// </summary>
        public void UpdateMappings()
        {
            m_NodesById = m_Nodes.ToDictionary(n => n.Id, n => n);
            m_ConnectionsById = m_Connections.ToDictionary(c => c.Id, c => c);
        }

        /// <summary>
        /// Tries to get a <see cref="ISerializedNode"/> by its id.
        /// </summary>
        /// <param name="nodeId"> The id of the node. </param>
        /// <param name="node"> The reference to the corresponding node. Is null if the id was not found. </param>
        /// <typeparam name="T"> The subtype the node is expected te be received as. </typeparam>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        public bool TryGetNodeById<T>(string nodeId, out T node) where T : class, ISerializedNode
        {
            node = null;
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return false;
            }
            
            if (NodesById.TryGetValue(nodeId, out var nodeBase))
            {
                if (nodeBase is not T castedNode)
                {
                    Debug.LogErrorFormat(k_NodeCastFailed, nodeId, typeof(T).Name, name);
                    return false;
                }
                
                node = castedNode;
                return true;
            }
            
            Debug.LogErrorFormat(k_NodeNotFound, nodeId, name);
            return false;
        }

        /// <summary>
        /// Tries to get the origin <see cref="ISerializedNode"/> of a <see cref="Connection"/>.
        /// </summary>
        /// <param name="connectionId"> The id of the <see cref="Connection"/>. </param>
        /// <param name="node"> The reference to the origin node. Is null if the node was not found. </param>
        /// <typeparam name="T"> The subtype the node is expected te be received as. </typeparam>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        public bool TryGetInputNode<T>(string connectionId, out T node) where T : class, ISerializedNode
        {
            node = null;
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return false;
            }
            
            if (!TryGetConnectionById(connectionId, out var connection))
                return false;
            
            if (!TryGetNodeById(connection.FromPort.NodeId, out node))
                return false;
            
            return true;
        }

        /// <summary>
        /// Tries to get the destination <see cref="ISerializedNode"/> of a <see cref="Connection"/>.
        /// </summary>
        /// <param name="connectionId"> The id of the <see cref="Connection"/>. </param>
        /// <param name="node"> The reference to the destination node. Is null if the node was not found. </param>
        /// <typeparam name="T"> The subtype the node is expected te be received as. </typeparam>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        public bool TryGetOutputNode<T>(string connectionId, out T node) where T : class, ISerializedNode
        {
            node = null;
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return false;
            }
            
            if (!TryGetConnectionById(connectionId, out var connection))
                return false;
            
            if (!TryGetNodeById(connection.ToPort.NodeId, out node))
                return false;
            
            return true;
        }

        /// <summary>
        /// Tries to get a <see cref="Connection"/> by its id.
        /// </summary>
        /// <param name="connectionId"> The id of the <see cref="Connection"/>. </param>
        /// <param name="connection"> The reference to the corresponding <see cref="Connection"/>. Is null if the id was not found. </param>
        /// <returns> Returns true if the <see cref="Connection"/> was found, otherwise returns false. </returns>
        public bool TryGetConnectionById(string connectionId, out Connection connection)
        {
            connection = null;
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return false;
            }
            
            if (ConnectionsById.TryGetValue(connectionId, out connection))
                return true;
            
            Debug.LogErrorFormat(k_ConnectionNotFound, connectionId, name);
            return false;
        }

        private bool TryConnectPorts(OutPort from, InPort to, out Connection connection)
        {
            if (from.ConnectionIds.Contains(to.ConnectionId))
            {
                Debug.LogErrorFormat(k_PortsAlreadyConnected, name, from.NodeId, to.NodeId);
                connection = null;
                
                return false;
            }

            if (!from.CompatibleType.Equals(to.CompatibleType))
            {
                Debug.LogErrorFormat(k_PortsTypeMismatch, name, from.NodeId, to.NodeId);
                connection = null;
                
                return false;
            }

            if (from.NodeId.Equals(to.NodeId))
            {
                Debug.LogErrorFormat(k_PortsOfTheSameNode, name, from.Index, to.Index);
                connection = null;
                
                return false;
            }

            connection = new Connection(from, to);
            to.ConnectionId = connection.Id;
            from.ConnectionIds.Add(connection.Id);

            m_Connections.Add(connection);
            ConnectionsById[connection.Id] = connection;

            return true;
        }

        private bool TryDisconnectPorts(OutPort from, InPort to)
        {
            if (!from.ConnectionIds.Contains(to.ConnectionId))
            {
                Debug.LogErrorFormat(k_PortsNotConnected, name, from.NodeId, to.NodeId);
                return false;
            }

            if (!TryGetConnectionById(to.ConnectionId, out var connection))
                return false;

            from.ConnectionIds.Remove(connection.Id);
            to.ConnectionId = null;

            m_Connections.Remove(connection);
            ConnectionsById.Remove(connection.Id);

            return true;
        }
        
        private void UpdateDependencyLevels(ISerializedNode node)
        {
            if (!m_VisitedNodes.Any())
                m_VisitedNodes.Clear();

            UpdateDependencyLevelsRecursively(node);
            m_VisitedNodes.Clear();
        }

        private void UpdateDependencyLevelsRecursively(ISerializedNode node)
        {
            if (m_VisitedNodes.Contains(node.Id))
                return;

            // By traversing the subgraph of the inputs without accounting for the nodes that are connected in a circle
            // will result in the dependency level of those nodes to be evaluated as the inputs of the first visited node
            // of the circle, which can lead to some unexpected behavior.
            
            int? maxDependencyLevel = null;
            foreach (var inPort in node.InPorts)
            {
                if (inPort.IsEmpty)
                    continue;
                
                if (!TryGetInputNode(inPort.ConnectionId, out ISerializedNode inNode))
                    continue;

                if (maxDependencyLevel is null || inNode.DepthLevel > maxDependencyLevel.Value)
                    maxDependencyLevel = inNode.DepthLevel;
            }

            node.DepthLevel = maxDependencyLevel is not null 
                ? maxDependencyLevel.Value + 1
                : 0;

            m_VisitedNodes.Add(node.Id);
            foreach (var outPort in node.OutPorts)
            {
                foreach (var connectionId in outPort.ConnectionIds)
                {
                    if (!TryGetOutputNode(connectionId, out ISerializedNode outNode))
                        continue;

                    UpdateDependencyLevelsRecursively(outNode);
                }
            }
        }

        private void SortNodesByDepthLevel()
        {
            m_Nodes.Sort((left, right) =>
            {
                if (left.DepthLevel < right.DepthLevel) return -1;
                if (left.DepthLevel > right.DepthLevel) return  1;

                return 0;
            });
        }
    }
}