using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine.Pool;

namespace GiftHorse.SerializedGraphs.Editor
{
    /// <summary>
    /// Node search window for creating new nodes in the graph editor.
    /// </summary>
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private struct SearchElement
        {
            public Type Type;
            public string Title;
            public string[] Path;
        }

        private const string k_Header = "Create Node";
        private const string k_NodeIcon = "d_winbtn_win_max@2x";

        private SerializedGraphWindow m_Window;
        private SerializedGraphView m_GraphView;
        private ISerializedGraph m_Graph;

        /// <summary>
        /// Initializes the search window with the context of the graph editor.
        /// </summary>
        /// <param name="context"> Reference to the <see cref="SerializedGraphEditorContext"/> of this graph. </param>
        public void Init(SerializedGraphEditorContext context)
        {
            m_Window = context.Window;
            m_GraphView = context.GraphView;
            m_Graph = context.Graph;
        }

        /// <inheritdoc />
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var elements = ReflectionHelper
                .GetNodeSearchEntries(m_Graph.NodesBaseType)
                .Select(e => new SearchElement
                {
                    Type = e.type,
                    Title = e.title,
                    Path = e.path
                })
                .ToList();

            elements.Sort(CompareElement);
            return PopulateTree(elements);
        }

        /// <inheritdoc />
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowMousePosition = m_Window.rootVisualElement.ChangeCoordinatesTo(m_GraphView, context.screenMousePosition - m_Window.position.position);
            var graphMousePosition = m_GraphView.contentViewContainer.WorldToLocal(windowMousePosition);

            if (searchTreeEntry.userData is not Type type) 
                return false;

            if (Activator.CreateInstance(type) is not ISerializedNode node) 
                return false;

            node.Position = new Rect(graphMousePosition, new Vector2());
            m_GraphView.Add(node);

            return true;
        }

        private int CompareElement(SearchElement first, SearchElement second)
        {
            var firstPath = first.Path;
            var secondPath = second.Path;

            if (firstPath is null && secondPath is null)
                return 0;

            if (firstPath is null)
                return 1;

            if (secondPath is null)
                return -1;

            for (var i = 0; i < firstPath.Length; i++)
            {
                if (i >= secondPath.Length)
                    return 1;

                var value = string.Compare(firstPath[i], secondPath[i], StringComparison.InvariantCulture);

                if (value == 0)
                    continue;

                // Make sure that leaves go before nodes
                if (firstPath.Length != secondPath.Length && (i == firstPath.Length || i == secondPath.Length))
                    return firstPath.Length < secondPath.Length ? 1 : -1;

                return value;
            }

            return 0;
        }

        private List<SearchTreeEntry> PopulateTree(List<SearchElement> elements)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent(k_Header))
            };

            var groupSet = HashSetPool<string>.Get();
            foreach (var element in elements)
            {
                var path = element.Path;
                var groupName = string.Empty;

                if (path is not null)
                {
                    for (var i = 0; i < path.Length; i++)
                    {
                        groupName = $"{groupName}{path[i]}";
                        if (!groupSet.Contains(groupName))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(path[i]), i + 1));
                            groupSet.Add(groupName);
                        }

                        groupName = $"{groupName}/";
                    }
                }

                var length = path?.Length ?? 0;
                var nodeIcon = EditorGUIUtility.IconContent(k_NodeIcon).image;

                var entry = new SearchTreeEntry(new GUIContent(element.Title, nodeIcon));
                entry.userData = element.Type;
                entry.level = length + 1;

                tree.Add(entry);
            }

            HashSetPool<string>.Release(groupSet);
            return tree;
        }
    }
}