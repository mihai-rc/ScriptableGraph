using System;
using UnityEngine;

namespace GiftHorse.SerializedGraphs
{
    /// <summary>
    /// Common data about a port.
    /// </summary>
    [Serializable]
    public abstract class PortBase
    {
        [SerializeField] private string m_Name;
        [SerializeField] private string m_NodeId;
        [SerializeField] private int m_Index;
        [SerializeField] private string m_CompatibleType;
        
        /// <summary>
        /// Returns true if the port is not part of any <see cref="Connection"/>, otherwise it returns false.
        /// </summary>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// The name of the port.
        /// </summary>
        public string Name => m_Name;

        /// <summary>
        /// The id of the <see cref="ISerializedNode"/> this port belongs to.
        /// </summary>
        public string NodeId => m_NodeId;

        /// <summary>
        /// The index of this port in the <see cref="ISerializedNode"/>'s input or output ports list.
        /// </summary>
        public int Index => m_Index;
        
        /// <summary>
        /// The assembly qualified name of the type this port is compatible with.
        /// </summary>
        public string CompatibleType => m_CompatibleType;

        /// <summary>
        /// <see cref="PortBase"/> constructor.
        /// </summary>
        /// <param name="name"> The name of the port. </param>
        /// <param name="nodeId"> The id of the <see cref="ISerializedNode"/> this port belongs to. </param>
        /// <param name="index"> The index of the port in the <see cref="ISerializedNode"/>'s input or output ports list. </param>
        /// <param name="compatibleType"> The assembly qualified name of the type this port is compatible with. </param>
        protected PortBase(string name, string nodeId, int index, string compatibleType)
        {
            m_Name = name;
            m_NodeId = nodeId;
            m_Index = index;
            m_CompatibleType = compatibleType;
        }
    }
}