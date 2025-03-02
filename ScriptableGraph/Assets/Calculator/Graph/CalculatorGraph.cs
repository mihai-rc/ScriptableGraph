using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GiftHorse.ScriptableGraphs;

namespace Calculator
{
    public class CalculatorGraph : ScriptableGraphOf<CalculatorNode>
    {
        private const string k_ConnectionNotFound = "[ScriptableGraph] Connection not found! Graph owner: {0}, Connection Id: {1}";
        private readonly HashSet<string> m_VisitedNodes = new();

        protected override void OnConnect(CalculatorNode fromNode, OutPort fromPort, CalculatorNode toNode, InPort toPort)
        {
            UpdateDependencyLevels(toNode);
            SortNodesByDependencyLevel();
        }

        protected override void OnDisconnect(CalculatorNode fromNode, OutPort fromPort, CalculatorNode toNode, InPort toPort)
        {
            UpdateDependencyLevels(toNode);
            SortNodesByDependencyLevel();
        }

        /// <summary>
        /// Executes all nodes processes.
        /// Set as callback on button click.
        /// </summary>
        public void Process()
        {
            foreach (var node in Nodes)
                node.Process(ConnectionsById);
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
                if (inPort.ConnectionId is null)
                    continue;

                if (!ConnectionsById.TryGetValue(inPort.ConnectionId, out var connection))
                    continue;

                if (!TryGetNodeById(connection.FromPort.NodeId, out var inNode))
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
                    if (!ConnectionsById.TryGetValue(connectionId, out var connection))
                    {
                        Debug.LogErrorFormat(k_ConnectionNotFound, name, connectionId);
                        continue;
                    }

                    if (!TryGetNodeById(connection.ToPort.NodeId, out var outNode))
                        continue;

                    UpdateDependencyLevelsRecursively(outNode);
                }
            }
        }

        private void SortNodesByDependencyLevel()
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
