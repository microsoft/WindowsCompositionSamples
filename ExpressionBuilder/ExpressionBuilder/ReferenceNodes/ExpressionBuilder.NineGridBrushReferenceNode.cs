///---------------------------------------------------------------------------------------------------------------------
/// <copyright company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>
///---------------------------------------------------------------------------------------------------------------------

namespace ExpressionBuilder
{
    using Microsoft.UI.Composition;

    public sealed class NineGridBrushReferenceNode : ReferenceNode
    {
        internal NineGridBrushReferenceNode(string paramName, CompositionNineGridBrush brush = null) : base(paramName, brush) { }
        
        internal static NineGridBrushReferenceNode CreateTargetReference()
        {
            var node = new NineGridBrushReferenceNode(null);
            node._nodeType = ExpressionNodeType.TargetReference;

            return node;
        }
                
        // Animatable properties
        public ScalarNode BottomInset      { get { return ReferenceProperty<ScalarNode>("BottomInset");      } }
        public ScalarNode BottomInsetScale { get { return ReferenceProperty<ScalarNode>("BottomInsetScale"); } }
        public ScalarNode LeftInset        { get { return ReferenceProperty<ScalarNode>("LeftInset");        } }
        public ScalarNode LeftInsetScale   { get { return ReferenceProperty<ScalarNode>("LeftInsetScale");   } }
        public ScalarNode RightInset       { get { return ReferenceProperty<ScalarNode>("RightInset");       } }
        public ScalarNode RightInsetScale  { get { return ReferenceProperty<ScalarNode>("RightInsetScale");  } }
        public ScalarNode TopInset         { get { return ReferenceProperty<ScalarNode>("TopInset");         } }
        public ScalarNode TopInsetScale    { get { return ReferenceProperty<ScalarNode>("TopInsetScale");    } }
    }
}