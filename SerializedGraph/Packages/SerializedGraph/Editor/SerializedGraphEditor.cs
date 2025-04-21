using UnityEditor;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs.Editor
{
    /// <summary>
    /// Custom inspector editor of the classes that inherit from <see cref="ScriptableGraph"/>.
    /// </summary>
    [CustomEditor(typeof(ScriptableGraph), true)]
    public class SerializedGraphEditor : UnityEditor.Editor
    {
        private const string k_ContextMenuPath = "CONTEXT/SerializedGraph/Edit Graph";
        private const string k_ButtonText = "Edit Graph";
        private const float k_ButtonsHeight = 25;

        [MenuItem(k_ContextMenuPath)]
        private static void OpenGraphEditorWindow(MenuCommand command)
        {
            SerializedGraphWindow.Open(command.context as ScriptableGraph);
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            var editGraphButton = GUILayout.Button(k_ButtonText, GUILayout.Height(k_ButtonsHeight));
            if (editGraphButton)
            {
                SerializedGraphWindow.Open(target as ScriptableGraph);
            }

            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}