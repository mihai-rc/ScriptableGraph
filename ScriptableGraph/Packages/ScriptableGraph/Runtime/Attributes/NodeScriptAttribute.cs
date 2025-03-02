using System;

namespace GiftHorse.ScriptableGraphs.Attributes
{
    /// <summary>
    /// Attribute class used to specify the node's type path.
    /// </summary>
    public class NodeScriptAttribute : Attribute
    {
        /// <summary>
        /// The path to the node's type in the search window tree.
        /// </summary>
        public string SearchPath { get; set; }

        /// <summary>
        /// <see cref="NodeScriptAttribute"/>'s constructor.
        /// </summary>
        /// <param name="path"> The path to the node's type in the search window tree. </param>
        public NodeScriptAttribute(string path = null)
        {
            SearchPath = path;
        }
    }
}
