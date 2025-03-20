using System.Collections.Generic;
using System.Linq;

namespace GiftHorse.ScriptableGraphs
{
    /// <inheritdoc />
    public class ScriptableGraphOf<TNode> : ScriptableGraph where TNode : ScriptableNode
    {
        /// <inheritdoc />
        public override string NodesBaseType => typeof(TNode).AssemblyQualifiedName;

        /// <summary>
        /// Collection of all nodes.
        /// </summary>
        public IEnumerable<TNode> Nodes => ScriptableNodes.Select(node => node as TNode);

        /// <summary>
        /// It is called when a connection is created.
        /// </summary>
        /// <param name="fromNode"> Reference to the node the connection starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the connection starts from. </param>
        /// <param name="toNode"> Reference to the node the connection goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the connection goes to. </param>
        protected virtual void OnConnect(TNode fromNode, OutPort fromPort, TNode toNode, InPort toPort) { }

        /// <summary>
        /// It is called when a connection is removed.
        /// </summary>
        /// <param name="fromNode"> Reference to the node the connection starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the connection starts from. </param>
        /// <param name="toNode"> Reference to the node the connection goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the connection goes to. </param>
        protected virtual void OnDisconnect(TNode fromNode, OutPort fromPort, TNode toNode, InPort toPort) { }

        /// <inheritdoc />
        protected override void OnConnectionCreated(ScriptableNode fromNode, OutPort fromPort, ScriptableNode toNode, InPort toPort)
        {
            OnConnect(fromNode as TNode, fromPort, toNode as TNode, toPort);
        }

        /// <inheritdoc />
        protected override void OnConnectionRemoved(ScriptableNode fromNode, OutPort fromPort, ScriptableNode toNode, InPort toPort)
        {
            OnDisconnect(fromNode as TNode, fromPort, toNode as TNode, toPort);
        }
    }
}