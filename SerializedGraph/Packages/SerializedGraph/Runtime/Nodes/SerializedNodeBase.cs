using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace GiftHorse.SerializedGraphs
{
    /// <summary>
    /// Serialized Nodes base class.
    /// </summary>
    [Serializable]
    public class SerializedNodeBase : ISerializedNode
    {
        private const string k_NodeNotInitialized = "[SerializedGraph] Trying to process a node that was not initialized! Graph name: {0}, Node id: {1}";
        private const string k_OutOfBoundsInPortIndex = "[SerializedGraph] Trying to get an In port by an out of bounds index! Graph name: {0}, Node id: {1}, In Port index: {2}";
        private const string k_OutOfBoundsOutPortIndex = "[SerializedGraph] Trying to get an Out port by an out of bounds index! Graph name: {0}, Node id: {1}, Out Port index: {2}";
        private const string k_InPortNameNotFound = "[SerializedGraph] Trying to find an In port by a name that was not registered! Graph name: {0}, Node id: {1}, In Port name: {2}";
        private const string k_OutPortNameNotFound = "[SerializedGraph] Trying to get an Out port by a name that was not registered! Graph name: {0}, Node id: {1}, Out Port name: {2}";

        [SerializeField] private string m_Id;
        [SerializeField] private Rect m_Position;
        [SerializeField] private int m_DepthLevel;
        [SerializeField] private bool m_Expanded;

        [SerializeReference] private List<InPort> m_InPorts;
        [SerializeReference] private List<OutPort> m_OutPorts;

        private string m_Title;
        private bool m_Initialized;
        private bool m_Disposed;

        /// <inheritdoc />
        public ISerializedGraph Graph { get; private set; }

        /// <inheritdoc />
        public string Id => m_Id;

        /// <inheritdoc />
        public string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(m_Title))
                    return m_Title;

                m_Title = ReflectionHelper.GetNodeTitleByType(GetType());
                return m_Title;
            }
        }

        /// <inheritdoc />
        public int DepthLevel
        {
            get => m_DepthLevel;
            set => m_DepthLevel = value;
        }

        /// <inheritdoc />
        public Rect Position
        {
            get => m_Position;
            set => m_Position = value;
        }

        /// <inheritdoc />
        public bool Expanded
        {
            get => m_Expanded;
            set => m_Expanded = value;
        }

        /// <inheritdoc />
        public IEnumerable<InPort> InPorts => m_InPorts;

        /// <inheritdoc />
        public IEnumerable<OutPort> OutPorts => m_OutPorts;

        /// <summary>
        /// <see cref="SerializedNodeBase"/>'s Constructor.
        /// It is called when a new node is created in Edit Mode.
        /// </summary>
        protected SerializedNodeBase()
        {
            m_Id = Guid.NewGuid().ToString();
            m_Expanded = true;

            m_InPorts = ListPool<InPort>.Get();
            m_OutPorts = ListPool<OutPort>.Get();
            ReflectionHelper.GetNodePorts(this, out m_InPorts, out m_OutPorts);
        }

        /// <summary>
        /// Finalizer to ensure unmanaged resources are released if Dispose is not called manually.
        /// </summary>
        ~SerializedNodeBase() => OnDispose();

        /// <summary>
        /// Node's initialization. It is called only once, when the graph is initialized.
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

        /// <inheritdoc />
        public void Init(ISerializedGraph graph)
        {
            if (m_Initialized) return;

            Graph = graph;
            m_Initialized = true;

            OnInit();
        }

        /// <inheritdoc />
        public void Process()
        {
            if (!m_Initialized)
            {
                Debug.LogErrorFormat(k_NodeNotInitialized, Graph.Name, m_Id);
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
        /// Releases the unmanaged resources used by the <see cref="ISerializedNode"/>.
        /// </summary>
        public void Dispose()
        {
            if (!m_Disposed) return;

            OnDispose();
            GC.SuppressFinalize(this);

            m_Disposed = true;
        }

        /// <inheritdoc />
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
            Debug.LogErrorFormat(k_InPortNameNotFound, Graph.Name, m_Id, inputName);

            return false;
        }

        /// <inheritdoc />
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
            Debug.LogErrorFormat(k_OutPortNameNotFound, Graph.Name, m_Id, outputName);

            return false;
        }

        /// <inheritdoc />
        public bool TryGetInPort(int index, out InPort port)
        {
            if (index < m_InPorts.Count && m_InPorts.Count > -1)
            {
                port = m_InPorts[index];
                return true;
            }

            port = null;
            Debug.LogErrorFormat(k_OutOfBoundsInPortIndex, Graph.Name, m_Id, index);

            return false;
        }

        /// <inheritdoc />
        public bool TryGetOutPort(int index, out OutPort port)
        {
            if (index < m_OutPorts.Count && m_OutPorts.Count > -1)
            {
                port = m_OutPorts[index];
                return true;
            }

            port = null;
            Debug.LogErrorFormat(k_OutOfBoundsOutPortIndex, Graph.Name, m_Id, index);

            return false;
        }

        /// <inheritdoc />
        public bool TryGetInputNodeOf(string portName, out ISerializedNode node)
        {
            if (TryGetInPort(portName, out var inPort)) 
                return Graph.TryGetInputNode(inPort.ConnectionId, out node);

            node = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetOutputNodesOf(string portName, List<ISerializedNode> nodes)
        {
            if (!TryGetOutPort(portName, out var outPort))
                return false;

            foreach (var connectionId in outPort.ConnectionIds)
            {
                if (!Graph.TryGetOutputNode(connectionId, out ISerializedNode node))
                    continue;

                nodes.Add(node);
            }

            return nodes.Any();
        }
    }
}