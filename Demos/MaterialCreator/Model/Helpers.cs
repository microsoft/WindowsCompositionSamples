//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MaterialCreator
{
    public class PropertyData
    {
        public PropertyData(object key, object obj, object target)
        {
            Key = key;
            Object = obj;
            Target = target;
        }

        public object Key { get; set; }
        public object Object { get; set; }
        public object Target { get; set; }
    }

    class Helpers
    {
        public static List<string> GetBlendModes()
        {
            string modeName;
            List<string> blendModes = new List<string>(30);

            foreach (BlendEffectMode mode in Enum.GetValues(typeof(BlendEffectMode)))
            {
                switch (mode)
                {
                    case BlendEffectMode.Dissolve:
                        // Not supported, skip
                        break;

                    default:
                        modeName = mode.ToString();
                        Debug.Assert(!blendModes.Contains(modeName));
                        blendModes.Add(modeName);
                        break;
                }
            }

            foreach (CanvasComposite mode in Enum.GetValues(typeof(CanvasComposite)))
            {
                switch (mode)
                {
                    case CanvasComposite.BoundedCopy:
                        // Not supported, skip
                        break;

                    default:
                        modeName = mode.ToString();
                        Debug.Assert(!blendModes.Contains(modeName));
                        blendModes.Add(modeName);
                        break;
                }
            }

            modeName = "Normal";
            Debug.Assert(!blendModes.Contains(modeName));
            blendModes.Add(modeName);
            
            // Sort the list
            blendModes.Sort();

            return blendModes;
        }
        
        public static IGraphicsEffect GetEffectFromBlendMode(string mode)
        {
            if (mode.ToLower() == "normal")
            {
                return new CompositeEffect()
                {
                    Mode = CanvasComposite.SourceOver,
                };
            }
            else if (mode.ToLower() == "arithmetic")
            {
                return new ArithmeticCompositeEffect()
                {
                    Source1Amount = .5f,
                    Source2Amount = .5f,
                };
            }
            else
            {
                BlendEffectMode blendMode;
                if (Enum.TryParse<BlendEffectMode>(mode, out blendMode))
                {
                    return new BlendEffect()
                    {
                        Mode = blendMode,
                    };
                }

                CanvasComposite compositeMode;
                if (Enum.TryParse<CanvasComposite>(mode, out compositeMode))
                {
                    return new CompositeEffect()
                    {
                        Mode = compositeMode,
                    };
                }
            }

            return null;
        }

        public static bool SkipProperty(string propertyName)
        {
            if (propertyName.ToLower() == "name" ||
                propertyName.ToLower() == "cacheoutput" ||
                propertyName.ToLower() == "bufferprecision" ||
                propertyName.ToLower() == "colorhdr" ||
                propertyName.ToLower() == "issupported" ||
                propertyName.ToLower() == "clampoutput" ||
                propertyName.ToLower() == "alphamode" ||
                propertyName.ToLower() == "source" ||
                // The following are for lights
                propertyName.ToLower() == "coordinatespace" ||
                propertyName.ToLower() == "targets" ||
                propertyName.ToLower() == "exclusions" ||
                propertyName.ToLower() == "compositor" ||
                propertyName.ToLower() == "dispatcher" ||
                propertyName.ToLower() == "properties" ||
                propertyName.ToLower() == "comment")
            {
                return true;
            }

            return false;
        }

        static public void AddPropertyToPanel(object propertyObject, object targetObject, StackPanel panel, PropertyInfo info)
        {
            TextBlock textName = new TextBlock();
            textName.Text = info.Name;
            textName.Margin = textName.Margin = new Thickness(0, 20, 0, 5);
            panel.Children.Add(textName);

            FrameworkElement element;
            if (info.PropertyType == typeof(float))
            {
                TextBox box = new TextBox();
                box.Text = ((float)info.GetValue(propertyObject)).ToString();
                box.Tag = new PropertyData(info.Name, propertyObject, targetObject);
                box.TextChanged += PropertyChanged;
                element = box;
            }
            else if (info.PropertyType == typeof(bool))
            {
                ComboBox box = new ComboBox();
                box.Tag = new PropertyData(info.Name, propertyObject, targetObject);
                box.SelectionChanged += PropertyChanged;
                box.SelectedValuePath = "Content";

                ComboBoxItem item = new ComboBoxItem();
                item.Content = "True";
                item.Tag = new PropertyData(true, propertyObject, targetObject);
                box.Items.Add(item);

                item = new ComboBoxItem();
                item.Content = "False";
                item.Tag = new PropertyData(false, propertyObject, targetObject);
                box.Items.Add(item);

                box.SelectedValue = ((bool)info.GetValue(propertyObject)).ToString();
                element = box;
            }
            else if (typeof(Enum).IsAssignableFrom(info.PropertyType))
            {
                ComboBox box = new ComboBox();
                box.Tag = new PropertyData(info.Name, propertyObject, targetObject);
                box.SelectionChanged += PropertyChanged;
                box.SelectedValuePath = "Content";

                foreach (object o in Enum.GetValues(info.PropertyType))
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = o.ToString();
                    item.Tag = new PropertyData(o, propertyObject, targetObject); ;
                    box.Items.Add(item);
                }

                box.SelectedValue = info.GetValue(propertyObject).ToString();
                element = box;
            }
            else if (info.PropertyType == typeof(Color))
            {
                ColorPicker mixer = new ColorPicker();
                mixer.ColorChanged += PropertyChanged;
                mixer.Tag = new PropertyData(info.Name, propertyObject, targetObject);
                mixer.Color = (Color)info.GetValue(propertyObject);
                element = mixer;
            }
            else if (info.PropertyType == typeof(Matrix5x4))
            {
                Matrix5x4Control mat = new Matrix5x4Control();
                mat.MatrixChanged += PropertyChanged;
                mat.Tag = new PropertyData(info.Name, propertyObject, targetObject);
                mat.Matrix = (Matrix5x4)info.GetValue(propertyObject);
                element = mat;
            }
            else if (info.PropertyType == typeof(Vector2))
            {
                VectorControl vec = new VectorControl(2);
                vec.VectorChanged += PropertyChanged;
                vec.Tag = new PropertyData(info.Name, propertyObject, targetObject);
                vec.Vector2 = (Vector2)info.GetValue(propertyObject);
                element = vec;
            }
            else if (info.PropertyType == typeof(Vector3))
            {
                VectorControl vec = new VectorControl(3);
                vec.VectorChanged += PropertyChanged;
                vec.Tag = new PropertyData(info.Name, propertyObject, targetObject);
                vec.Vector3 = (Vector3)info.GetValue(propertyObject);
                element = vec;
            }
            else if (info.PropertyType == typeof(Vector4))
            {
                VectorControl vec = new VectorControl(4);
                vec.VectorChanged += PropertyChanged;
                vec.Tag = new PropertyData(info.Name, propertyObject, targetObject);
                vec.Vector4 = (Vector4)info.GetValue(propertyObject);
                element = vec;
            }
            else
            {
                textName = new TextBlock();
                object value = info.GetValue(propertyObject);
                textName.Text = value == null ? "" : value.ToString();
                element = textName;
            }

            panel.Children.Add(element);
        }


        private static void PropertyChanged(object sender, object args)
        {
            PropertyData data = null;

            if (sender.GetType() == typeof(ColorPicker))
            {
                ColorPicker mixer = (ColorPicker)sender;
                data = (PropertyData)mixer.Tag;
                string name = (string)data.Key;

                MethodInfo method = data.Target.GetType().GetMethod("SetProperty");
                object result = method.Invoke(data.Target, new object[] { name, mixer.Color });
            }
            else if (sender.GetType() == typeof(ComboBox))
            {
                ComboBox box = (ComboBox)sender;
                data = (PropertyData)box.Tag;
                PropertyData itemData = (PropertyData)((ComboBoxItem)box.SelectedItem).Tag;
                string name = (string)data.Key;

                MethodInfo method = data.Target.GetType().GetMethod("SetProperty");
                object result = method.Invoke(data.Target, new object[] { name, itemData.Key });
            }
            else if (sender.GetType() == typeof(TextBox))
            {
                TextBox box = (TextBox)sender;
                data = (PropertyData)box.Tag;
                string name = (string)data.Key;

                if (box.Text.Length > 0)
                {
                    try
                    {
                        MethodInfo method = data.Target.GetType().GetMethod("SetProperty");
                        object result = method.Invoke(data.Target, new object[] { name, Convert.ToSingle(box.Text) });
                    }
                    catch
                    {
                        // Ignore, invalid conversion or property range
                    }
                }
            }
            else if (sender.GetType() == typeof(Matrix5x4Control))
            {
                Matrix5x4Control mat = (Matrix5x4Control)sender;
                data = (PropertyData)mat.Tag;
                string name = (string)data.Key;

                MethodInfo method = data.Target.GetType().GetMethod("SetProperty");
                object result = method.Invoke(data.Target, new object[] { name, mat.Matrix });
            }
            else if (sender.GetType() == typeof(VectorControl))
            {
                VectorControl vec = (VectorControl)sender;
                data = (PropertyData)vec.Tag;
                string name = (string)data.Key;

                MethodInfo method = data.Target.GetType().GetMethod("SetProperty");
                switch (vec.Components)
                {
                    case 2:
                        method.Invoke(data.Target, new object[] { name, vec.Vector2 });
                        break;
                    case 3:
                        method.Invoke(data.Target, new object[] { name, vec.Vector3 });
                        break;
                    case 4:
                        method.Invoke(data.Target, new object[] { name, vec.Vector4 });
                        break;
                }
            }
            else
            {
                Debug.Assert(false);
            }
        }

        private static string Matrix5x4ToString(Matrix5x4 matrix)
        {
            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},",
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44,
                matrix.M51, matrix.M52, matrix.M53, matrix.M54);
        }

        private static Matrix5x4 Matrix5x4FromString(string matrix)
        {
            string[] parts = matrix.Split(',');

            Matrix5x4 mat = new Matrix5x4();
            mat.M11 = float.Parse(parts[0]);
            mat.M12 = float.Parse(parts[1]);
            mat.M13 = float.Parse(parts[2]);
            mat.M14 = float.Parse(parts[3]);
            mat.M21 = float.Parse(parts[4]);
            mat.M22 = float.Parse(parts[5]);
            mat.M23 = float.Parse(parts[6]);
            mat.M24 = float.Parse(parts[7]);
            mat.M31 = float.Parse(parts[8]);
            mat.M32 = float.Parse(parts[9]);
            mat.M33 = float.Parse(parts[10]);
            mat.M34 = float.Parse(parts[11]);
            mat.M41 = float.Parse(parts[12]);
            mat.M42 = float.Parse(parts[13]);
            mat.M43 = float.Parse(parts[14]);
            mat.M44 = float.Parse(parts[15]);
            mat.M51 = float.Parse(parts[16]);
            mat.M52 = float.Parse(parts[17]);
            mat.M53 = float.Parse(parts[18]);
            mat.M54 = float.Parse(parts[19]);

            return mat;
        }

        private static string Vector2ToString(Vector2 vector)
        {
            return String.Format("{0},{1}", vector.X, vector.Y);
        }

        private static Vector2 Vector2FromString(string matrix)
        {
            string[] parts = matrix.Split(',');

            Vector2 vector = new Vector2();
            vector.X = float.Parse(parts[0]);
            vector.Y = float.Parse(parts[1]);
            return vector;
        }

        private static string Vector3ToString(Vector3 vector)
        {
            return String.Format("{0},{1},{2}", vector.X, vector.Y, vector.Z);
        }

        private static Vector3 Vector3FromString(string matrix)
        {
            string[] parts = matrix.Split(',');

            Vector3 vector = new Vector3();
            vector.X = float.Parse(parts[0]);
            vector.Y = float.Parse(parts[1]);
            vector.Z = float.Parse(parts[2]);
            return vector;
        }

        private static string Vector4ToString(Vector4 vector)
        {
            return String.Format("{0},{1},{2},{3}", vector.X, vector.Y, vector.Z, vector.W);
        }

        private static Vector4 Vector4FromString(string matrix)
        {
            string[] parts = matrix.Split(',');

            Vector4 vector = new Vector4();
            vector.X = float.Parse(parts[0]);
            vector.Y = float.Parse(parts[1]);
            vector.Z = float.Parse(parts[2]);
            vector.W = float.Parse(parts[3]);
            return vector;
        }

        public static string ToString(object obj)
        {
            string s;
            Type type = obj.GetType();

            if (type == typeof(Matrix5x4))
            {
                s = Matrix5x4ToString((Matrix5x4)obj);
            }
            else if (type == typeof(Vector2))
            {
                s = Vector2ToString((Vector2)obj);
            }
            else if (type == typeof(Vector3))
            {
                s = Vector3ToString((Vector3)obj);
            }
            else if (type == typeof(Vector4))
            {
                s = Vector4ToString((Vector4)obj);
            }
            else if (type == typeof(CompositionEffectSourceParameter))
            {
                s = "CompositionEffectSourceParameter";
            }
            else
            {
                s = obj.ToString();
            }

            return s;
        }

        public static object FromString(Type type, string value)
        {
            object obj;

            if (typeof(Enum).IsAssignableFrom(type))
            {
                obj = Enum.Parse(type, value);
            }
            else if (type == typeof(float))
            {
                obj = float.Parse(value);
            }
            else if (type == typeof(bool))
            {
                obj = bool.Parse(value);
            }
            else if (type == typeof(Color))
            {
                string color = value;
                byte a = byte.Parse(color.Substring(1, 2), NumberStyles.HexNumber);
                byte r = byte.Parse(color.Substring(3, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(color.Substring(5, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(color.Substring(7, 2), NumberStyles.HexNumber);

                obj = Color.FromArgb(a, r, g, b);
            }
            else if (type == typeof(Vector2))
            {
                obj = Helpers.Vector2FromString(value);
            }
            else if (type == typeof(Vector3))
            {
                obj = Helpers.Vector3FromString(value);
            }
            else if (type == typeof(Vector4))
            {
                obj = Helpers.Vector4FromString(value);
            }
            else if (type == typeof(Matrix5x4))
            {
                obj = Helpers.Matrix5x4FromString(value);
            }
            else
            {
                Debug.Assert(false);
                obj = null;
            }

            return obj;
        }
     }
}
