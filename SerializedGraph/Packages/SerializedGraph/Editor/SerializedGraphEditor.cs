using UnityEditor;
using UnityEngine;

namespace GiftHorse.SerializedGraphs.Editor
{
    /// <summary>
    /// Custom inspector editor of the classes that inherit from <see cref="SerializedGraphBase"/>.
    /// </summary>
    [CustomEditor(typeof(SerializedGraphBase), true)]
    public class SerializedGraphEditor : UnityEditor.Editor
    {
        private const string k_ContextMenuPath = "CONTEXT/SerializedGraph/Edit Graph";
        private const string k_ButtonText = "Edit Graph";
        private const float k_ButtonsHeight = 25;

        [MenuItem(k_ContextMenuPath)]
        private static void OpenGraphEditorWindow(MenuCommand command)
        {
            SerializedGraphWindow.Open(command.context as SerializedGraphBase);
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            var editGraphButton = GUILayout.Button(k_ButtonText, GUILayout.Height(k_ButtonsHeight));
            if (editGraphButton)
            {
                SerializedGraphWindow.Open(target as SerializedGraphBase);
            }

            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}