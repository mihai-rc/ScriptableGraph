using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GiftHorse.ScriptableGraphs.Editor
{
    /// <summary>
    /// <see cref="EditorWindow"/> class used to display a <see cref="ScriptableGraphView"/> in the editor.
    /// </summary>
    public class ScriptableGraphWindow : EditorWindow
    {
        private const string k_WindowIcon = "d_SceneViewTools";
        private const string k_ComponentNotFound = "[Editor] [ScriptableGraph] ScriptableGraph component couldn't be found at the cached path: {0}.";

        /// <summary>
        /// Opens a <see cref="ScriptableGraphWindow"/> for the given <see cref="ScriptableGraph"/>.
        /// </summary>
        /// <param name="sceneAsset"> Reference to the graph component the window will edit. </param>
        public static void Open(ScriptableGraph sceneAsset)
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
        /// Tries to find the <see cref="ScriptableGraph"/> component in the active scene from the cached path.
        /// </summary>
        /// <param name="graph"> The reference to the <see cref="ScriptableGraph"/> if found, otherwise is null. </param>
        /// <returns> Returns whether the <see cref="ScriptableGraph"/> was found or not. </returns>
        public bool TryGetScriptableGraph(out ScriptableGraph graph)
        {
            graph = null;
            if (string.IsNullOrEmpty(m_GraphHierarchyPath)) 
                return false;

            // This way we can keep the window open between editor sessions but this method
            // only works if the owner GameObject has only one ScriptableGraph component attached.

            graph = GameObject
                .Find(m_GraphHierarchyPath)
                .GetComponent<ScriptableGraph>();

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

        private void Load(ScriptableGraph scriptableGraph)
        {
            m_GraphHierarchyPath = GetHierarchyPath(scriptableGraph);
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
            
            CreateGraph(scriptableGraph);
        }

        private void CreateGraph(ScriptableGraph scriptableGraph)
        {
            m_Context = new ScriptableGraphEditorContext(scriptableGraph, this);
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