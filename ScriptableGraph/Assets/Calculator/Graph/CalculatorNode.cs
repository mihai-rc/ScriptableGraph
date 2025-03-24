using UnityEngine;
using GiftHorse.ScriptableGraphs;

namespace Calculator
{
    public abstract class CalculatorNode : ScriptableNode
    {
        [SerializeField] private int m_DepthLevel;

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
        /// Processes the node.
        /// </summary>
        public void Process()
        {
            foreach (var inPort in InPorts)
            {
                if (Graph.TryGetConnectionById(inPort.ConnectionId, out var connection))
                {
                    connection.TransferValue();
                }
            }
            
            OnProcess();
        }
    }    
}
