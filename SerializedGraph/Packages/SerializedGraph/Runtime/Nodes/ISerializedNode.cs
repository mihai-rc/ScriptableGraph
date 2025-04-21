using System;
using System.Collections.Generic;
using UnityEngine;

namespace GiftHorse.SerializedGraphs
{
    /// <summary>
    /// Serialized Node interface.
    /// </summary>
    public interface  ISerializedNode : IDisposable
    {
        /// <summary>
        /// Reference to the <see cref="ISerializedGraph"/> that owns this <see cref="ISerializedNode"/>.
        /// </summary>
        ISerializedGraph Graph { get; }

        /// <summary>
        /// The id of this node.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The name of the node.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Number of nodes in the longest input chain this node is part of. It is used by
        /// the sorting algorithm to figure out in which order the nodes should be evaluated.
        /// </summary>
        int DepthLevel { get; set; }

        /// <summary>
        /// <see cref="Rect"/> used by Unity Editor to manage node's position in the Graph View.
        /// </summary>
        public Rect Position { get; set; }

        /// <summary>
        /// Flag used by Unity Editor to toggle between Expanded/Collapse states in the Graph View.
        /// </summary>
        public bool Expanded { get; set; }

        /// <summary>
        /// Collection of all <see cref="InPort"/>s of the node.
        /// </summary>
        IEnumerable<InPort> InPorts { get; }

        /// <summary>
        /// Collection of all <see cref="OutPort"/>s of the node.
        /// </summary>
        IEnumerable<OutPort> OutPorts { get; }

        /// <summary>
        /// Initializes the node and its ports.
        /// </summary>
        /// <param name="graph"> The <see cref="ISerializedGraph"/> the owns this node. </param>
        void Init(ISerializedGraph graph);

        /// <summary>
        /// Processes the node.
        /// </summary>
        void Process();

        /// <summary>
        /// Tries to get the <see cref="InPort"/> with the provided name.
        /// </summary>
        /// <param name="inputName"> Name of the <see cref="InPort"/> to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Whether the <see cref="InPort"/> was found or not. </returns>
        bool TryGetInPort(string inputName, out InPort port);

        /// <summary>
        /// Tries to get the <see cref="OutPort"/> with the provided name.
        /// </summary>
        /// <param name="outputName"> Name of the <see cref="OutPort"/> to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Whether the <see cref="OutPort"/> was found or not. </returns>
        bool TryGetOutPort(string outputName, out OutPort port);

        /// <summary>
        /// Tries to get the <see cref="InPort"/> at the provided index.
        /// </summary>
        /// <param name="index"> Index of the <see cref="InPort"/> to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Whether the <see cref="InPort"/> was found or not. </returns>
        public bool TryGetInPort(int index, out InPort port);

        /// <summary>
        /// Tries to get the <see cref="OutPort"/> at the provided index.
        /// </summary>
        /// <param name="index"> Index of the <see cref="OutPort"/> to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Whether the <see cref="OutPort"/> was found or not. </returns>
        public bool TryGetOutPort(int index, out OutPort port);

        /// <summary>
        /// Tries to get the node connected to the <see cref="InPort"/> with the provided name.
        /// </summary>
        /// <param name="portName">
        /// Name of the <see cref="InPort"/> whose connected node is being fetched.
        /// </param>
        /// <param name="node">
        /// Reference to the node connected to the specified <see cref="InPort"/>.
        /// Is null if the port name is invalid or the port is not connected.
        /// </param>
        /// <returns>
        /// Whether the node was found or not.
        /// </returns>
        public bool TryGetInputNodeOf(string portName, out ISerializedNode node);

        /// <summary>
        /// Tries to get the nodes connected to the <see cref="OutPort"/> with the provided name.
        /// </summary>
        /// <param name="portName">
        /// Name of the <see cref="OutPort"/> whose connected nodes are being fetched.
        /// </param>
        /// <param name="nodes">
        /// List to be populated with nodes connected to the specified <see cref="OutPort"/>.
        /// </param>
        /// <returns>
        /// Whether the nodes were found or not.
        /// </returns>
        public bool TryGetOutputNodesOf(string portName, List<ISerializedNode> nodes);
    }
}