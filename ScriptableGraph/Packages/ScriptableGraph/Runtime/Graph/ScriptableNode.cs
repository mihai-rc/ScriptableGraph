using System;
using System.Collections.Generic;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs
{
    /// <summary>
    /// ScriptableNode nodes base class.
    /// </summary>
    [Serializable]
    public class ScriptableNode : IDisposable
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

        private string m_Title;
        private bool m_Initialized;
        
        /// <summary>
        /// Reference to the <see cref="ScriptableGraph"/> that owns this node.
        /// </summary>
        protected ScriptableGraph Graph { get; private set; }

        /// <summary>
        /// The id of this node.
        /// </summary>
        public string Id => m_Id;

        /// <summary>
        /// <see cref="Rect"/> used by Unity Editor to manage node's position in the Graph View.
        /// </summary>
        public Rect Position
        {
            get => m_Position;
            set => m_Position = value;
        }

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
        /// Flag used by Unity Editor to toggle between Expanded/Collapse states in the Graph View.
        /// </summary>
        public bool Expanded
        {
            get => m_Expanded;
            set => m_Expanded = value;
        }

        /// <summary>
        /// Collection of all input ports of the node.
        /// </summary>
        public IEnumerable<InPort> InPorts => m_InPorts;

        /// <summary>
        /// Collection of all output ports of the node.
        /// </summary>
        public IEnumerable<OutPort> OutPorts => m_OutPorts;

        /// <summary>
        /// The title of the node. It is displayed on node view header.
        /// </summary>
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

        /// <summary>
        /// Initializes ports values and binds all references.
        /// </summary>
        /// <param name="graph"> The <see cref="ScriptableGraph"/> the owns this node. </param>
        public void Init(ScriptableGraph graph)
        {
            if (m_Initialized)
                return;
            
            m_Initialized = true;
            Graph = graph;

            OnInit();
        }

        /// <summary>
        /// Processes the node.
        /// </summary>
        public void Process()
        {
            if (!m_Initialized)
                Debug.LogErrorFormat(k_NodeNotInitialized, Graph.name, m_Id);
            
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
        public void Dispose() => OnDispose();

        /// <summary>
        /// Tries to get the Input port at the provided index.
        /// </summary>
        /// <param name="index"> Index of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        public bool TryGetInPortByIndex(int index, out InPort port)
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
        /// Tries to get the Output port at the provided index.
        /// </summary>
        /// <param name="index"> Index of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        public bool TryGetOutPortByIndex(int index, out OutPort port)
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
        /// Tries to find the Input port with the provided name.
        /// </summary>
        /// <param name="inputName"> Name of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        public bool TryFindInPortByName(string inputName, out InPort port)
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
        /// Tries to find the Output port with the provided name.
        /// </summary>
        /// <param name="outputName"> Name of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        public bool TryFindOutPortByName(string outputName, out OutPort port)
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
    }
}