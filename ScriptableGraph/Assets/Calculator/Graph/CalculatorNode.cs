using GiftHorse.ScriptableGraphs;
using GiftHorse.ScriptableGraphs.Attributes;

namespace Calculator
{
    public abstract class CalculatorNode : ScriptableNode
    {
        /// <summary>
        /// Number of nodes in the longest input chain this node is part of. It is used by
        /// the sorting algorithm to figure out in which order the nodes should be evaluated.
        /// </summary>
        [NodeField] public int DepthLevel = 0;
    }    
}
