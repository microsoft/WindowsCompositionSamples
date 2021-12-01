///---------------------------------------------------------------------------------------------------------------------
/// <copyright company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>
///---------------------------------------------------------------------------------------------------------------------

namespace ExpressionBuilder
{
    using Microsoft.UI.Composition;

    public sealed class DropShadowReferenceNode : ReferenceNode
    {
        internal DropShadowReferenceNode(string paramName, DropShadow source = null) : base(paramName, source) { }
        
        internal static DropShadowReferenceNode CreateTargetReference()
        {
            var node = new DropShadowReferenceNode(null);
            node._nodeType = ExpressionNodeType.TargetReference;

            return node;
        }
        
        // Animatable properties
        public ScalarNode  BlurRadius { get { return ReferenceProperty<ScalarNode>("BlurRadius"); } }
        public ScalarNode  Opacity    { get { return ReferenceProperty<ScalarNode>("Opacity");    } }
        public Vector3Node Offset     { get { return ReferenceProperty<Vector3Node>("Offset");    } }
        public ColorNode   Color      { get { return ReferenceProperty<ColorNode>("Color");       } }
    }
}