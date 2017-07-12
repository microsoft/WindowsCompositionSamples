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

using CompositionSampleGallery.Shared;
using SamplesCommon;
using System.Diagnostics;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;   
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.System;


namespace CompositionSampleGallery
{
    public sealed partial class PullToRefresh : SamplePage
    {
        private Visual _contentPanelVisual;
        private Visual _root;
        private Compositor _compositor;
        private VisualInteractionSource _interactionSource;
        private InteractionTracker _tracker;
        private Windows.UI.Core.CoreWindow _Window;
        private static Size ControlSize = new Size(500,500);
        private ExpressionAnimation m_positionExpression;
        

        public PullToRefresh()
        {   
            Model = new LocalDataSource();
            this.InitializeComponent();
            _Window = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow;
        }

     
        public static string       StaticSampleName     { get { return "PullToRefresh ListView Items"; } }
        public override string     SampleName           { get { return StaticSampleName; } }
        public override string     SampleDescription    { get { return "Demonstrates how to apply a parallax effect to each item in a ListView control. As you scroll the ListView control watch as each ListView item translates at a different rate in comparison to the ListView's scroll position."; } }
        public override string     SampleCodeUri        { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761169"; } }

        public LocalDataSource Model { set; get; }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ThumbnailList.ItemsSource = Model.Items;
            _contentPanelVisual = ElementCompositionPreview.GetElementVisual(ContentPanel);
            _root = ElementCompositionPreview.GetElementVisual(Root);
            _compositor = _contentPanelVisual.Compositor;

            ConfigureInteractionTracker();
        }



        private void ConfigureInteractionTracker()
        {
            _tracker = InteractionTracker.Create(_compositor);
            
            _interactionSource = VisualInteractionSource.Create(_root);
           
            _interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
            //_interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.PositionYChainingMode = InteractionChainingMode.Always;

            _tracker.InteractionSources.Add(_interactionSource);
            float refreshPanelHeight = (float)RefreshPanel.ActualHeight;

            _tracker.MaxPosition = new Vector3((float)Root.ActualWidth, 0, 0);
            _tracker.MinPosition = new Vector3(-(float)Root.ActualWidth, -refreshPanelHeight, 0);

            

            //The PointerPressed handler needs to be added using AddHandler method with the handledEventsToo boolean set to "true"
            //instead of the XAML element's "PointerPressed=Window_PointerPressed",
            //because the list view needs to chain PointerPressed handled events as well. 
            ContentPanel.AddHandler(PointerPressedEvent, new PointerEventHandler(Window_PointerPressed), true);

            SetupPullToRefreshBehavior(refreshPanelHeight);

            //
            // Use the Tacker's Position (negated) to apply to the Offset of the Image. The -{refreshPanelHeight} is to hide the refresh panel
            //
            m_positionExpression = _compositor.CreateExpressionAnimation($"-tracker.Position.Y - {refreshPanelHeight} ");
            m_positionExpression.SetReferenceParameter("tracker", _tracker);
            _contentPanelVisual.StartAnimation("Offset.Y", m_positionExpression);
            
        }

        private void Window_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)

            {
                // Tell the system to use the gestures from this pointer point (if it can).
                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(null));
            }
        }

        // Apply two sourcemodifiers to the input source: One to provide resistance, one to stop motion
        public void SetupPullToRefreshBehavior(
            float pullToRefreshDistance)
        {
            //
            // Modifier 1: Cut DeltaY to a third as long as the InteractionTracker is not yet at the 
            // pullRefreshDistance.
            //

            CompositionConditionalValue resistanceModifier = CompositionConditionalValue.Create(_compositor);

            ExpressionAnimation resistanceCondition = _compositor.CreateExpressionAnimation(
                $"-tracker.Position.Y < {pullToRefreshDistance}");

            resistanceCondition.SetReferenceParameter("tracker", _tracker);

            ExpressionAnimation resistanceAlternateValue = _compositor.CreateExpressionAnimation(
            "source.DeltaPosition.Y / 3");

            resistanceAlternateValue.SetReferenceParameter("source", _interactionSource);

            resistanceModifier.Condition = resistanceCondition;
            resistanceModifier.Value = resistanceAlternateValue;

            //
            // Modifier 2: Zero the delta if we are past the pullRefreshDistance. (So we can't pan 
            // past the pullRefreshDistance)
            //

            CompositionConditionalValue stoppingModifier = CompositionConditionalValue.Create(_compositor);

            ExpressionAnimation stoppingCondition = _compositor.CreateExpressionAnimation(
                            $"-tracker.Position.Y >= {pullToRefreshDistance}");

            stoppingCondition.SetReferenceParameter("tracker", _tracker);

            ExpressionAnimation stoppingAlternateValue = _compositor.CreateExpressionAnimation("0");
            
            stoppingModifier.Condition = stoppingCondition;
            stoppingModifier.Value = stoppingAlternateValue;

            //
            // Modifier 3: Zero the delta if we pull it back up past the 0 point. (So we can't pan 
            // past the pullRefreshDistance)
            //

            CompositionConditionalValue stoppingUpModifier = CompositionConditionalValue.Create(_compositor);

            ExpressionAnimation stoppingUpCondition = _compositor.CreateExpressionAnimation(
            $"-tracker.Position.Y < 0");

            stoppingUpCondition.SetReferenceParameter("tracker", _tracker);

            ExpressionAnimation stoppingUpAlternateValue = _compositor.CreateExpressionAnimation("0");
            
            stoppingUpModifier.Condition = stoppingUpCondition;
            stoppingUpModifier.Value = stoppingUpAlternateValue;

            
            //
            // Apply the modifiers to the source as a list
            //

            List<CompositionConditionalValue> modifierList =
            new List<CompositionConditionalValue>() { resistanceModifier, stoppingModifier , stoppingUpModifier};

            _interactionSource.ConfigureDeltaPositionYModifiers(modifierList);

           
        }
        

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);
            System.Diagnostics.Debug.WriteLine("GridClip.Rect" + GridClip.Rect);
        }
        
    }
}
