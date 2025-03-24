using System.Collections.Generic;
using System.Linq;
using GiftHorse.ScriptableGraphs;

namespace Calculator
{
    public class CalculatorGraph : ScriptableGraphOf<CalculatorNode>
    {
        private readonly HashSet<string> m_VisitedNodes = new();

        /// <summary>
        /// Executes all nodes processes.
        /// </summary>
        public void Process()
        {
            foreach (var node in Nodes)
                node.Process();
        }

        protected override void OnConnect(CalculatorNode fromNode, OutPort fromPort, CalculatorNode toNode, InPort toPort)
        {
            UpdateDependencyLevels(toNode);
            SortNodesByDepthLevel();
        }

        protected override void OnDisconnect(CalculatorNode fromNode, OutPort fromPort, CalculatorNode toNode, InPort toPort)
        {
            UpdateDependencyLevels(toNode);
            SortNodesByDepthLevel();
        }

        private void UpdateDependencyLevels(CalculatorNode node)
        {
            if (!m_VisitedNodes.Any())
                m_VisitedNodes.Clear();

            UpdateDependencyLevelsRecursively(node);
            m_VisitedNodes.Clear();
        }

        private void UpdateDependencyLevelsRecursively(CalculatorNode node)
        {
            if (m_VisitedNodes.Contains(node.Id))
                return;

            // By traversing the subgraph of the inputs without accounting for the nodes that are connected in a circle
            // will result in the dependency level of those nodes to be evaluated as the inputs of the first visited node
            // of the circle, which can lead to some unexpected behavior.
            
            int? maxDependencyLevel = null;
            foreach (var inPort in node.InPorts)
            {
                if (inPort.IsEmpty)
                    continue;
                
                if (!TryGetInputNode(inPort.ConnectionId, out CalculatorNode inNode))
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
                    if (!TryGetOutputNode(connectionId, out CalculatorNode outNode))
                        continue;

                    UpdateDependencyLevelsRecursively(outNode);
                }
            }
        }

        private void SortNodesByDepthLevel()
        {
            ScriptableNodes.Sort((leftNode, rightNode) =>
            {
                var left = (CalculatorNode)leftNode;
                var right = (CalculatorNode)rightNode;
                
                if (left.DepthLevel < right.DepthLevel) return -1;
                if (left.DepthLevel > right.DepthLevel) return  1;

                return 0;
            });
        }
    }
}
