///---------------------------------------------------------------------------------------------------------------------
/// <copyright company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>
///---------------------------------------------------------------------------------------------------------------------

namespace ExpressionBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    using Windows.UI;

    using Microsoft.UI.Composition;

    internal enum ExpressionNodeType
    {
        ConstantValue,
        ConstantParameter,
        CurrentValueProperty,
        Reference,
        ReferenceProperty,
        StartingValueProperty,
        TargetReference,
        Conditional,
        Swizzle,
        Add,
        And,
        Divide,
        Equals,
        GreaterThan,
        GreaterThanEquals,
        LessThan,
        LessThanEquals,
        Multiply,
        Not,
        NotEquals,
        Or,
        Subtract,
        Absolute,
        Acos,
        Asin,
        Atan,
        Cos,
        Ceil,
        Clamp,
        ColorHsl,
        ColorRgb,
        ColorLerp,
        ColorLerpHsl,
        ColorLerpRgb,
        Concatenate,
        Distance,
        DistanceSquared,
        Floor,
        Inverse,
        Length,
        LengthSquared,
        Lerp,
        Ln,
        Log10,
        Max,
        Matrix3x2FromRotation,
        Matrix3x2FromScale,
        Matrix3x2FromSkew,
        Matrix3x2FromTranslation,
        Matrix3x2,
        Matrix4x4FromAxisAngle,
        Matrix4x4FromScale,
        Matrix4x4FromTranslation,
        Matrix4x4,
        Min,
        Modulus,
        Negate,
        Normalize,
        Pow,
        QuaternionFromAxisAngle,
        Quaternion,
        Round,
        Scale,
        Sin,
        Slerp,
        Sqrt,
        Square,
        Tan,
        ToDegrees,
        ToRadians,
        Transform,
        Vector2,
        Vector3,
        Vector4,

        Count
    }

    internal enum ValueKeywordKind
    {
        CurrentValue,
        StartingValue,
    }

    ///---------------------------------------------------------------------------------------------------------------------
    /// 
    /// class ExpressionNode
    ///    ToDo: Add description after docs written
    /// 
    ///---------------------------------------------------------------------------------------------------------------------

    public abstract class ExpressionNode
    {
        internal ExpressionNode() { }

        /// <summary> Resolve a named reference parameter to the CompositionObject it will refer to. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="compObj">The composition object that the parameter should resolve to.</param>
        public void SetReferenceParameter(string parameterName, CompositionObject compObj)
        {
            // Make sure we have our reference list populated
            EnsureReferenceInfo();

            for (int i = 0; i < _objRefList.Count; i++)
            {
                if (string.Compare(_objRefList[i].ParameterName, parameterName, true /*ignoreCase*/) == 0)
                {
                    var item = _objRefList[i];
                    item.CompObject = compObj;
                    _objRefList[i] = item;
                }
            }
        }

        /// <summary> Resolve a named parameter to the boolean value it will use. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="value">The value that the parameter should resolve to.</param>
        public void SetBooleanParameter(string parameterName, bool value)          { _constParamMap[parameterName] = value; }

        /// <summary> Resolve a named parameter to the float value it will use. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="value">The value that the parameter should resolve to.</param>
        public void SetScalarParameter(string parameterName, float value)          { _constParamMap[parameterName] = value; }

        /// <summary> Resolve a named parameter to the Vector2 value it will use. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="value">The value that the parameter should resolve to.</param>
        public void SetVector2Parameter(string parameterName, Vector2 value)       { _constParamMap[parameterName] = value; }

        /// <summary> Resolve a named parameter to the Vector3 value it will use. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="value">The value that the parameter should resolve to.</param>
        public void SetVector3Parameter(string parameterName, Vector3 value)       { _constParamMap[parameterName] = value; }

        /// <summary> Resolve a named parameter to the Vector4 value it will use. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="value">The value that the parameter should resolve to.</param>
        public void SetVector4Parameter(string parameterName, Vector4 value)       { _constParamMap[parameterName] = value; }

        /// <summary> Resolve a named parameter to the Color value it will use. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="value">The value that the parameter should resolve to.</param>
        public void SetColorParameter(string parameterName, Color value)           { _constParamMap[parameterName] = value; }

        /// <summary> Resolve a named parameter to the Quaternion value it will use. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="value">The value that the parameter should resolve to.</param>
        public void SetQuaternionParameter(string parameterName, Quaternion value) { _constParamMap[parameterName] = value; }

        /// <summary> Resolve a named parameter to the Matrix3x2 value it will use. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="value">The value that the parameter should resolve to.</param>
        public void SetMatrix3x2Parameter(string parameterName, Matrix3x2 value)   { _constParamMap[parameterName] = value; }

        /// <summary> Resolve a named parameter to the Matrix4x4 value it will use. </summary>
        /// <param name="parameterName">The string name of the parameter to be resolved.</param>
        /// <param name="value">The value that the parameter should resolve to.</param>
        public void SetMatrix4x4Parameter(string parameterName, Matrix4x4 value)   { _constParamMap[parameterName] = value; }
        
        /// <summary> Releases all resources used by this ExpressionNode. </summary>
        public void Dispose()
        {
            _objRefList = null;
            _compObjToParamNameMap = null;
            _constParamMap = null;
            _subchannels = null;
            _propertyName = null;
            _nodeType = ExpressionNodeType.Count;

            // Note: we don't recursively dispose all child nodes, as those nodes could be in use by a different Expression
            _children = null;
            
            if (_expressionAnimation != null)
            {
                _expressionAnimation.Dispose();
                _expressionAnimation = null;
            }
        }

        //
        // Helper functions
        //
        
        internal static T CreateExpressionNode<T>() where T : class
        {
            T newNode;

            if (typeof(T) == typeof(BooleanNode))
            {
                newNode = new BooleanNode() as T;
            }
            else if (typeof(T) == typeof(ScalarNode))
            {
                newNode = new ScalarNode() as T;
            }
            else if (typeof(T) == typeof(Vector2Node))
            {
                newNode = new Vector2Node() as T;
            }
            else if (typeof(T) == typeof(Vector3Node))
            {
                newNode = new Vector3Node() as T;
            }
            else if (typeof(T) == typeof(Vector4Node))
            {
                newNode = new Vector4Node() as T;
            }
            else if (typeof(T) == typeof(ColorNode))
            {
                newNode = new ColorNode() as T;
            }
            else if (typeof(T) == typeof(QuaternionNode))
            {
                newNode = new QuaternionNode() as T;
            }
            else if (typeof(T) == typeof(Matrix3x2Node))
            {
                newNode = new Matrix3x2Node() as T;
            }
            else if (typeof(T) == typeof(Matrix4x4Node))
            {
                newNode = new Matrix4x4Node() as T;
            }
            else
            {
                throw new Exception("unexpected type");
            }

            return newNode;
        }

        internal string ToExpressionString()
        {
            if (_objRefList == null)
            {
                EnsureReferenceInfo();
            }

            return ToExpressionStringInternal();
        }

        internal void EnsureReferenceInfo()
        {
            if (_objRefList == null)
            {
                // Get all ReferenceNodes in this expression
                HashSet<ReferenceNode> referenceNodes = new HashSet<ReferenceNode>();
                PopulateParameterNodes(ref _constParamMap, ref referenceNodes);

                // Find all CompositionObjects across all referenceNodes that need a paramName to be created
                HashSet<CompositionObject> compObjects = new HashSet<CompositionObject>();
                foreach (var refNode in referenceNodes)
                {
                    if ((refNode.Reference != null) && (refNode.GetReferenceParamString() == null))
                    {
                        compObjects.Add(refNode.Reference);
                    }
                }

                // Create a map to store the generated paramNames for each CompObj
                uint id = 0;
                _compObjToParamNameMap = new Dictionary<CompositionObject, string>();
                foreach (var compObj in compObjects)
                {
                    // compObj.ToString() will return something like "Microsoft.UI.Composition.SpriteVisual"
                    // Make it look like "SpriteVisual_1"
                    string paramName = compObj.ToString();
                    paramName = $"{paramName.Substring(paramName.LastIndexOf('.') + 1)}_{++id}";       // make sure the created param name doesn't overwrite a custom name

                    _compObjToParamNameMap.Add(compObj, paramName);
                }

                // Go through all reference nodes again to create our full list of referenceInfo. This time, if 
                // the param name is null, look it up from our new map and store it.
                _objRefList = new List<ReferenceInfo>();
                foreach (var refNode in referenceNodes)
                {
                    string paramName = refNode.GetReferenceParamString();

                    if ((refNode.Reference == null) && (paramName == null))
                    {
                        // This can't happen - if the ref is null it must be because it's a named param
                        throw new Exception("Reference and paramName can't both be null");
                    }

                    if (paramName == null)
                    {
                        paramName = _compObjToParamNameMap[refNode.Reference];
                    }

                    _objRefList.Add(new ReferenceInfo(paramName, refNode.Reference));
                    refNode._paramName = paramName;
                }
            }
        }

        internal void SetAllParameters(CompositionAnimation anim)
        {
            // Make sure the list is populated
            EnsureReferenceInfo();

            foreach (var refInfo in _objRefList)
            {
                anim.SetReferenceParameter(refInfo.ParameterName, refInfo.CompObject);
            }

            foreach (var constParam in _constParamMap)
            {
                if (constParam.Value.GetType() == typeof(bool))
                {
                    anim.SetBooleanParameter(constParam.Key, (bool)constParam.Value);
                }
                else if (constParam.Value.GetType() == typeof(float))
                {
                    anim.SetScalarParameter(constParam.Key, (float)constParam.Value);
                }
                else if (constParam.Value.GetType() == typeof(Vector2))
                {
                    anim.SetVector2Parameter(constParam.Key, (Vector2)constParam.Value);
                }
                else if (constParam.Value.GetType() == typeof(Vector3))
                {
                    anim.SetVector3Parameter(constParam.Key, (Vector3)constParam.Value);
                }
                else if (constParam.Value.GetType() == typeof(Vector4))
                {
                    anim.SetVector4Parameter(constParam.Key, (Vector4)constParam.Value);
                }
                else if (constParam.Value.GetType() == typeof(Color))
                {
                    anim.SetColorParameter(constParam.Key, (Color)constParam.Value);
                }
                else if (constParam.Value.GetType() == typeof(Quaternion))
                {
                    anim.SetQuaternionParameter(constParam.Key, (Quaternion)constParam.Value);
                }
                else if (constParam.Value.GetType() == typeof(Matrix3x2))
                {
                    anim.SetMatrix3x2Parameter(constParam.Key, (Matrix3x2)constParam.Value);
                }
                else if (constParam.Value.GetType() == typeof(Matrix4x4))
                {
                    anim.SetMatrix4x4Parameter(constParam.Key, (Matrix4x4)constParam.Value);
                }
                else
                {
                    throw new Exception($"Unexpected constant parameter datatype ({constParam.Value.GetType()})");
                }
            }
        }

        internal static T CreateValueKeyword<T>(ValueKeywordKind keywordKind) where T : class
        {
            T node = CreateExpressionNode<T>();

            (node as ExpressionNode)._paramName = null;

            switch (keywordKind)
            {
                case ValueKeywordKind.CurrentValue:
                    (node as ExpressionNode)._nodeType = ExpressionNodeType.CurrentValueProperty;
                    break;

                case ValueKeywordKind.StartingValue:
                    (node as ExpressionNode)._nodeType = ExpressionNodeType.StartingValueProperty;
                    break;

                default:
                    throw new Exception("Invalid ValueKeywordKind");
            }

            return node;
        }

        internal protected abstract string GetValue();
        
        internal protected T SubchannelsInternal<T>(params string[] subchannels) where T : class
        {
            ExpressionNodeType swizzleNodeType = ExpressionNodeType.Swizzle;
            T newNode;

            switch (subchannels.GetLength(0))
            {
                case 1:
                    newNode = ExpressionFunctions.Function<ScalarNode>(swizzleNodeType, this) as T;
                    break;

                case 2:
                    newNode = ExpressionFunctions.Function<Vector2Node>(swizzleNodeType, this) as T;
                    break;

                case 3:
                    newNode = ExpressionFunctions.Function<Vector3Node>(swizzleNodeType, this) as T;
                    break;

                case 4:
                    newNode = ExpressionFunctions.Function<Vector4Node>(swizzleNodeType, this) as T;
                    break;

                case 6:
                    newNode = ExpressionFunctions.Function<Matrix3x2Node>(swizzleNodeType, this) as T;
                    break;

                case 16:
                    newNode = ExpressionFunctions.Function<Matrix4x4Node>(swizzleNodeType, this) as T;
                    break;

                default:
                    throw new Exception($"Invalid subchannel count ({subchannels.GetLength(0)})");
            }

            (newNode as ExpressionNode)._subchannels = subchannels;

            return newNode;
        }

        internal protected void PopulateParameterNodes(ref Dictionary<string, object> constParamMap, ref HashSet<ReferenceNode> referenceNodes)
        {
            var refNode = (this as ReferenceNode);
            if ((refNode != null) && (refNode._nodeType != ExpressionNodeType.TargetReference))
            {
                referenceNodes.Add(refNode);
            }

            if ((_constParamMap != null) && (_constParamMap != constParamMap))
            {
                foreach (var entry in _constParamMap)
                {
                    // If this parameter hasn't already been set on the root, use this node's parameter info
                    if (!constParamMap.ContainsKey(entry.Key))
                    {
                        constParamMap[entry.Key] = entry.Value;
                    }
                }
            }

            foreach (var child in _children)
            {
                child.PopulateParameterNodes(ref constParamMap, ref referenceNodes);
            }
        }
        

        private OperationType GetOperationKind()   { return ExpressionFunctions.GetNodeInfoFromType(_nodeType).NodeOperationKind; }
        private string        GetOperationString() { return ExpressionFunctions.GetNodeInfoFromType(_nodeType).OperationString;   }

        
        private string ToExpressionStringInternal()
        {
            string ret = "";

            // Do a recursive depth-first traversal of the node tree to print out the full expression string
            switch (GetOperationKind())
            {
                case OperationType.Function:
                    if (_children.Count == 0)
                    {
                        throw new Exception("Can't have an expression function with no params");
                    }

                    ret = $"{GetOperationString()}({_children[0].ToExpressionStringInternal()}";
                    for (int i = 1; i < _children.Count; i++)
                    {
                        ret += "," + _children[i].ToExpressionStringInternal();
                    }
                    ret += ")";
                    break;

                case OperationType.Operator:
                    if (_children.Count != 2)
                    {
                        throw new Exception("Can't have an operator that doesn't have 2 exactly params");
                    }
                    
                    ret = $"({_children[0].ToExpressionStringInternal()} {GetOperationString()} {_children[1].ToExpressionStringInternal()})";
                    break;

                case OperationType.Constant:
                    if (_children.Count == 0)
                    {
                        // If a parameterName was specified, use it. Otherwise write the value.
                        ret = _paramName ?? GetValue();
                    }
                    else
                    {
                        throw new Exception("Constants must have 0 children");
                    }
                    break;

                case OperationType.Swizzle:
                    if (_children.Count != 1)
                    {
                        throw new Exception("Swizzles should have exactly 1 child");
                    }

                    string swizzleString = "";
                    foreach (var sub in _subchannels)
                    {
                        swizzleString += sub;
                    }

                    ret = $"{_children[0].ToExpressionStringInternal()}.{swizzleString}";
                    break;

                case OperationType.Reference:
                    if ((_nodeType == ExpressionNodeType.Reference) ||
                        (_nodeType == ExpressionNodeType.TargetReference))
                    {
                        // This is the reference node itself
                        if (_children.Count != 0)
                        {
                            throw new Exception("References cannot have children");
                        }

                        ret = (this as ReferenceNode).GetReferenceParamString();
                    }
                    else if (_nodeType == ExpressionNodeType.ReferenceProperty)
                    {
                        // This is the property node of the reference
                        if (_children.Count != 1)
                        {
                            throw new Exception("Reference properties must have exactly one child");
                        }

                        if (_propertyName == null)
                        {
                            throw new Exception("Reference properties must have a property name");
                        }

                        ret = $"{_children[0].ToExpressionStringInternal()}.{_propertyName}";
                    }
                    else if (_nodeType == ExpressionNodeType.StartingValueProperty)
                    {
                        // This is a "this.StartingValue" node
                        if (_children.Count != 0)
                        {
                            throw new Exception("StartingValue references Cannot have children");
                        }

                        ret = "this.StartingValue";
                    }
                    else if (_nodeType == ExpressionNodeType.CurrentValueProperty)
                    {
                        // This is a "this.CurrentValue" node
                        if (_children.Count != 0)
                        {
                            throw new Exception("CurrentValue references Cannot have children");
                        }

                        ret = "this.CurrentValue";
                    }
                    else
                    {
                        throw new Exception("Unexpected NodeType for OperationType.Reference");
                    }
                    break;

                case OperationType.Conditional:
                    if (_children.Count != 3)
                    {
                        throw new Exception("Conditionals must have exactly 3 children");
                    }

                    ret = $"(({_children[0].ToExpressionStringInternal()}) ? ({_children[1].ToExpressionStringInternal()}) : ({_children[2].ToExpressionStringInternal()}))";
                    break;

                default:
                    throw new Exception($"Unexpected operation type ({GetOperationKind()}), nodeType = {_nodeType}");
            }

            return ret;
        }

        //
        // Structs
        //
        
        internal struct ReferenceInfo
        {
            public ReferenceInfo(string paramName, CompositionObject compObj)
            {
                ParameterName = paramName;
                CompObject = compObj;
            }

            public string ParameterName;
            public CompositionObject CompObject;
        }

        
        //
        // Data
        //
        
        private List<ReferenceInfo> _objRefList = null;
        private Dictionary<CompositionObject, string> _compObjToParamNameMap = null;
        private Dictionary<string, object> _constParamMap = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

        internal protected string[] _subchannels = null;
        internal string _propertyName = null;

        internal ExpressionNodeType _nodeType;
        internal List<ExpressionNode> _children = new List<ExpressionNode>();
        internal string _paramName = null;
        
        internal ExpressionAnimation _expressionAnimation = null;
    }
}