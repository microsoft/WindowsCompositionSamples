///---------------------------------------------------------------------------------------------------------------------
/// <copyright company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>
///---------------------------------------------------------------------------------------------------------------------

namespace ExpressionBuilder
{
    using Microsoft.UI.Composition;

    public sealed class SurfaceBrushReferenceNode : ReferenceNode
    {
        internal SurfaceBrushReferenceNode(string paramName, CompositionSurfaceBrush brush = null) : base(paramName, brush) { }
        
        internal static SurfaceBrushReferenceNode CreateTargetReference()
        {
            var node = new SurfaceBrushReferenceNode(null);
            node._nodeType = ExpressionNodeType.TargetReference;

            return node;
        }

        // Animatable properties
        public ScalarNode    HorizontalAlignmentRatio { get { return ReferenceProperty<ScalarNode>("HorizontalAlignmentRatio"); } }
        public ScalarNode    VerticalAlignmentRatio   { get { return ReferenceProperty<ScalarNode>("VerticalAlignmentRatio");   } }

        public ScalarNode    BottomInset              { get { return ReferenceProperty<ScalarNode>("BottomInset");              } }
        public ScalarNode    LeftInset                { get { return ReferenceProperty<ScalarNode>("LeftInset");                } }
        public ScalarNode    RightInset               { get { return ReferenceProperty<ScalarNode>("RightInset");               } }
        public ScalarNode    TopInset                 { get { return ReferenceProperty<ScalarNode>("TopInset");                 } }

        public ScalarNode    RotationAngle            { get { return ReferenceProperty<ScalarNode>("RotationAngle");            } }
        public ScalarNode    RotationAngleInDegrees   { get { return ReferenceProperty<ScalarNode>("RotationAngleInDegrees");   } }

        public Vector2Node   AnchorPoint              { get { return ReferenceProperty<Vector2Node>("AnchorPoint");             } }
        public Vector2Node   CenterPoint              { get { return ReferenceProperty<Vector2Node>("CenterPoint");             } }
        public Vector2Node   Offset                   { get { return ReferenceProperty<Vector2Node>("Offset");                  } }
        public Vector2Node   Scale                    { get { return ReferenceProperty<Vector2Node>("Scale");                   } }

        public Matrix3x2Node TransformMatrix          { get { return ReferenceProperty<Matrix3x2Node>("TransformMatrix");       } }
    }
}