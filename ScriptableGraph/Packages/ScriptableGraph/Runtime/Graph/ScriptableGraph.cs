using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs
{
    /// <summary>
    /// ScriptableGraph component base class.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class ScriptableGraph : MonoBehaviour
    {
        private const string k_SceneNotLoaded = "[ScriptableGraph] The graph: {0} cannot be interacted with when its parent scene: {1} is not loaded!";
        private const string k_PortsAlreadyConnected = "[ScriptableGraph] Trying to connect two ports that are already connected! Graph name: {0}, From Node Id: {1} To Node Id: {2}";
        private const string k_PortsNotConnected = "[ScriptableGraph] Trying to disconnect two ports that are not connected! Graph name: {0}, From Node Id: {1} To Node Id: {2}";
        private const string k_PortsTypeMismatch = "[ScriptableGraph] Trying to connect two ports that are not the same type! Graph name: {0}, From Node Id: {1} To Node Id: {2}";
        private const string k_PortsOfTheSameNode = "[ScriptableGraph] Trying to connect two ports that belong to the same node! Graph name: {0}, From Port Index: {1} To Port Index: {2}";
        private const string k_NodeNotFound = "[ScriptableGraph] No node was found with Id: {0}! Graph name: {1}.";
        private const string k_ConnectionNotFound = "[ScriptableGraph] No connection was found for Id: {0}! Graph name: {1}.";

        [SerializeReference] private List<ScriptableNode> m_Nodes;
        [SerializeReference] private List<Connection> m_Connections;

        private Dictionary<string, ScriptableNode> m_NodesById;
        private Dictionary<string, Connection> m_ConnectionsById;
        private readonly HashSet<string> m_VisitedNodes = new();

        /// <summary>
        /// The Assembly Qualified Name of <see cref="ScriptableNode"/>'s specialized type.
        /// </summary>
        public abstract string NodesBaseType { get; }

        /// <summary>
        /// List of all <see cref="ScriptableNode"/>s sorted in order of execution.
        /// </summary>
        public List<ScriptableNode> ScriptableNodes => m_Nodes;

        /// <summary>
        /// List of all connections.
        /// </summary>
        public List<Connection> Connections => m_Connections;
        
        /// <summary>
        /// Returns whether the scene this graph is serialized into is loaded.
        /// </summary>
        private bool IsSceneLoaded => gameObject.scene.isLoaded;

        /// <summary>
        /// Dictionary of all <see cref="ScriptableNode"/>s stored by their ids.
        /// </summary>
        private Dictionary<string, ScriptableNode> NodesById
        {
            get
            {
                if (m_NodesById is null)
                    m_NodesById = m_Nodes.ToDictionary(n => n.Id, n => n);

                return m_NodesById;
            }
        }

        /// <summary>
        /// Dictionary of all <see cref="Connection"/>s stored by their ids.
        /// </summary>
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

        private void Start()
        {
            foreach (var node in ScriptableNodes)
                node.Init(this);

            foreach (var connection in m_Connections)
                connection.Init(NodesById);

            OnStart();
        }

        /// <summary>
        /// Called when a <see cref="Connection"/> is formed.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the connection starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the connection starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the connection goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the connection goes to. </param>
        protected abstract void OnConnectionCreated(ScriptableNode fromNode, OutPort fromPort, ScriptableNode toNode, InPort toPort);

        /// <summary>
        /// Called when a <see cref="Connection"/> is removed.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the connection starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the connection starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the connection goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the connection goes to. </param>
        protected abstract void OnConnectionRemoved(ScriptableNode fromNode, OutPort fromPort, ScriptableNode toNode, InPort toPort);

        /// <summary>
        /// It is called on the Start event of the <see cref="GameObject"/> the graph is attached to.
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// Adds a <see cref="ScriptableNode"/> to the graph data structure.
        /// </summary>
        /// <param name="node"> The <see cref="ScriptableNode"/> to be added. </param>
        public void AddNode(ScriptableNode node)
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
        /// Removes the <see cref="ScriptableNode"/> from the graph data structure.
        /// </summary>
        /// <param name="node"> The <see cref="ScriptableNode"/> to be removed. </param>
        public void RemoveNode(ScriptableNode node)
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
        /// Connects two <see cref="ScriptableNode"/>s at the specified port indices and stores the <see cref="Connection"/> in the graph data structure.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the connection starts from. </param>
        /// <param name="fromPortIndex"> The index of the port the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the connection goes to. </param>
        /// <param name="toPortIndex"> The index of the port the <see cref="Connection"/> goes to. </param>
        public void ConnectNodes(ScriptableNode fromNode, int fromPortIndex, ScriptableNode toNode, int toPortIndex)
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

            UpdateDependencyLevels(toNode);
            SortNodesByDepthLevel();
            OnConnectionCreated(fromNode, fromPort, toNode, toPort);
        }

        /// <summary>
        /// Disconnects two <see cref="ScriptableNode"/>s at the specified port indices and removes the <see cref="Connection"/> from the graph data structure.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPortIndex"> The index of the port the connection starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPortIndex"> The index of the port the connection goes to. </param>
        public void DisconnectNodes(ScriptableNode fromNode, int fromPortIndex, ScriptableNode toNode, int toPortIndex)
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

            UpdateDependencyLevels(toNode);
            SortNodesByDepthLevel();
            OnConnectionRemoved(fromNode, fromPort, toNode, toPort);
        }
        
        /// <summary>
        /// Executes all nodes processes.
        /// </summary>
        public void Process()
        {
            foreach (var node in m_Nodes)
                node.Process(this);
        }

        /// <summary>
        /// Updates nodes and connections mappings on editor undo.
        /// </summary>
        public void UpdateMappings()
        {
            m_NodesById = m_Nodes.ToDictionary(n => n.Id, n => n);
            m_ConnectionsById = m_Connections.ToDictionary(c => c.Id, c => c);
        }
        
        /// <summary>
        /// Tries to get a node by its id.
        /// </summary>
        /// <param name="nodeId"> The id of the node. </param>
        /// <param name="node"> The reference to the corresponding node. Is null if the id was not found. </param>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        public bool TryGetNodeById(string nodeId, out ScriptableNode node)
        {
            node = null;
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return false;
            }

            if (NodesById.TryGetValue(nodeId, out node))
            {
                return true;
            }

            Debug.LogErrorFormat(k_NodeNotFound, nodeId, name);
            return false;
        }
        
        /// <summary>
        /// Tries to get a connection by its id.
        /// </summary>
        /// <param name="connectionId"> The id of the connection. </param>
        /// <param name="connection"> The reference to the corresponding connection. Is null if the id was not found. </param>
        /// <returns> Returns true if the connection was found, otherwise returns false. </returns>
        public bool TryGetConnectionById(string connectionId, out Connection connection)
        {
            connection = null;
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return false;
            }

            if (ConnectionsById.TryGetValue(connectionId, out connection))
            {
                return true;
            }
            
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
            {
                return false;
            }

            from.ConnectionIds.Remove(connection.Id);
            to.ConnectionId = null;

            m_Connections.Remove(connection);
            ConnectionsById.Remove(connection.Id);

            return true;
        }
        
        private void UpdateDependencyLevels(ScriptableNode node)
        {
            if (!m_VisitedNodes.Any())
                m_VisitedNodes.Clear();

            UpdateDependencyLevelsRecursively(node);
            m_VisitedNodes.Clear();
        }

        private void UpdateDependencyLevelsRecursively(ScriptableNode node)
        {
            if (m_VisitedNodes.Contains(node.Id))
                return;

            // By traversing the subgraph of the inputs without accounting for the nodes that are connected in a circle
            // will result in the dependency level of those nodes to be evaluated as the inputs of the first visited node
            // of the circle, which can lead to some unexpected behavior.
            
            int? maxDependencyLevel = null;
            foreach (var inPort in node.InPorts)
            {
                if (inPort.ConnectionId is null)
                    continue;

                if (!TryGetConnectionById(inPort.ConnectionId, out var connection))
                    continue;

                if (!TryGetNodeById(connection.FromPort.NodeId, out var inNode))
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
                    if (!TryGetConnectionById(connectionId, out var connection))
                    {
                        continue;
                    }

                    if (!TryGetNodeById(connection.ToPort.NodeId, out var outNode))
                        continue;

                    UpdateDependencyLevelsRecursively(outNode);
                }
            }
        }

        private void SortNodesByDepthLevel()
        {
            ScriptableNodes.Sort((left, right) =>
            {
                if (left.DepthLevel < right.DepthLevel) 
                    return -1;
                
                if (left.DepthLevel > right.DepthLevel) 
                    return  1;
                
                return 0;
            });
        }
    }
}