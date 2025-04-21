using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs
{
    /// <summary>
    /// Scriptable Nodes base class.
    /// </summary>
    public class ScriptableNode : ScriptableObject, IDisposable
    {
        private const string k_NodeNotInitialized = "[ScriptableGraph] Trying to process a node that was not initialized! Graph name: {0}, Node id: {1}";
        private const string k_OutOfBoundsInPortIndex = "[ScriptableGraph] Trying to get an In port by an out of bounds index! Graph name: {0}, Node id: {1}, In Port index: {2}";
        private const string k_OutOfBoundsOutPortIndex = "[ScriptableGraph] Trying to get an Out port by an out of bounds index! Graph name: {0}, Node id: {1}, Out Port index: {2}";
        private const string k_InPortNameNotFound = "[ScriptableGraph] Trying to find an In port by a name that was not registered! Graph name: {0}, Node id: {1}, In Port name: {2}";
        private const string k_OutPortNameNotFound = "[ScriptableGraph] Trying to get an Out port by a name that was not registered! Graph name: {0}, Node id: {1}, Out Port name: {2}";

        [SerializeField] private string m_Id;
        [SerializeField] private Rect m_Position;
        [SerializeField] private int m_DepthLevel;
        [SerializeField] private bool m_Expanded;

        [SerializeReference] private List<InPort> m_InPorts;
        [SerializeReference] private List<OutPort> m_OutPorts;

        private bool m_Initialized;
        private bool m_Disposed;

        /// <summary>
        /// Reference to the <see cref="ScriptableGraph"/> that owns this <see cref="ScriptableNode"/>.
        /// </summary>
        public ScriptableGraph Graph { get; private set; }

        /// <summary>
        /// The id of this node.
        /// </summary>
        public string Id => m_Id;

        /// <summary>
        /// Number of nodes in the longest input chain this node is part of. It is used by
        /// the sorting algorithm to figure out in which order the nodes should be evaluated.
        /// </summary>
        public int DepthLevel
        {
            get => m_DepthLevel;
            set => m_DepthLevel = value;
        }

        /// <summary>
        /// <see cref="Rect"/> used by Unity Editor to manage node's position in the Graph View.
        /// </summary>
        public Rect Position
        {
            get => m_Position;
            set => m_Position = value;
        }

        /// <summary>
        /// Flag used by Unity Editor to toggle between Expanded/Collapse states in the Graph View.
        /// </summary>
        public bool Expanded
        {
            get => m_Expanded;
            set => m_Expanded = value;
        }

        /// <summary>
        /// Collection of all <see cref="InPort"/>s of the node.
        /// </summary>
        public IEnumerable<InPort> InPorts => m_InPorts;

        /// <summary>
        /// Collection of all <see cref="OutPort"/>s of the node.
        /// </summary>
        public IEnumerable<OutPort> OutPorts => m_OutPorts;

        /// <summary>
        /// <see cref="ScriptableNode"/>'s Constructor.
        /// It is called when a new node is created in Edit Mode.
        /// </summary>
        protected ScriptableNode()
        {
            m_Id = Guid.NewGuid().ToString();
            m_Expanded = true;

            ReflectionHelper.GetNodePorts(this, out m_InPorts, out m_OutPorts);
        }

        /// <summary>
        /// Finalizer to ensure unmanaged resources are released if Dispose is not called manually.
        /// </summary>
        ~ScriptableNode() => OnDispose();

        /// <summary>
        /// Node's initialization implementation.
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// Node's process implementation.
        /// </summary>
        protected virtual void OnProcess() { }

        /// <summary>
        /// Node's disposal implementation. It is called when the graph is disposed.
        /// </summary>
        protected virtual void OnDispose() { }

        /// <summary>
        /// Initializes the node and its ports.
        /// </summary>
        /// <param name="graph"> The <see cref="ScriptableGraph"/> the owns this node. </param>
        public void Init(ScriptableGraph graph)
        {
            if (m_Initialized) return;

            Graph = graph;
            m_Initialized = true;

            OnInit();
        }

        /// <summary>
        /// Executes the node process.
        /// </summary>
        public void Process()
        {
            if (!m_Initialized)
            {
                Debug.LogErrorFormat(k_NodeNotInitialized, Graph.name, m_Id);
                return;
            }

            foreach (var inPort in InPorts)
            {
                if (inPort.IsEmpty)
                    continue;

                if (Graph.TryGetConnectionById(inPort.ConnectionId, out var connection))
                    connection.TransferValue();
            }

            OnProcess();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ScriptableNode"/>.
        /// </summary>
        public void Dispose()
        {
            if (!m_Disposed) return;

            OnDispose();
            GC.SuppressFinalize(this);

            m_Disposed = true;
        }

        /// <summary>
        /// Tries to get the <see cref="InPort"/> with the provided name.
        /// </summary>
        /// <param name="inputName"> Name of the <see cref="InPort"/> to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Whether the <see cref="InPort"/> was found or not. </returns>
        public bool TryGetInPort(string inputName, out InPort port)
        {
            foreach (var inPort in m_InPorts)
            {
                if (inPort.Name.Equals(inputName))
                {
                    port = inPort;
                    return true;
                }
            }

            port = null;
            Debug.LogErrorFormat(k_InPortNameNotFound, Graph.name, m_Id, inputName);

            return false;
        }

        /// <summary>
        /// Tries to get the <see cref="OutPort"/> with the provided name.
        /// </summary>
        /// <param name="outputName"> Name of the <see cref="OutPort"/> to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Whether the <see cref="OutPort"/> was found or not. </returns>
        public bool TryGetOutPort(string outputName, out OutPort port)
        {
            foreach (var outPort in m_OutPorts)
            {
                if (outPort.Name.Equals(outputName))
                {
                    port = outPort;
                    return true;
                }
            }

            port = null;
            Debug.LogErrorFormat(k_OutPortNameNotFound, Graph.name, m_Id, outputName);

            return false;
        }

        /// <summary>
        /// Tries to get the <see cref="InPort"/> at the provided index.
        /// </summary>
        /// <param name="index"> Index of the <see cref="InPort"/> to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Whether the <see cref="InPort"/> was found or not. </returns>
        public bool TryGetInPort(int index, out InPort port)
        {
            if (index < m_InPorts.Count && m_InPorts.Count > -1)
            {
                port = m_InPorts[index];
                return true;
            }

            port = null;
            Debug.LogErrorFormat(k_OutOfBoundsInPortIndex, Graph.name, m_Id, index);

            return false;
        }

        /// <summary>
        /// Tries to get the <see cref="OutPort"/> at the provided index.
        /// </summary>
        /// <param name="index"> Index of the <see cref="OutPort"/> to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Whether the <see cref="OutPort"/> was found or not. </returns>
        public bool TryGetOutPort(int index, out OutPort port)
        {
            if (index < m_OutPorts.Count && m_OutPorts.Count > -1)
            {
                port = m_OutPorts[index];
                return true;
            }

            port = null;
            Debug.LogErrorFormat(k_OutOfBoundsOutPortIndex, Graph.name, m_Id, index);

            return false;
        }

        /// <summary>
        /// Tries to get the node connected to the <see cref="InPort"/> with the provided name.
        /// </summary>
        /// <param name="portName">
        /// Name of the <see cref="InPort"/> whose connected node is being fetched.
        /// </param>
        /// <param name="node">
        /// Reference to the node connected to the specified <see cref="InPort"/>.
        /// Is null if the port name is invalid or the port is not connected.
        /// </param>
        /// <returns>
        /// Whether the node was found or not.
        /// </returns>
        public bool TryGetInputNodeOf(string portName, out ScriptableNode node)
        {
            if (TryGetInPort(portName, out var inPort)) 
                return Graph.TryGetInputNode(inPort.ConnectionId, out node);

            node = null;
            return false;
        }

        /// <summary>
        /// Tries to get the nodes connected to the <see cref="OutPort"/> with the provided name.
        /// </summary>
        /// <param name="portName">
        /// Name of the <see cref="OutPort"/> whose connected nodes are being fetched.
        /// </param>
        /// <param name="nodes">
        /// List to be populated with nodes connected to the specified <see cref="OutPort"/>.
        /// </param>
        /// <returns>
        /// Whether the nodes were found or not.
        /// </returns>
        public bool TryGetOutputNodesOf(string portName, List<ScriptableNode> nodes)
        {
            if (!TryGetOutPort(portName, out var outPort))
                return false;

            foreach (var connectionId in outPort.ConnectionIds)
            {
                if (!Graph.TryGetOutputNode(connectionId, out ScriptableNode node))
                    continue;

                nodes.Add(node);
            }

            return nodes.Any();
        }
    }
}