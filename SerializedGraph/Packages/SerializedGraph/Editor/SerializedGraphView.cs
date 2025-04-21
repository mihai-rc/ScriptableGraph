using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GiftHorse.SerializedGraphs.Editor
{
    /// <summary>
    /// <see cref="GraphView"/> class used to display the <see cref="SerializedGraphBase"/> in the editor.
    /// </summary>
    public class SerializedGraphView : GraphView
    {
        private const string k_UssFilePath = "Packages/com.gift-horse.serialized-graph/Editor/Uss/SerializedGraphView.uss";
        private const string k_UssFileNotFoundError = "[Editor] [SerializedGraph] No uss file was found at: {0}.";
        private const string k_BackgroundName = "Background";
        private const string k_AddNodeUndoRecord = "Added Graph Node";
        private const string k_AddConnectionUndoRecord = "Added Graph Connections";
        private const string k_MoveNodeUndoRecord = "Moved Graph Nodes";
        private const string k_RemoveElementUndoRecord = "Removed Graph Element";

        private readonly SerializedGraphEditorContext m_Context;
        private readonly List<SerializedNodeView> m_NodeViews = new();
        private readonly List<Edge> m_EdgeViews = new();
        private readonly Dictionary<string, SerializedNodeView> m_NodeViewsByNodeId = new();

        /// <summary>
        /// <see cref="SerializedGraphView"/>'s constructor.
        /// </summary>
        /// <param name="context"> Reference to the <see cref="SearchWindowContext"/> to access relevant dependencies and draw the graph. </param>
        public SerializedGraphView(SerializedGraphEditorContext context)
        {
            m_Context = context;

            LoadUssFile();
            SetupBackground();
            SetupManipulators();
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // DrawBlackboard();
            DrawNodes();
            DrawConnections();
            UpdateExpandedNodes();

            graphViewChanged += OnGraphViewChanged;
            Undo.undoRedoPerformed += RepaintGraph;
        }

        ~SerializedGraphView()
        {
            Undo.undoRedoPerformed -= RepaintGraph;
        }

        /// <inheritdoc />
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var allPorts = new List<Port>();
            var compatiblePorts = new List<Port>();
            foreach (var node in m_NodeViews)
            {
                allPorts.AddRange(node.InPorts);
                allPorts.AddRange(node.OutPorts);
            }

            foreach (var port in allPorts)
            {
                if (port == startPort) continue;
                if (port.node == startPort.node) continue;
                if (port.direction == startPort.direction) continue;
                if (port.portType != startPort.portType) continue;

                compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }

        /// <summary>
        /// Adds a <see cref="ISerializedNode"/> to the graph data structure when an entry is selected from <see cref="NodeSearchWindow"/>.
        /// </summary>
        /// <param name="node"> Reference to the newly created <see cref="ISerializedNode"/>. </param>
        public void Add(ISerializedNode node)
        {
            Undo.RecordObject(m_Context.SerializedObject.targetObject, k_AddNodeUndoRecord);

            m_Context.Graph.AddNode(node);
            m_Context.SerializedObject.Update();

            AddNodeToGraph(node);
            this.Bind(m_Context.SerializedObject);

            m_Context.MarkAssetAsDirty();
        }

        private void LoadUssFile()
        {
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_UssFilePath);
            if (uss is null)
            {
                Debug.LogErrorFormat(k_UssFileNotFoundError, k_UssFilePath);
                return;
            }

            styleSheets.Add(uss);
        }

        private void SetupBackground()
        {
            var background = new GridBackground { name = k_BackgroundName };
            Add(background);
            background.SendToBack();
        }

        private void SetupManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }

        // private void DrawBlackboard()
        // {
        //     var blackboard = new Blackboard(this)
        //     {
        //         title = "Blackboard",
        //         scrollable = true,
        //         addItemRequested = blackboard =>
        //         {
        //             var newBlackboardField = new BlackboardField
        //             {
        //                 text = "New Variable",
        //                 typeText = "Type",
        //                 // type = typeof(string)
        //             };
        //
        //             // var container = new VisualElement();
        //             // container.Add(newBlackboardField);
        //             blackboard.Add(newBlackboardField);
        //         }
        //     };
        //
        //     blackboard.Add(new BlackboardSection 
        //     {
        //         title = "Properties",
        //         // type = typeof(string)
        //     });
        //
        //     blackboard.SetPosition(new Rect(10, 10, 200, 200));
        //     Add(blackboard);
        // }

        private void DrawNodes()
        {
            foreach (var node in m_Context.Graph.Nodes)
                AddNodeToGraph(node);

            this.Bind(m_Context.SerializedObject);
        }

        private void DrawConnections()
        {
            if (m_Context.Graph.Connections is null)
                return;

            foreach (var connection in m_Context.Graph.Connections)
                DrawConnection(connection);
        }

        private void DrawConnection(Connection connection)
        {
            var fromPort = connection.FromPort;
            var toPort = connection.ToPort;

            m_NodeViewsByNodeId.TryGetValue(fromPort.NodeId, out var fromNodeView);
            m_NodeViewsByNodeId.TryGetValue(toPort.NodeId, out var toNodeView);

            var fromPortView = fromNodeView.OutPorts[fromPort.Index];
            var toPortView = toNodeView.InPorts[toPort.Index];
            var edge = fromPortView.ConnectTo(toPortView);
            m_EdgeViews.Add(edge);

            AddElement(edge);
        }

        private void UpdateExpandedNodes()
        {
            foreach (var node in m_NodeViews)
                node.expanded = node.SerializedNode.Expanded;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements is not null)
            {
                Undo.RecordObject(m_Context.SerializedObject.targetObject, k_MoveNodeUndoRecord);

                foreach (var movedNode in graphViewChange.movedElements.OfType<SerializedNodeView>())
                    movedNode.SavePosition();
            }

            if (graphViewChange.elementsToRemove is not null)
            {
                Undo.RecordObject(m_Context.SerializedObject.targetObject, k_RemoveElementUndoRecord);

                foreach (var removedEdge in graphViewChange.elementsToRemove.OfType<Edge>())
                    RemoveConnection(removedEdge);

                foreach (var removedNode in graphViewChange.elementsToRemove.OfType<SerializedNodeView>())
                    RemoveNodeFromGraph(removedNode);

                RepaintGraph();
            }

            if (graphViewChange.edgesToCreate is not null)
            {
                Undo.RegisterCompleteObjectUndo(m_Context.SerializedObject.targetObject, k_AddConnectionUndoRecord);

                foreach (var edge in graphViewChange.edgesToCreate)
                    CreateConnection(edge);
            }

            return graphViewChange;
        }

        private void RepaintGraph()
        {
            foreach (var edge in m_EdgeViews)
                RemoveElement(edge);

            foreach (var node in m_NodeViews)
                RemoveElement(node);

            m_EdgeViews.Clear();
            m_NodeViews.Clear();
            m_NodeViewsByNodeId.Clear();
            m_Context.Graph.UpdateMappings();

            DrawNodes();
            DrawConnections();
        }

        private void AddNodeToGraph(ISerializedNode node)
        {
            var nodeView = new SerializedNodeView(node, m_Context);
            nodeView.SetPosition(node.Position);

            m_NodeViews.Add(nodeView);
            m_NodeViewsByNodeId.Add(node.Id, nodeView);

            AddElement(nodeView);
        }

        private void RemoveNodeFromGraph(SerializedNodeView nodeView)
        {
            m_Context.Graph.RemoveNode(nodeView.SerializedNode);
            m_Context.SerializedObject.Update();
            Undo.DestroyObjectImmediate(nodeView.SerializedNode as SerializedNodeBase);

            m_NodeViewsByNodeId.Remove(nodeView.SerializedNode.Id);
            m_NodeViews.Remove(nodeView);
        }

        private void CreateConnection(Edge edge)
        {
            var toNodeView = edge.input.node as SerializedNodeView;
            var toIndex = toNodeView.InPorts.IndexOf(edge.input);
            var toNode = toNodeView.SerializedNode;

            var fromNodeView = edge.output.node as SerializedNodeView;
            var fromIndex = fromNodeView.OutPorts.IndexOf(edge.output);
            var fromNode = fromNodeView.SerializedNode;

            m_EdgeViews.Add(edge);
            m_Context.Graph.ConnectNodes(fromNode, fromIndex, toNode, toIndex);
        }

        private void RemoveConnection(Edge edge)
        {
            var toNodeView = edge.input.node as SerializedNodeView;
            var toIndex = toNodeView.InPorts.IndexOf(edge.input);
            var toNode = toNodeView.SerializedNode;

            var fromNodeView = edge.output.node as SerializedNodeView;
            var fromIndex = fromNodeView.OutPorts.IndexOf(edge.output);
            var fromNode = fromNodeView.SerializedNode;

            m_Context.Graph.DisconnectNodes(fromNode, fromIndex, toNode, toIndex);
        }
    }
}