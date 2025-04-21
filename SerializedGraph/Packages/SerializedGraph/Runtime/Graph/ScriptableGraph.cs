using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs
{
    /// <summary>
    /// Scriptable Graph base class.
    /// </summary>
    public abstract class ScriptableGraph : ScriptableObject, IDisposable
    {
        private const string k_PortsAlreadyConnected = "[ScriptableGraph] Trying to connect two ports that are already connected! Graph name: {0}, From Node Id: {1} To Node Id: {2}";
        private const string k_PortsNotConnected = "[ScriptableGraph] Trying to disconnect two ports that are not connected! Graph name: {0}, From Node Id: {1} To Node Id: {2}";
        private const string k_PortsTypeMismatch = "[ScriptableGraph] Trying to connect two ports that are not of the same type! Graph name: {0}, From Node Id: {1} To Node Id: {2}";
        private const string k_PortsOfTheSameNode = "[ScriptableGraph] Trying to connect two ports that belong to the same node! Graph name: {0}, From Port Index: {1} To Port Index: {2}";
        private const string k_NodeNotFound = "[ScriptableGraph] No node was found for Id: {0}! Graph name: {1}.";
        private const string k_ConnectionNotFound = "[ScriptableGraph] No connection was found for Id: {0}! Graph name: {1}.";
        private const string k_NodeCastFailed = "[ScriptableGraph] Cast failed! The node with Id: {0} is not of type: {1}! Graph name: {2}.";

        [SerializeField] private List<ScriptableNode> m_Nodes;
        [SerializeField] private List<Connection> m_Connections;

        private readonly HashSet<string> m_VisitedNodes = new();
        private Dictionary<string, ScriptableNode> m_NodesById;
        private Dictionary<string, Connection> m_ConnectionsById;

        /// <summary>
        /// The name of the graph.
        /// </summary>
        // public string Name { get; }

        /// <summary>
        /// The Assembly Qualified Name of the node's specialized base type that implements from <see cref="ScriptableNode"/>.
        /// </summary>
        public abstract string NodesBaseType { get; }

        /// <summary>
        /// Collection of all <see cref="ScriptableNode"/>s sorted in the order of execution.
        /// </summary>
        public IEnumerable<ScriptableNode> Nodes => m_Nodes;

        /// <summary>
        /// Collection of all <see cref="Connection"/>s.
        /// </summary>
        public IEnumerable<Connection> Connections => m_Connections;

        private Dictionary<string, ScriptableNode> NodesById
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

        protected ScriptableGraph()
        {
            m_Nodes = new List<ScriptableNode>();
            m_Connections = new List<Connection>();
        }

        /// <summary>
        /// Callback called when a <see cref="Connection"/> is formed.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the <see cref="Connection"/> goes to. </param>
        protected abstract void OnConnectionCreated(ScriptableNode fromNode, OutPort fromPort, ScriptableNode toNode, InPort toPort);

        /// <summary>
        /// Callback called when a <see cref="Connection"/> is removed.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the <see cref="Connection"/> goes to. </param>
        protected abstract void OnConnectionRemoved(ScriptableNode fromNode, OutPort fromPort, ScriptableNode toNode, InPort toPort);

        /// <summary>
        /// Callback called when the graph is initialized.
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// Initializes the graph and all <see cref="ScriptableNode"/>s in it.
        /// </summary>
        public void Init() 
        {
            foreach (var node in Nodes)
                node.Init(this);

            foreach (var connection in m_Connections)
                connection.Init(NodesById);

            SortNodes();
            OnInit();
        }

        /// <summary>
        /// Sorts the <see cref="ScriptableNode"/>s in the graph by their depth level.
        /// </summary>
        public void SortNodes()
        {
            m_Nodes.Sort((left, right) =>
            {
                if (left.DepthLevel < right.DepthLevel) return -1;
                if (left.DepthLevel > right.DepthLevel) return  1;

                return 0;
            });
        }

        /// <summary>
        /// Executes all <see cref="ScriptableNode"/>s processes.
        /// </summary>
        public void Process()
        {
            foreach (var node in Nodes)
                node.Process();
        }

        /// <summary>
        /// Adds a <see cref="ScriptableNode"/> to the graph data structure.
        /// </summary>
        /// <param name="node"> The <see cref="ScriptableNode"/> to be added. </param>
        public void AddNode(ScriptableNode node)
        {
            var baseNode = node;

            m_Nodes.Add(baseNode);
            NodesById[node.Id] = node;
        }

        /// <summary>
        /// Removes the <see cref="ScriptableNode"/> from the graph data structure.
        /// </summary>
        /// <param name="node"> The <see cref="ScriptableNode"/> to be removed. </param>
        public void RemoveNode(ScriptableNode node)
        {
            m_Nodes.Remove(node as ScriptableNode);
            NodesById.Remove(node.Id);
        }

        /// <summary>
        /// Connects two <see cref="ScriptableNode"/>s at the specified port indices and stores the <see cref="Connection"/> in the graph data structure.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPortIndex"> The index of the port the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPortIndex"> The index of the port the <see cref="Connection"/> goes to. </param>
        public void ConnectNodes(ScriptableNode fromNode, int fromPortIndex, ScriptableNode toNode, int toPortIndex)
        {
            if (!fromNode.TryGetOutPort(fromPortIndex, out var fromPort)) 
                return;

            if (!toNode.TryGetInPort(toPortIndex, out var toPort)) 
                return;

            if (!TryConnectPorts(fromPort, toPort, out var connection)) 
                return;

            UpdateDepthLevels(toNode);
            OnConnectionCreated(fromNode, fromPort, toNode, toPort);
        }

        /// <summary>
        /// Disconnects two <see cref="ScriptableNode"/>s at the specified port indices and removes the <see cref="Connection"/> from the graph data structure.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPortIndex"> The index of the port the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPortIndex"> The index of the port the <see cref="Connection"/> goes to. </param>
        public void DisconnectNodes(ScriptableNode fromNode, int fromPortIndex, ScriptableNode toNode, int toPortIndex)
        {
            if (!fromNode.TryGetOutPort(fromPortIndex, out var fromPort)) 
                return;

            if (!toNode.TryGetInPort(toPortIndex, out var toPort)) 
                return;

            if (!TryDisconnectPorts(fromPort, toPort))
                return;

            UpdateDepthLevels(toNode);
            OnConnectionRemoved(fromNode, fromPort, toNode, toPort);
        }

        /// <summary>
        /// Updates <see cref="ScriptableNode"/>s and <see cref="Connection"/>s mappings on editor undo.
        /// </summary>
        public void UpdateMappings()
        {
            m_NodesById = m_Nodes.ToDictionary(n => n.Id, n => n);
            m_ConnectionsById = m_Connections.ToDictionary(c => c.Id, c => c);
        }

        /// <summary>
        /// Tries to get a <see cref="ScriptableNode"/> by its id.
        /// </summary>
        /// <param name="nodeId"> The id of the node. </param>
        /// <param name="node"> The reference to the corresponding node. Is null if the id was not found. </param>
        /// <typeparam name="T"> The subtype the node is expected to be received as. </typeparam>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        public bool TryGetNodeById<T>(string nodeId, out T node)
        {
            node = default;
            if (NodesById.TryGetValue(nodeId, out var scriptableNode))
            {
                if (scriptableNode is not T castedNode)
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
        /// Tries to get the origin <see cref="ScriptableNode"/> of a <see cref="Connection"/>.
        /// </summary>
        /// <param name="connectionId"> The id of the <see cref="Connection"/>. </param>
        /// <param name="node"> The reference to the origin node. Is null if the node was not found. </param>
        /// <typeparam name="T"> The subtype the node is expected to be received as. </typeparam>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        public bool TryGetInputNode<T>(string connectionId, out T node)
        {
            node = default;
            if (!TryGetConnectionById(connectionId, out var connection))
                return false;

            if (!TryGetNodeById(connection.FromPort.NodeId, out node))
                return false;

            return true;
        }

        /// <summary>
        /// Tries to get the destination <see cref="ScriptableNode"/> of a <see cref="Connection"/>.
        /// </summary>
        /// <param name="connectionId"> The id of the <see cref="Connection"/>. </param>
        /// <param name="node"> The reference to the destination node. Is null if the node was not found. </param>
        /// <typeparam name="T"> The subtype the node is expected to be received as. </typeparam>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        public bool TryGetOutputNode<T>(string connectionId, out T node)
        {
            node = default;
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

            if (ConnectionsById.TryGetValue(connectionId, out connection))
                return true;

            Debug.LogErrorFormat(k_ConnectionNotFound, connectionId, name);
            return false;
        }

        /// <summary>
        /// Disposes the graph and all its nodes.
        /// </summary>
        public void Dispose()
        {
            foreach (var node in Nodes)
                node.Dispose();
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

        private void UpdateDepthLevels(ScriptableNode node)
        {
            if (!m_VisitedNodes.Any())
                m_VisitedNodes.Clear();

            UpdateDepthLevelsRecursively(node);
            m_VisitedNodes.Clear();
        }

        private void UpdateDepthLevelsRecursively(ScriptableNode node)
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

                if (!TryGetInputNode(inPort.ConnectionId, out ScriptableNode inNode))
                    continue;

                if (maxDependencyLevel is null || inNode.DepthLevel > maxDependencyLevel.Value)
                    maxDependencyLevel = inNode.DepthLevel;
            }

            node.DepthLevel = maxDependencyLevel is not null 
                ? maxDependencyLevel.Value + 1
                : 0;

            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssetIfDirty(node);
            #endif

            m_VisitedNodes.Add(node.Id);
            foreach (var outPort in node.OutPorts)
            {
                foreach (var connectionId in outPort.ConnectionIds)
                {
                    if (!TryGetOutputNode(connectionId, out ScriptableNode outNode))
                        continue;

                    UpdateDepthLevelsRecursively(outNode);
                }
            }
        }
    }
}