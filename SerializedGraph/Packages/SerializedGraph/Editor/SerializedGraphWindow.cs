using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GiftHorse.ScriptableGraphs.Editor
{
    /// <summary>
    /// <see cref="EditorWindow"/> class used to display a <see cref="SerializedGraphView"/> in the editor.
    /// </summary>
    public class SerializedGraphWindow : EditorWindow
    {
        private const string k_WindowIcon = "d_SceneViewTools";
        private const string k_ComponentNotFound = "[Editor] [ScriptableGraph] Graph couldn't be found at the cached path: {0}.";

        /// <summary>
        /// Opens a <see cref="SerializedGraphWindow"/> for the given <see cref="ScriptableGraph"/>.
        /// </summary>
        /// <param name="sceneAsset"> Reference to the graph asset the window will edit. </param>
        public static void Open(ScriptableGraph sceneAsset)
        {
            var windows = Resources.FindObjectsOfTypeAll<SerializedGraphWindow>();
            foreach (var window in windows)
            {
                if (window.m_Context is null) 
                    continue;

                if (window.m_Context.Graph != sceneAsset) 
                    continue;

                window.Focus();
                return;
            }

            var newWindow = CreateWindow<SerializedGraphWindow>(typeof(SerializedGraphWindow), typeof(SceneView));
            newWindow.titleContent = new GUIContent(sceneAsset.name, EditorGUIUtility.IconContent(k_WindowIcon).image);
            newWindow.Load(sceneAsset);
        }

        [SerializeField] private ScriptableGraph m_GraphAsset;
        private SerializedGraphEditorContext m_Context;

        /// <summary>
        /// Tries to find the <see cref="ScriptableGraph"/> component in the active scene from the cached path.
        /// </summary>
        /// <param name="graph"> The reference to the <see cref="ScriptableGraph"/> if found, otherwise is null. </param>
        /// <returns> Returns whether the <see cref="ScriptableGraph"/> was found or not. </returns>
        // public bool TryGetSerializedGraph(out ScriptableGraph graph)
        // {
        //     graph = null;
        //     if (string.IsNullOrEmpty(m_GraphAsset)) 
        //         return false;
        //
        //     // This way we can keep the window open between editor sessions but this method
        //     // only works if the owner GameObject has only one ScriptableGraph component attached.
        //
        //     graph = GameObject
        //         .Find(m_GraphAsset)
        //         .GetComponent<ScriptableGraph>();
        //
        //     return true;
        // }

        // private void OnEnable()
        // {
        //     if (!TryGetSerializedGraph(out var graph)) 
        //         return;
        //
        //     if (graph is null)
        //     {
        //         CloseWindow();
        //         return;
        //     }
        //
        //     Load(graph);
        // }

        // private void OnDestroy() => EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged; 

        private void OnGUI()
        {
            if (m_Context is null) 
                return;

            hasUnsavedChanges = m_Context.HasUnsavedChanges;
        }

        private void Load(ScriptableGraph graph)
        {
            m_GraphAsset = graph;
            m_Context = new SerializedGraphEditorContext(graph, this);
            m_Context.DrawGraphView();
        }

        // private void OnSceneChanged(Scene from, Scene to)
        // {
        //     if (m_Scene == to)
        //         return;
        //
        //     CloseWindow();
        // }

        // private string GetHierarchyPath(Component component)
        // {
        //     if (component is null)
        //     {
        //         Debug.LogWarningFormat(k_ComponentNotFound, m_GraphAsset);
        //         return string.Empty;
        //     }
        //
        //     var current = component.transform;
        //     var path = component.gameObject.name;
        //     while (current.parent is not null)
        //     {
        //         current = current.parent;
        //         path = $"{current.name}/{path}";
        //     }
        //
        //     return path;
        // }
    }
}