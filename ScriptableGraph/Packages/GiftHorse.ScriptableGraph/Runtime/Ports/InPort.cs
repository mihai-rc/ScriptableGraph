using System;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs
{
    /// <summary>
    /// Data about an input port.
    /// </summary>
    [Serializable]
    public class InPort : PortBase
    {
        [SerializeField] 
        private string m_ConnectionId;

        /// <summary>
        /// The id of the connection this port is linked to.
        /// </summary>
        public string ConnectionId
        {
            get => m_ConnectionId;
            set => m_ConnectionId = value;
        }
        
        /// <inheritdoc />
        public override bool IsConnected => m_ConnectionId is not null;
        
        /// <summary>
        /// <see cref="InPort"/> constructor.
        /// </summary>
        /// <param name="name"> The name of the port. </param>
        /// <param name="nodeId"> The id of the node this port belongs to. </param>
        /// <param name="index"> The index of the port in node's input ports list. </param>
        /// <param name="compatibleType"> The assembly qualified name of the type this port is compatible with. </param>
        public InPort(string name, string nodeId, int index, string compatibleType) 
            : base(name, nodeId, index, compatibleType)
        { }
    }
}