namespace GiftHorse.ScriptableGraphs
{
    /// <inheritdoc />
    public class ScriptableGraphOf<TNode> : ScriptableGraph 
        where TNode : ScriptableNode
    {
        /// <inheritdoc />
        public override string NodesBaseType => typeof(TNode).AssemblyQualifiedName;

        /// <summary>
        /// Callback called when a <see cref="Connection"/> is created.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the <see cref="Connection"/> goes to. </param>
        protected virtual void OnConnect(TNode fromNode, OutPort fromPort, TNode toNode, InPort toPort) { }

        /// <summary>
        /// Callback called when a <see cref="Connection"/> is removed.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ScriptableNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the <see cref="Connection"/> goes to. </param>
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