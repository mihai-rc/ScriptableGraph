using System.Collections.Generic;

namespace GiftHorse.SerializedGraphs
{
    /// <summary>
    /// Serialized Graph interface.
    /// </summary>
    public interface ISerializedGraph
    {
        /// <summary>
        /// The name of the graph.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The Assembly Qualified Name of the node's specialized base type that implements from <see cref="ISerializedNode"/>.
        /// </summary>
        string NodesBaseType { get; }

        /// <summary>
        /// Collection of all <see cref="ISerializedNode"/>s sorted in the order of execution.
        /// </summary>
        IEnumerable<ISerializedNode> Nodes { get; }

        /// <summary>
        /// Collection of all <see cref="Connection"/>s.
        /// </summary>
        IEnumerable<Connection> Connections { get; }

        /// <summary>
        /// Sorts the <see cref="ISerializedNode"/>s in the graph by their depth level.
        /// </summary>
        void SortNodes();

        /// <summary>
        /// Executes all <see cref="ISerializedNode"/>s processes.
        /// </summary>
        void Process();

        /// <summary>
        /// Adds a <see cref="ISerializedNode"/> to the graph data structure.
        /// </summary>
        /// <param name="node"> The <see cref="ISerializedNode"/> to be added. </param>
        void AddNode(ISerializedNode node);

        /// <summary>
        /// Removes the <see cref="ISerializedNode"/> from the graph data structure.
        /// </summary>
        /// <param name="node"> The <see cref="ISerializedNode"/> to be removed. </param>
        void RemoveNode(ISerializedNode node);

        /// <summary>
        /// Connects two <see cref="ISerializedNode"/>s at the specified port indices and stores the <see cref="Connection"/> in the graph data structure.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPortIndex"> The index of the port the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPortIndex"> The index of the port the <see cref="Connection"/> goes to. </param>
        void ConnectNodes(ISerializedNode fromNode, int fromPortIndex, ISerializedNode toNode, int toPortIndex);

        /// <summary>
        /// Disconnects two <see cref="ISerializedNode"/>s at the specified port indices and removes the <see cref="Connection"/> from the graph data structure.
        /// </summary>
        /// <param name="fromNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> starts from. </param>
        /// <param name="fromPortIndex"> The index of the port the <see cref="Connection"/> starts from. </param>
        /// <param name="toNode"> Reference to the <see cref="ISerializedNode"/> the <see cref="Connection"/> goes to. </param>
        /// <param name="toPortIndex"> The index of the port the <see cref="Connection"/> goes to. </param>
        void DisconnectNodes(ISerializedNode fromNode, int fromPortIndex, ISerializedNode toNode, int toPortIndex);

        /// <summary>
        /// Updates <see cref="ISerializedNode"/>s and <see cref="Connection"/>s mappings on editor undo.
        /// </summary>
        void UpdateMappings();

        /// <summary>
        /// Tries to get a <see cref="ISerializedNode"/> by its id.
        /// </summary>
        /// <param name="nodeId"> The id of the node. </param>
        /// <param name="node"> The reference to the corresponding node. Is null if the id was not found. </param>
        /// <typeparam name="T"> The subtype the node is expected to be received as. </typeparam>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        bool TryGetNodeById<T>(string nodeId, out T node) where T : class, ISerializedNode;

        /// <summary>
        /// Tries to get the origin <see cref="ISerializedNode"/> of a <see cref="Connection"/>.
        /// </summary>
        /// <param name="connectionId"> The id of the <see cref="Connection"/>. </param>
        /// <param name="node"> The reference to the origin node. Is null if the node was not found. </param>
        /// <typeparam name="T"> The subtype the node is expected to be received as. </typeparam>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        bool TryGetInputNode<T>(string connectionId, out T node) where T : class, ISerializedNode;

        /// <summary>
        /// Tries to get the destination <see cref="ISerializedNode"/> of a <see cref="Connection"/>.
        /// </summary>
        /// <param name="connectionId"> The id of the <see cref="Connection"/>. </param>
        /// <param name="node"> The reference to the destination node. Is null if the node was not found. </param>
        /// <typeparam name="T"> The subtype the node is expected to be received as. </typeparam>
        /// <returns> Returns true if the node was found, otherwise returns false. </returns>
        bool TryGetOutputNode<T>(string connectionId, out T node) where T : class, ISerializedNode;

        /// <summary>
        /// Tries to get a <see cref="Connection"/> by its id.
        /// </summary>
        /// <param name="connectionId"> The id of the <see cref="Connection"/>. </param>
        /// <param name="connection"> The reference to the corresponding <see cref="Connection"/>. Is null if the id was not found. </param>
        /// <returns> Returns true if the <see cref="Connection"/> was found, otherwise returns false. </returns>
        bool TryGetConnectionById(string connectionId, out Connection connection);
    }
}