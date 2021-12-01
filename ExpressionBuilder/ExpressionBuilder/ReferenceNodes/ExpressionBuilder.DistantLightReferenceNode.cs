///---------------------------------------------------------------------------------------------------------------------
/// <copyright company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>
///---------------------------------------------------------------------------------------------------------------------

namespace ExpressionBuilder
{
    using Microsoft.UI.Composition;

    public sealed class DistantLightReferenceNode : ReferenceNode
    {
        internal DistantLightReferenceNode(string paramName, DistantLight light = null) : base(paramName, light) { }
        
        internal static DistantLightReferenceNode CreateTargetReference()
        {
            var node = new DistantLightReferenceNode(null);
            node._nodeType = ExpressionNodeType.TargetReference;

            return node;
        }

        // Animatable properties
        public ColorNode   Color     { get { return ReferenceProperty<ColorNode>("Color");       } }
        public Vector3Node Direction { get { return ReferenceProperty<Vector3Node>("Direction"); } }
    }
}