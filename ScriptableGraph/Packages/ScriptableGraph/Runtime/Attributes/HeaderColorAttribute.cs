using System;
using UnityEngine;

namespace GiftHorse.SerializedGraphs.Attributes
{
    /// <summary>
    /// Attribute class used to specify the color of the node view header.
    /// </summary>
    public class HeaderColorAttribute : Attribute
    {
        /// <summary>
        /// The header's color.
        /// </summary>
        public Color Color { get; }
        
        /// <summary>
        /// <see cref="HeaderColorAttribute"/>'s constructor.
        /// </summary>
        /// <param name="r"> Red channel of the color. </param>
        /// <param name="g"> Green channel of the color. </param>
        /// <param name="b"> Blue channel of the color. </param>
        public HeaderColorAttribute(float r, float g, float b)
        {
            Color = new Color(r, g, b);
        }
    }
}