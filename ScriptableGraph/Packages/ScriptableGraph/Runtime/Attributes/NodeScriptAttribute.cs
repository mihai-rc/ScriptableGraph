using System;

namespace GiftHorse.ScriptableGraphs.Attributes
{
    /// <summary>
    /// Attribute class used to specify the node's type path.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NodeScriptAttribute : Attribute
    {
        /// <summary>
        /// The path to the node's type in the search window tree.
        /// </summary>
        public string SearchPath { get; set; }
        
        /// <summary>
        /// Whether the node should be excluded from search window or not.
        /// </summary>
        public bool ExcludeFromSearch { get; set; }

        /// <summary>
        /// <see cref="NodeScriptAttribute"/>'s constructor.
        /// </summary>
        /// <param name="path"> The path to the node's type in the search window tree. </param>
        /// <param name="excludeFromSearch"> Whether the node should be excluded from search window or not. </param>
        public NodeScriptAttribute(string path = null, bool excludeFromSearch = false)
        {
            SearchPath = path;
            ExcludeFromSearch = excludeFromSearch;
        }
    }
}
