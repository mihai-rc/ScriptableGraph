using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GiftHorse.SerializedGraphs.Editor
{
    public class SerializedGraphEditorContext
    {
        /// <summary>
        /// Reference to the graph component.
        /// </summary>
        public SerializedGraphBase Graph { get; private set; }

        /// <summary>
        /// The <see cref="SerializedGraphView"/> that edits the <see cref="SerializedGraphBase"/>.
        /// </summary>
        public SerializedGraphView GraphView { get; private set; }

        /// <summary>
        /// The serialized object of the <see cref="SerializedGraphBase"/>.
        /// </summary>
        public SerializedObject SerializedObject { get; private set; }

        /// <summary>
        /// The editor window that contains the <see cref="SerializedGraphView"/>.
        /// </summary>
        public SerializedGraphWindow Window { get; }

        /// <summary>
        /// Whether the graph has unsaved changes or not.
        /// </summary>
        public bool HasUnsavedChanges => EditorUtility.IsDirty(Graph);

        private NodeSearchWindow m_SearchWindow;

        /// <summary>
        /// <see cref="SerializedGraphEditorContext"/>'s constructor.
        /// </summary>
        /// <param name="graph"> Reference to the graph component. </param>
        /// <param name="window"> The editor window that contains the <see cref="SerializedGraphView"/>. </param>
        public SerializedGraphEditorContext(SerializedGraphBase graph, SerializedGraphWindow window)
        {
            Graph = graph;
            Window = window;
        }
        
        /// <summary>
        /// Draws the <see cref="SerializedGraphView"/> in the editor window.
        /// </summary>
        public void DrawGraphView()
        {
            SerializedObject = new SerializedObject(Graph);
            GraphView = new SerializedGraphView(this);
            GraphView.graphViewChanged += change =>
            {
                MarkAssetAsDirty();
                return change;
            };

            GraphView.nodeCreationRequest += request =>
            {
                SearchWindow.Open(new SearchWindowContext(request.screenMousePosition), m_SearchWindow);
            };

            m_SearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            m_SearchWindow.Init(this);
            
            Window.rootVisualElement.Add(GraphView);
        }

        /// <summary>
        /// Marks the graph as dirty, so it can be saved.
        /// </summary>
        public void MarkAssetAsDirty()
        {
            if (Graph == null)
            {
                if (Window.TryGetSerializedGraph(out var graph))
                    Graph = graph;
            }

            EditorUtility.SetDirty(Graph);
        }
    }
}