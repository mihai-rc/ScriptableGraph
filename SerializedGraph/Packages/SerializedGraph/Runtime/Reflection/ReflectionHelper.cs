using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using GiftHorse.SerializedGraphs.Attributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GiftHorse.SerializedGraphs
{
    public static class ReflectionHelper
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const string k_NotSubTypeOfSerializedNode = "[SerializedGraph] Cannot process type: {0} because it is not a subtype of SerializedNode!";
        
        /// <summary>
        /// Gets the title of the node based on its type.
        /// </summary>
        /// <param name="nodeType"> The type of the node. </param>
        /// <returns> Returns the formated title. </returns>
        public static string GetNodeTitleByType(Type nodeType)
        {
            if (!IsSubclassOfNode(nodeType))
                return null;

            return BeautifyTitle(nodeType.Name);
        }

        /// <summary>
        /// Gets the input and output ports of a node.
        /// </summary>
        /// <param name="node"> The node whose ports are being fetched. </param>
        /// <param name="inPorts"> Out reference of the list containing all the input ports. </param>
        /// <param name="outPorts"> Out reference of the list containing all the output ports. </param>
        public static void GetNodePorts(SerializedNodeBase node, out List<InPort> inPorts, out List<OutPort> outPorts)
        {
            inPorts = null;
            outPorts = null;
            var type = node.GetType();
            
            if (!IsSubclassOfNode(type)) 
                return;

            var fields = type.GetFields(k_BindingFlags);

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
            if (type.IsSubclassOf(typeof(SerializedNodeBase))) 
                return true;

            Debug.LogErrorFormat(k_NotSubTypeOfSerializedNode, type.FullName);
            return false;
        }

        private static InPort CreateInPort(FieldInfo fieldInfo, string nodeId, int index) => 
            new(fieldInfo.Name, nodeId, index, fieldInfo.FieldType.AssemblyQualifiedName);

        private static OutPort CreateOutPort(FieldInfo fieldInfo, string nodeId, int index) => 
            new(fieldInfo.Name, nodeId, index, fieldInfo.FieldType.AssemblyQualifiedName);

#if UNITY_EDITOR
        /// <summary>
        /// Returns a list of metadata tuples of all the nodes that are derived from the given base type.
        /// </summary>
        /// <param name="nodeBaseTypeName"> The assembly qualified name of the common base type of all nodes belonging to a graph. </param>
        /// <returns> A list of metadata tuples corresponding to each node search entry. </returns>
        public static List<(Type type, string title, string[] path)> GetNodeSearchEntries(string nodeBaseTypeName)
        {
            var nodeBaseType = Type.GetType(nodeBaseTypeName);

            if (!IsSubclassOfNode(nodeBaseType))
                return null;

            return TypeCache
                .GetTypesDerivedFrom(nodeBaseType)
                .Select(t => (Type: t, NodeScript: t.GetCustomAttribute<NodeScriptAttribute>()))
                .Where(tmd => tmd.NodeScript is not null && IsNotExcludedNode(tmd))
                .Select(tp => (tp.Type, GetNodeTitleByType(tp.Type), tp.NodeScript?.SearchPath?.Split('/')))
                .ToList();
        }

        /// <summary>
        /// Checks if the node type should be excluded from the search window based on its <see cref="NodeScriptAttribute"/>.
        /// </summary>
        /// <param name="nodeType"> The type of the node. </param>
        /// <returns> Whether the node type should be excluded or not. </returns>
        public static bool IsNodeExcludedFromSearch(Type nodeType)
        {
            if (!IsSubclassOfNode(nodeType))
                return false;

            var nodeScriptAttribute = nodeType.GetCustomAttribute<NodeScriptAttribute>();
            return nodeScriptAttribute?.ExcludeFromSearch ?? false;
        }

        /// <summary>
        /// Trys to get the header color of a node based on its <see cref="HeaderColorAttribute"/>.
        /// </summary>
        /// <param name="nodeType"> The type of the node. </param>
        /// <param name="color"> Out parameter containing the specified color if set, otherwise has default value. </param>
        /// <returns> Whether a header color was set or not. </returns>
        public static bool TryGetNodeHeaderColor(Type nodeType, out Color color)
        {
            if (!IsSubclassOfNode(nodeType))
            {
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

        /// <summary>
        /// Gets all the exposed fields of a node.
        /// </summary>
        /// <param name="type"> The type of the node whose fields are being fetched. </param>
        /// <returns> A list of the names of all exposed fields. </returns>
        public static IEnumerable<string> GetNodeExposedFieldsNames(Type type)
        {
            if (!IsSubclassOfNode(type))
                return Enumerable.Empty<string>();

            return type
                .GetFields(k_BindingFlags)
                .Where(fieldInfo => fieldInfo
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