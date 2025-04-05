using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GiftHorse.SerializedGraphs
{
    /// <summary>
    /// Data about an output port.
    /// </summary>
    [Serializable]
    public class OutPort : PortBase
    {
        [SerializeField]
        private List<string> m_ConnectionIds;

        /// <summary>
        /// List of <see cref="Connection"/>s ids this port is linked to.
        /// </summary>
        public List<string> ConnectionIds => m_ConnectionIds;
        
        /// <inheritdoc />
        public override bool IsEmpty => !m_ConnectionIds?.Any() ?? true;
        
        /// <summary>
        /// <see cref="OutPort"/> constructor.
        /// </summary>
        /// <param name="name"> The name of the port. </param>
        /// <param name="nodeId"> The id of the <see cref="ISerializedNode"/> this port belongs to. </param>
        /// <param name="index"> The index of the port in <see cref="ISerializedNode"/>'s output ports list. </param>
        /// <param name="compatibleType"> The assembly qualified name of the type this port is compatible with. </param>
        public OutPort(string name, string nodeId, int index, string compatibleType) 
            : base(name, nodeId, index, compatibleType)
        {
            m_ConnectionIds = new List<string>();
        }
    }
}