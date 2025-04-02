namespace GiftHorse.SerializedGraphs
{
    /// <inheritdoc />
    public class SerializedGraphOf<TNode> : SerializedGraphBase 
        where TNode : class, ISerializedNode
    {
        /// <inheritdoc />
        public override string NodesBaseType => typeof(TNode).AssemblyQualifiedName;

        /// <summary>
        /// Callback called when a connection is created.
        /// </summary>
        /// <param name="fromNode"> Reference to the node the connection starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the connection starts from. </param>
        /// <param name="toNode"> Reference to the node the connection goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the connection goes to. </param>
        protected virtual void OnConnect(TNode fromNode, OutPort fromPort, TNode toNode, InPort toPort) { }

        /// <summary>
        /// Callback called when a connection is removed.
        /// </summary>
        /// <param name="fromNode"> Reference to the node the connection starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the connection starts from. </param>
        /// <param name="toNode"> Reference to the node the connection goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the connection goes to. </param>
        protected virtual void OnDisconnect(TNode fromNode, OutPort fromPort, TNode toNode, InPort toPort) { }

        /// <inheritdoc />
        protected override void OnConnectionCreated(ISerializedNode fromNode, OutPort fromPort, ISerializedNode toNode, InPort toPort)
        {
            OnConnect(fromNode as TNode, fromPort, toNode as TNode, toPort);
        }

        /// <inheritdoc />
        protected override void OnConnectionRemoved(ISerializedNode fromNode, OutPort fromPort, ISerializedNode toNode, InPort toPort)
        {
            OnDisconnect(fromNode as TNode, fromPort, toNode as TNode, toPort);
        }
    }
}