///---------------------------------------------------------------------------------------------------------------------
/// <copyright company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>
///---------------------------------------------------------------------------------------------------------------------

namespace ExpressionBuilder
{
    using System.Numerics;
    using Windows.UI;

    ///---------------------------------------------------------------------------------------------------------------------
    /// 
    /// class ExpressionValues.Constant
    ///    ToDo: Add description after docs written
    /// 
    ///---------------------------------------------------------------------------------------------------------------------

    // ExpressionValues is a static class instead of a namespace to improve intellisense discoverablity and consistency with the other helper classes.
    public static partial class ExpressionValues
    {        
        /// <summary> Create a constant parameter whose value can be changed without recreating the expression. </summary>
        public static class Constant
        {
            //
            // Constant parameters with no default value
            //
            
            /// <summary> Creates a named constant parameter of type bool. </summary>
            public static BooleanNode    CreateConstantBoolean(string paramName)    { return new BooleanNode(paramName);    }
            
            /// <summary> Creates a named constant parameter of type float. </summary>
            public static ScalarNode     CreateConstantScalar(string paramName)     { return new ScalarNode(paramName);     }
            
            /// <summary> Creates a named constant parameter of type Vector2. </summary>
            public static Vector2Node    CreateConstantVector2(string paramName)    { return new Vector2Node(paramName);    }
            
            /// <summary> Creates a named constant parameter of type Vector3. </summary>
            public static Vector3Node    CreateConstantVector3(string paramName)    { return new Vector3Node(paramName);    }
            
            /// <summary> Creates a named constant parameter of type Vector4. </summary>
            public static Vector4Node    CreateConstantVector4(string paramName)    { return new Vector4Node(paramName);    }
            
            /// <summary> Creates a named constant parameter of type Color. </summary>
            public static ColorNode      CreateConstantColor(string paramName)      { return new ColorNode(paramName);      }
            
            /// <summary> Creates a named constant parameter of type Quaternion. </summary>
            public static QuaternionNode CreateConstantQuaternion(string paramName) { return new QuaternionNode(paramName); }
            
            /// <summary> Creates a named constant parameter of type Matrix3x2. </summary>
            public static Matrix3x2Node  CreateConstantMatrix3x2(string paramName)  { return new Matrix3x2Node(paramName);  }
            
            /// <summary> Creates a named constant parameter of type Matrix4x4. </summary>
            public static Matrix4x4Node  CreateConstantMatrix4x4(string paramName)  { return new Matrix4x4Node(paramName);  }

            
            //
            // Constant parameters with a default value
            //
            
            /// <summary> Creates a named constant parameter of type bool, initialized with the specified value. </summary>
            public static BooleanNode    CreateConstantBoolean(string paramName, bool value)          { return new BooleanNode(paramName, value);    }
            
            /// <summary> Creates a named constant parameter of type float, initialized with the specified value. </summary>
            public static ScalarNode     CreateConstantScalar(string paramName, float value)          { return new ScalarNode(paramName, value);     }
            
            /// <summary> Creates a named constant parameter of type Vector2, initialized with the specified value. </summary>
            public static Vector2Node    CreateConstantVector2(string paramName, Vector2 value)       { return new Vector2Node(paramName, value);    }
            
            /// <summary> Creates a named constant parameter of type Vector3, initialized with the specified value. </summary>
            public static Vector3Node    CreateConstantVector3(string paramName, Vector3 value)       { return new Vector3Node(paramName, value);    }
            
            /// <summary> Creates a named constant parameter of type Vector4, initialized with the specified value. </summary>
            public static Vector4Node    CreateConstantVector4(string paramName, Vector4 value)       { return new Vector4Node(paramName, value);    }
            
            /// <summary> Creates a named constant parameter of type Color, initialized with the specified value. </summary>
            public static ColorNode      CreateConstantColor(string paramName, Color value)           { return new ColorNode(paramName, value);      }
            
            /// <summary> Creates a named constant parameter of type Quaternion, initialized with the specified value. </summary>
            public static QuaternionNode CreateConstantQuaternion(string paramName, Quaternion value) { return new QuaternionNode(paramName, value); }
            
            /// <summary> Creates a named constant parameter of type Matrix3x2, initialized with the specified value. </summary>
            public static Matrix3x2Node  CreateConstantMatrix3x2(string paramName, Matrix3x2 value)   { return new Matrix3x2Node(paramName, value);  }
            
            /// <summary> Creates a named constant parameter of type Matrix4x4, initialized with the specified value. </summary>
            public static Matrix4x4Node  CreateConstantMatrix4x4(string paramName, Matrix4x4 value)   { return new Matrix4x4Node(paramName, value);  }
        }
    }
}