using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GiftHorse.SerializedGraphs.Editor
{
    /// <summary>
    /// <see cref="EditorWindow"/> class used to display a <see cref="ScriptableGraphView"/> in the editor.
    /// </summary>
    public class ScriptableGraphWindow : EditorWindow
    {
        private const string k_WindowIcon = "d_SceneViewTools";
        private const string k_ComponentNotFound = "[Editor] [SerializedGraph] SerializedGraph component couldn't be found at the cached path: {0}.";

        /// <summary>
        /// Opens a <see cref="ScriptableGraphWindow"/> for the given <see cref="SerializedGraphBase"/>.
        /// </summary>
        /// <param name="sceneAsset"> Reference to the graph component the window will edit. </param>
        public static void Open(SerializedGraphBase sceneAsset)
        {
            var windows = Resources.FindObjectsOfTypeAll<ScriptableGraphWindow>();
            foreach (var window in windows)
            {
                if (window.m_Context is null) 
                    continue;

                if (window.m_Context.Graph != sceneAsset) 
                    continue;

                window.Focus();
                return;
            }

            var newWindow = CreateWindow<ScriptableGraphWindow>(typeof(ScriptableGraphWindow), typeof(SceneView));
            newWindow.titleContent = new GUIContent(sceneAsset.name, EditorGUIUtility.IconContent(k_WindowIcon).image);
            newWindow.Load(sceneAsset);
        }

        [SerializeField] private string m_GraphHierarchyPath;
        private ScriptableGraphEditorContext m_Context;
        private Scene m_Scene;

        /// <summary>
        /// Tries to find the <see cref="SerializedGraphBase"/> component in the active scene from the cached path.
        /// </summary>
        /// <param name="graph"> The reference to the <see cref="SerializedGraphBase"/> if found, otherwise is null. </param>
        /// <returns> Returns whether the <see cref="SerializedGraphBase"/> was found or not. </returns>
        public bool TryGetScriptableGraph(out SerializedGraphBase graph)
        {
            graph = null;
            if (string.IsNullOrEmpty(m_GraphHierarchyPath)) 
                return false;

            // This way we can keep the window open between editor sessions but this method
            // only works if the owner GameObject has only one SerializedGraphBase component attached.

            graph = GameObject
                .Find(m_GraphHierarchyPath)
                .GetComponent<SerializedGraphBase>();

            return true;
        }

        private void OnEnable()
        {
            if (!TryGetScriptableGraph(out var graph)) 
                return;

            if (graph is null)
            {
                CloseWindow();
                return;
            }

            m_Scene = graph.gameObject.scene;
            Load(graph);
        }

        private void OnDestroy() => EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged; 

        private void OnGUI()
        {
            if (m_Context is null) 
                return;

            hasUnsavedChanges = m_Context.HasUnsavedChanges;
        }

        private void Load(SerializedGraphBase graph)
        {
            m_GraphHierarchyPath = GetHierarchyPath(graph);
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
            
            CreateGraph(graph);
        }

        private void CreateGraph(SerializedGraphBase graph)
        {
            m_Context = new ScriptableGraphEditorContext(graph, this);
            m_Context.DrawGraphView();
        }

        private void CloseWindow()
        {
            m_GraphHierarchyPath = string.Empty;
            Close();
        }

        private void OnSceneChanged(Scene from, Scene to)
        {
            if (m_Scene == to)
                return;
            
            CloseWindow();
        }

        private string GetHierarchyPath(Component component)
        {
            if (component is null)
            {
                Debug.LogWarningFormat(k_ComponentNotFound, m_GraphHierarchyPath);
                return string.Empty;
            }

            var current = component.transform;
            var path = component.gameObject.name;
            while (current.parent is not null)
            {
                current = current.parent;
                path = $"{current.name}/{path}";
            }

            return path;
        }
    }
}