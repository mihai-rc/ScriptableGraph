using System;
using System.Collections.Generic;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs
{
    /// <summary>
    /// ScriptableNode nodes base class.
    /// </summary>
    [Serializable]
    public abstract class ScriptableNode
    {
        private const string k_OutOfBoundsInPortIndex = "[ScriptableGraph] Trying to get an In port by an out of bounds index! Graph owner: {0}, Node id: {1}, In Port index: {2}";
        private const string k_OutOfBoundsOutPortIndex = "[ScriptableGraph] Trying to get an Out port by an out of bounds index! Graph owner: {0}, Node id: {1}, Out Port index: {2}";

        [SerializeField] private string m_Id;
        [SerializeField] private Rect m_Position;
        [SerializeField] private bool m_Expanded;

        [SerializeReference] private List<InPort> m_InPorts;
        [SerializeReference] private List<OutPort> m_OutPorts;

        private string m_Title;
        private string m_GraphName;
        private bool m_Initialized;

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
        protected virtual void OnCreate() { }

        /// <summary>
        /// Node's process implementation.
        /// </summary>
        protected abstract void OnProcess();

        /// <summary>
        /// Initializes ports values and binds all references.
        /// </summary>
        /// <param name="graphName"> The name of the <see cref="GameObject"/> the graph is attached to. </param>
        public void Init(string graphName)
        {
            if (m_Initialized)
                return;

            m_GraphName = graphName;
            m_Initialized = true;

            OnCreate();
        }

        /// <summary>
        /// Tries to get the Input port by the provided index.
        /// </summary>
        /// <param name="index"> Index of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        public bool TryGetInPort(int index, out InPort port)
        {
            if (index < m_InPorts.Count && m_InPorts.Count > -1)
            {
                port = m_InPorts[index];
                return true;
            }

            port = null;
            Debug.LogErrorFormat(k_OutOfBoundsInPortIndex, m_GraphName, m_Id, index);

            return false;
        }

        /// <summary>
        /// Tries to get the Output port by the provided index.
        /// </summary>
        /// <param name="index"> Index of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        public bool TryGetOutPort(int index, out OutPort port)
        {
            if (index < m_OutPorts.Count && m_OutPorts.Count > -1)
            {
                port = m_OutPorts[index];
                return true;
            }

            port = null;
            Debug.LogErrorFormat(k_OutOfBoundsOutPortIndex, m_GraphName, m_Id, index);

            return false;
        }

        /// <summary>
        /// Processes the node.
        /// </summary>
        /// <param name="connectionsById"> Dictionary of connections stored by their ids. </param>
        public void Process(Dictionary<string, Connection> connectionsById)
        {
            foreach (var inPort in InPorts)
            {
                if (connectionsById.TryGetValue(inPort.ConnectionId, out var connection))
                {
                    connection.TransferValue();
                }
            }
            
            OnProcess();
        }
    }
}