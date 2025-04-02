using System;
using System.Collections.Generic;
using UnityEngine;

namespace GiftHorse.SerializedGraphs
{
    /// <summary>
    /// Serialized Node interface.
    /// </summary>
    public interface ISerializedNode : IDisposable
    {
        /// <summary>
        /// Reference to the <see cref="SerializedGraphBase"/> that owns this node.
        /// </summary>
        SerializedGraphBase Graph { get; }

        /// <summary>
        /// The id of this node.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The title of the node. It is displayed on node view header.
        /// </summary>
        string Title { get; }

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
        /// Collection of all input ports of the node.
        /// </summary>
        IEnumerable<InPort> InPorts { get; }

        /// <summary>
        /// Collection of all output ports of the node.
        /// </summary>
        IEnumerable<OutPort> OutPorts { get; }

        /// <summary>
        /// Initializes ports values and binds all references.
        /// </summary>
        /// <param name="graph"> The <see cref="SerializedGraphBase"/> the owns this node. </param>
        void Init(SerializedGraphBase graph);

        /// <summary>
        /// Processes the node.
        /// </summary>
        void Process();

        /// <summary>
        /// Tries to find the Input port with the provided name.
        /// </summary>
        /// <param name="inputName"> Name of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        bool TryFindInPortByName(string inputName, out InPort port);

        /// <summary>
        /// Tries to find the Output port with the provided name.
        /// </summary>
        /// <param name="outputName"> Name of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        bool TryFindOutPortByName(string outputName, out OutPort port);

        /// <summary>
        /// Tries to get the Input port at the provided index.
        /// </summary>
        /// <param name="index"> Index of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        public bool TryGetInPortByIndex(int index, out InPort port);

        /// <summary>
        /// Tries to get the Output port at the provided index.
        /// </summary>
        /// <param name="index"> Index of the port to be retrieved. </param>
        /// <param name="port"> Reference of the retrieved port. Is null if the port was not found. </param>
        /// <returns> Returns true if the port was found, otherwise returns false. </returns>
        public bool TryGetOutPortByIndex(int index, out OutPort port);
    }
}