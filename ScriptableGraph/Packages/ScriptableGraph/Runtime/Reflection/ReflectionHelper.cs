using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using GiftHorse.ScriptableGraphs.Attributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GiftHorse.ScriptableGraphs
{
    public static class ReflectionHelper
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const string k_NotSubTypeOfScriptableNode = "[ScriptableGraph] Cannot process type: {0} because it is not a subtype of ScriptableNode!";
        
        public static bool IsNodeExcludedFromSearch(Type nodeType)
        {
            if (!IsSubclassOfNode(nodeType))
            {
                Debug.LogErrorFormat(k_NotSubTypeOfScriptableNode, nodeType.FullName);
                return false;
            }

            var nodeScriptAttribute = nodeType.GetCustomAttribute<NodeScriptAttribute>();
            return nodeScriptAttribute?.ExcludeFromSearch ?? false;
        }
        
        public static string GetNodeTitleByType(Type nodeType)
        {
            if (!IsSubclassOfNode(nodeType))
            {
                Debug.LogErrorFormat(k_NotSubTypeOfScriptableNode, nodeType.FullName);
                return null;
            }

            return BeautifyTitle(nodeType.Name);
        }

        public static void GetNodePorts(ScriptableNode node, out List<InPort> inPorts, out List<OutPort> outPorts)
        {
            var type = node.GetType();
            var fields = type
                .GetFields(k_BindingFlags)
                .ToList()
                .Where(_ => IsSubclassOfNode(type));

            inPorts = fields
                .Where(field => field
                .GetCustomAttribute<InputAttribute>() is not null)
                .Select((fieldInfo, index) => CreateInPort(fieldInfo, node.Id, index))
                .ToList();

            outPorts = fields
                .Where(field => field
                .GetCustomAttribute<OutputAttribute>() is not null)
                .Select((fieldInfo, index) => CreateOutPort(fieldInfo, node.Id, index))
                .ToList();
        }
        
        private static bool IsSubclassOfNode(Type type)
        {
            if (type.IsSubclassOf(typeof(ScriptableNode))) 
                return true;

            Debug.LogErrorFormat(k_NotSubTypeOfScriptableNode, type.FullName);
            return false;
        }

        private static InPort CreateInPort(FieldInfo fieldInfo, string nodeId, int index) => 
            new(fieldInfo.Name, nodeId, index, fieldInfo.FieldType.AssemblyQualifiedName);

        private static OutPort CreateOutPort(FieldInfo fieldInfo, string nodeId, int index) => 
            new(fieldInfo.Name, nodeId, index, fieldInfo.FieldType.AssemblyQualifiedName);

#if UNITY_EDITOR
        public static List<(Type type, string title, string[] path)> GetNodeSearchEntries(ScriptableGraph graph)
        {
            var nodeBaseType = Type.GetType(graph.NodesBaseType);
            if (!IsSubclassOfNode(nodeBaseType))
            {
                Debug.LogErrorFormat(k_NotSubTypeOfScriptableNode, nodeBaseType?.FullName);
                return null;
            }

            return TypeCache
                .GetTypesDerivedFrom(nodeBaseType)
                .Select(t => (Type: t, NodeScript: t.GetCustomAttribute<NodeScriptAttribute>()))
                .Where(tmd => tmd.NodeScript is not null && IsNotExcludedNode(tmd))
                .Select(tp => (tp.Type, GetNodeTitleByType(tp.Type), tp.NodeScript?.SearchPath?.Split('/')))
                .ToList();
        }

        public static bool TryGetNodeHeaderColor(Type nodeType, out Color color)
        {
            if (!nodeType.IsSubclassOf(typeof(ScriptableNode)))
            {
                Debug.LogErrorFormat(k_NotSubTypeOfScriptableNode, nodeType.FullName);

                color = default;
                return false;
            }

            var colorAttribute = nodeType.GetCustomAttribute<HeaderColorAttribute>();
            if (colorAttribute is null)
            {
                color = default;
                return false;
            }

            color = colorAttribute.Color;
            return true;
        }

        public static IEnumerable<string> GetNodeExposedFieldsNames(Type type)
        {
            if (!IsSubclassOfNode(type))
            {
                return Enumerable.Empty<string>();
            }

            return type
                .GetFields(k_BindingFlags)
                .ToList()
                .Where(fieldInfo => IsSubclassOfNode(type) && fieldInfo
                .GetCustomAttribute<NodeFieldAttribute>() is not null)
                .Select(fieldInfo => fieldInfo.Name);
        }

        private static bool IsNotExcludedNode((Type nodeType, NodeScriptAttribute attribute) nodeMetadata)
        {
            return nodeMetadata.attribute is not null && !nodeMetadata.attribute.ExcludeFromSearch;
        }
        
        private static string BeautifyTitle(string title)
        {
            var titleWithoutSuffix = Regex.Replace(title, "Node$", "");
            var titleWithSpaces = Regex.Replace(titleWithoutSuffix, "([a-z])([A-Z])", "$1 $2");
            
            return titleWithSpaces;
        }
#endif

    }
}