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

using System;
using System.Numerics;

using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace SamplesCommon
{
    public class SwipeDismissPanel : UserControl, IInteractionTrackerOwner
    {
        private Compositor m_compositor;
        private Visual m_rootVisual;
        private InteractionTracker m_interactionTracker;
        private VisualInteractionSource m_interactionSource;
        private Visual m_contentVisual;
        private bool m_isInteracting;
        private double m_completedOffset;
        private double m_completionThreshold;
        private InteractionTrackerInertiaModifier[] m_inertiaModifiers;
        private CompositionInteractionSourceCollection m_sourcesWorkaround;
        private bool m_setUpExpressions;
        private ExpressionAnimation m_progressAnimation;

        public SwipeDismissPanel()
        {
            m_rootVisual = ElementCompositionPreview.GetElementVisual(this);
            m_compositor = m_rootVisual.Compositor;

            CompletedOffset = 2000;
            CompletionThreshold = 400;


            m_interactionSource = VisualInteractionSource.Create(m_rootVisual);
            m_interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;

            var snapHomeEndpoint = InteractionTrackerInertiaRestingValue.Create(m_compositor);
            snapHomeEndpoint.Condition = m_compositor.CreateExpressionAnimation("true");
            snapHomeEndpoint.RestingValue = m_compositor.CreateExpressionAnimation("0");

            var snapNearConditionExpression = m_compositor.CreateExpressionAnimation(
                "target.Position.X < -target.CompletionThreshold");
            var snapNearValueExpression = m_compositor.CreateExpressionAnimation("-target.CompletedOffset");

            var snapNearEndpoint = InteractionTrackerInertiaRestingValue.Create(m_compositor);
            snapNearEndpoint.Condition = snapNearConditionExpression;
            snapNearEndpoint.RestingValue = snapNearValueExpression;

            var snapFarConditionExpression = m_compositor.CreateExpressionAnimation(
                "target.Position.X > target.CompletionThreshold");
            var snapFarValueExpression = m_compositor.CreateExpressionAnimation("target.CompletedOffset");

            var snapFarEndpoint = InteractionTrackerInertiaRestingValue.Create(m_compositor);
            snapFarEndpoint.Condition = snapFarConditionExpression;
            snapFarEndpoint.RestingValue = snapFarValueExpression;

            m_inertiaModifiers = new InteractionTrackerInertiaModifier[] {
                snapNearEndpoint,
                snapFarEndpoint,
                snapHomeEndpoint };

            this.Loading += OnLoading;
            this.Unloaded += OnUnloaded;
        }

        public double CompletedOffset
        {
            get
            {
                return m_completedOffset;
            }
            set
            {
                m_completedOffset = value;
                if (m_interactionTracker != null)
                {
                    m_interactionTracker.Properties.InsertScalar(nameof(CompletedOffset), (float)m_completedOffset);
                }
            }
        }


        public double CompletionThreshold
        {
            get
            {
                return m_completionThreshold;
            }
            set
            {
                m_completionThreshold = value;
                if (m_interactionTracker != null)
                {
                    m_interactionTracker.Properties.InsertScalar(nameof(CompletionThreshold), (float)m_completionThreshold);
                }
            }
        }

        public event TypedEventHandler<object, SwipeDismissedArgs> Dismissed;

        public CompositionPropertySet VisualProperties
        {
            get
            {
                if (!m_setUpExpressions)
                {
                    SetUpPropertySetExpressions();
                    m_setUpExpressions = true;
                }
                return m_rootVisual.Properties;
            }
        }

        private void OnLoading(FrameworkElement sender, object args)
        {
            m_interactionTracker = InteractionTracker.CreateWithOwner(m_compositor, this);
            m_sourcesWorkaround = m_interactionTracker.InteractionSources;
            m_sourcesWorkaround.Add(m_interactionSource);
            m_interactionTracker.Properties.InsertScalar(nameof(CompletedOffset), (float)m_completedOffset);
            m_interactionTracker.Properties.InsertScalar(nameof(CompletionThreshold), (float)m_completionThreshold);
            m_interactionTracker.Properties.InsertVector2("Size", Vector2.Zero);
            m_interactionTracker.ConfigurePositionXInertiaModifiers(m_inertiaModifiers);

            this.SizeChanged += OnSizeChanged;
            OnSizeChanged(this, null);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= OnSizeChanged;

            m_sourcesWorkaround.RemoveAll();
            m_interactionTracker.Dispose();
            m_interactionTracker = null;
        }


        private void OnSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            Vector2 size = new System.Numerics.Vector2((float)this.ActualWidth, (float)this.ActualHeight);

            if (m_interactionTracker != null)
            {
                m_interactionTracker.MinPosition = new Vector3(-size.X, 0, 0);
                m_interactionTracker.MaxPosition = new Vector3(size.X, 0, 0);

                if (Content != null)
                {
                    var positionExpression = m_compositor.CreateExpressionAnimation("-tracker.Position");
                    positionExpression.SetReferenceParameter("tracker", m_interactionTracker);

                    m_contentVisual = ElementCompositionPreview.GetElementVisual(Content);
                    m_contentVisual.StartAnimation("Offset", positionExpression);
                    m_contentVisual.Size = size;

                    if (m_setUpExpressions && m_progressAnimation == null)
                    {
                        m_progressAnimation = m_compositor.CreateExpressionAnimation(
                            "clamp(visual.Offset.X / visual.Size.X, -1, 1)");
                        m_progressAnimation.SetReferenceParameter("visual", m_contentVisual);

                        m_rootVisual.Properties.StartAnimation("NormalizedProgress", m_progressAnimation);
                    }
                }
            }
        }

        private void SetUpPropertySetExpressions()
        {
            m_rootVisual.Properties.InsertScalar("NormalizedProgress", 0f);
        }


        void IInteractionTrackerOwner.CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args)
        {

        }

        void IInteractionTrackerOwner.IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
        {
        }

        void IInteractionTrackerOwner.InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
        {
            m_isInteracting = true;
        }

        void IInteractionTrackerOwner.InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
        {
            m_isInteracting = false;
            /*
            if (args.NaturalRestingPosition.X != 0)
            {
                SwipeDismissedArgs dismissedArgs = new SwipeDismissedArgs();
                if (args.NaturalRestingPosition.X < 0)
                {
                    dismissedArgs.DismissedNear = true;
                }
                else
                {
                    dismissedArgs.DismissedFar = true;
                }
                if (Dismissed != null)
                {
                    Dismissed(this, dismissedArgs);
                }
            }
            */
        }

        void IInteractionTrackerOwner.ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
        {
            if (!m_isInteracting && Math.Abs(args.Position.X) > CompletionThreshold)
            {
                SwipeDismissedArgs dismissedArgs = new SwipeDismissedArgs();
                if (args.Position.X < 0)
                {
                    dismissedArgs.DismissedNear = true;
                }
                else
                {
                    dismissedArgs.DismissedFar = true;
                }
                if (Dismissed != null)
                {
                    Dismissed(this, dismissedArgs);
                }
            }
        }

        void IInteractionTrackerOwner.RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args)
        {

        }
    }

    public class SwipeDismissedArgs
    {
        public bool DismissedNear;
        public bool DismissedFar;
    }
}
