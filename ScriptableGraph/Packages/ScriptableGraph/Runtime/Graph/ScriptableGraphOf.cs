using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs
{
    /// <inheritdoc />
    public abstract class ScriptableGraphOf<TNode> : ScriptableGraph where TNode : ScriptableNode
    {
        private const string k_NodeNotFound = "[ScriptableGraph] Node not found! Graph owner: {0}, Node Id: {1}";

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
        protected abstract void OnConnect(TNode fromNode, OutPort fromPort, TNode toNode, InPort toPort);

        /// <summary>
        /// It is called when a connection is removed.
        /// </summary>
        /// <param name="fromNode"> Reference to the node the connection starts from. </param>
        /// <param name="fromPort"> The <see cref="OutPort"/> the connection starts from. </param>
        /// <param name="toNode"> Reference to the node the connection goes to. </param>
        /// <param name="toPort"> The <see cref="InPort"/> the connection goes to. </param>
        protected abstract void OnDisconnect(TNode fromNode, OutPort fromPort, TNode toNode, InPort toPort);

        /// <summary>
        /// Tries to get a node by its id.
        /// </summary>
        /// <param name="nodeId"> The id of the node. </param>
        /// <param name="node"> The reference to the corresponding node. Is null if the id was not found. </param>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        public bool TryGetNodeById(string nodeId, out TNode node)
        {
            node = null;
            if (!IsSceneLoaded)
            {
                Debug.LogErrorFormat(k_SceneNotLoaded, name, gameObject.scene.name);
                return false;
            }

            if (NodesById.TryGetValue(nodeId, out var sceneNode))
            {
                node = (TNode)sceneNode;
                return true;
            }

            Debug.LogErrorFormat(k_NodeNotFound, name, nodeId);
            return false;
        }

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