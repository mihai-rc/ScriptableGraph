using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs.Editor
{
    public class ScriptableGraphEditorContext
    {
        public ScriptableGraphWindow Window { get; }
        public ScriptableGraph Graph { get; private set; }
        public ScriptableGraphView GraphView { get; private set; }
        public SerializedObject SerializedObject { get; private set; }
        
        public bool HasUnsavedChanges => EditorUtility.IsDirty(Graph);

        private NodeSearchWindow m_SearchWindow;

        public ScriptableGraphEditorContext(ScriptableGraph graph, ScriptableGraphWindow window)
        {
            Graph = graph;
            Window = window;
        }
        
        public void DrawGraphView()
        {
            SerializedObject = new SerializedObject(Graph);
            GraphView = new ScriptableGraphView(this);
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

        public void MarkAssetAsDirty()
        {
            if (Graph == null)
            {
                if (Window.TryGetScriptableGraph(out var graph))
                    Graph = graph;
            }
                
            EditorUtility.SetDirty(Graph);
        }
    }
}