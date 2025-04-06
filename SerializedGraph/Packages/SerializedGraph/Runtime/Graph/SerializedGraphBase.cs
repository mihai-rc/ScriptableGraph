using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GiftHorse.SerializedGraphs
{
    /// <summary>
    /// Serialized Graph components base class.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class SerializedGraphBase : MonoBehaviour, ISerializedGraph
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

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public abstract string NodesBaseType { get; }

        /// <inheritdoc />
        public IEnumerable<ISerializedNode> Nodes => m_Nodes;

        /// <inheritdoc />
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

            SortNodes();
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

        /// <inheritdoc />
        public void SortNodes()
        {
            m_Nodes.Sort((left, right) =>
            {
                if (left.DepthLevel < right.DepthLevel) return -1;
                if (left.DepthLevel > right.DepthLevel) return  1;

                return 0;
            });
        }

        /// <inheritdoc />
        public void Process()
        {
            foreach (var node in Nodes)
                node.Process();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void ConnectNodes(ISerializedNode fromNode, int fromPortIndex, ISerializedNode toNode, int toPortIndex)
        {
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return;
            }

            if (!fromNode.TryGetOutPort(fromPortIndex, out var fromPort)) 
                return;

            if (!toNode.TryGetInPort(toPortIndex, out var toPort)) 
                return;

            if (!TryConnectPorts(fromPort, toPort, out var connection)) 
                return;

            UpdateDepthLevels(toNode);
            OnConnectionCreated(fromNode, fromPort, toNode, toPort);
        }

        /// <inheritdoc />
        public void DisconnectNodes(ISerializedNode fromNode, int fromPortIndex, ISerializedNode toNode, int toPortIndex)
        {
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return;
            }

            if (!fromNode.TryGetOutPort(fromPortIndex, out var fromPort)) 
                return;

            if (!toNode.TryGetInPort(toPortIndex, out var toPort)) 
                return;

            if (!TryDisconnectPorts(fromPort, toPort))
                return;

            UpdateDepthLevels(toNode);
            OnConnectionRemoved(fromNode, fromPort, toNode, toPort);
        }

        /// <inheritdoc />
        public void UpdateMappings()
        {
            m_NodesById = m_Nodes.ToDictionary(n => n.Id, n => n);
            m_ConnectionsById = m_Connections.ToDictionary(c => c.Id, c => c);
        }

        /// <inheritdoc />
        public bool TryGetNodeById<T>(string nodeId, out T node) where T : class, ISerializedNode
        {
            node = null;
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return false;
            }

            if (NodesById.TryGetValue(nodeId, out var serializedNode))
            {
                if (serializedNode is not T castedNode)
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        private void UpdateDepthLevels(ISerializedNode node)
        {
            if (!m_VisitedNodes.Any())
                m_VisitedNodes.Clear();

            UpdateDepthLevelsRecursively(node);
            m_VisitedNodes.Clear();
        }

        private void UpdateDepthLevelsRecursively(ISerializedNode node)
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

                    UpdateDepthLevelsRecursively(outNode);
                }
            }
        }
    }
}