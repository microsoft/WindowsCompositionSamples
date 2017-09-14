//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace SamplesCommon
{
    public sealed class PerspectivePanel : UserControl
    {
        Compositor m_compositor;
        Visual m_rootVisual;
        ExpressionAnimation m_matrixExpression;
        bool m_setUpExpressions;

        public PerspectivePanel()
        {
            m_rootVisual = ElementCompositionPreview.GetElementVisual(this as UIElement);
            m_compositor = m_rootVisual.Compositor;

            m_rootVisual.Properties.InsertScalar(PerspectiveDepthProperty, 0);
            m_rootVisual.Properties.InsertVector2(PerspectiveOriginPercentProperty, new Vector2(0.5f, 0.5f));

            this.Loading += OnLoading;
            this.Unloaded += OnUnloaded;
        }

        public const string PerspectiveDepthProperty = nameof(PerspectiveDepth);
        public const string PerspectiveOriginPercentProperty = nameof(PerspectiveOriginPercent);

        public double PerspectiveDepth
        {
            get
            {
                float value = 0;
                m_rootVisual.Properties.TryGetScalar(PerspectiveDepthProperty, out value);
                return value;
            }
            set
            {
                m_rootVisual.Properties.InsertScalar(PerspectiveDepthProperty, (float)value);
                UpdatePerspectiveMatrix();
            }
        }

        public Vector2 PerspectiveOriginPercent
        {
            get
            {
                Vector2 value;
                m_rootVisual.Properties.TryGetVector2(PerspectiveOriginPercentProperty, out value);
                return value;
            }
            set
            {
                m_rootVisual.Properties.InsertVector2(PerspectiveOriginPercentProperty, value);
                UpdatePerspectiveMatrix();
            }
        }

        public CompositionPropertySet VisualProperties
        {
            get
            {
                if (!m_setUpExpressions)
                {
                    m_setUpExpressions = true;
                    UpdatePerspectiveMatrix();
                }
                return m_rootVisual.Properties;
            }
        }

        private void OnLoading(FrameworkElement sender, object args)
        {
            this.SizeChanged += OnSizeChanged;
            OnSizeChanged(this, null);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= OnSizeChanged;
        }


        private void OnSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            if (m_rootVisual != null)
            {
                m_rootVisual.Size = new System.Numerics.Vector2((float)this.ActualWidth, (float)this.ActualHeight);
                if (m_matrixExpression != null)
                {
                    m_matrixExpression.Properties.InsertVector3("LayoutSize", new Vector3(m_rootVisual.Size, 0));
                }
                else
                {
                    UpdatePerspectiveMatrix();
                }
            }
        }

        private void UpdatePerspectiveMatrix()
        {
            if (!m_setUpExpressions)
            {
                Vector3 perspectiveOrigin = new Vector3(PerspectiveOriginPercent * m_rootVisual.Size, 0);

                Matrix4x4 transform =
                    Matrix4x4.CreateTranslation(-perspectiveOrigin) *
                    new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, -1 / (float)PerspectiveDepth, 0, 0, 0, 1) *
                    Matrix4x4.CreateTranslation(perspectiveOrigin);

                m_rootVisual.TransformMatrix = transform;
            }
            else if (m_matrixExpression == null)
            {
                m_matrixExpression = m_compositor.CreateExpressionAnimation();
                m_matrixExpression.Properties.InsertVector3("LayoutSize", new Vector3(m_rootVisual.Size, 0));

                // Expressions don't have an easy way to convert vector2 to vector3. But having this intermediate expression makes the below expression cleaner anyway.
                var perspectiveOriginExpression = m_compositor.CreateExpressionAnimation(
                    "Vector3(publicProps.PerspectiveOriginPercent.x, publicProps.PerspectiveOriginPercent.y, 0) * props.LayoutSize");
                perspectiveOriginExpression.SetReferenceParameter("publicProps", m_rootVisual.Properties);
                perspectiveOriginExpression.SetReferenceParameter("props", m_matrixExpression.Properties);
                m_matrixExpression.Properties.InsertVector3("PerspectiveOrigin", Vector3.Zero);
                m_matrixExpression.Properties.StartAnimation("PerspectiveOrigin", perspectiveOriginExpression);

                m_matrixExpression.Expression =
                    "Matrix4x4.CreateFromTranslation(-props.PerspectiveOrigin) * " +
                    "Matrix4x4(1,0,0,0,  0,1,0,0,  0,0,1,-1/publicProps.PerspectiveDepth,  0,0,0,1) * " +
                    "Matrix4x4.CreateFromTranslation( props.PerspectiveOrigin)";
                m_matrixExpression.SetReferenceParameter("publicProps", m_rootVisual.Properties);
                m_matrixExpression.SetReferenceParameter("props", m_matrixExpression.Properties);

                m_rootVisual.StartAnimation("TransformMatrix", m_matrixExpression);
            }
        }
    }
}
