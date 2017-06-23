using DepthDemo.Scenarios;
using ExpressionBuilder;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

using EF = ExpressionBuilder.ExpressionFunctions;

namespace DepthDemo
{

    public sealed partial class MainPage : Page, IInteractionTrackerOwner
    {
        #region vars
        private Compositor _compositor;
        private ContainerVisual _mainContainer;
        private InteractionTracker _tracker;
        private VisualInteractionSource _interactionSource;
        private List<Scenario> _scenarios;
        private Scenario _currentScenario;
        private Dictionary<Scenario, SpriteVisual> _scenarioContainersMapping;
        private SpriteVisual _activeScenarioVisualIndicator;
        private NestedScenario _nestedScenario;
        private BasicElementsScenario _basicScenario;
        #endregion
        public MainPage()
        {
            this.InitializeComponent();

            _scenarioContainersMapping = new Dictionary<Scenario, SpriteVisual>();
            _scenarios = new List<Scenario>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(MainGrid).Compositor;

            _activeScenarioVisualIndicator = _compositor.CreateSpriteVisual();
            _activeScenarioVisualIndicator.Brush = _compositor.CreateColorBrush(Color.FromArgb(255, 0, 153, 153));
            ElementCompositionPreview.SetElementChildVisual(ProgressIndicatorStackPanel, _activeScenarioVisualIndicator);

            // Create and add container for layers
            _mainContainer = _compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(MainCanvas, _mainContainer);

            InitializeScenarios();

            ConfigureInteractionTracker();
        }

        private void InitializeScenarios()
        {
            _nestedScenario = new NestedScenario(0, _compositor, MainCanvas);
            _scenarios.Add(_nestedScenario);
            _basicScenario = new BasicElementsScenario(1, _compositor, MainCanvas);
            _scenarios.Add(_basicScenario);

            // For each scenario, allocate a spritevisual scenario container the size of maincanvas 
            // and add the scenario container to the maincontainer
            var nextOffset = new Vector3(0, 0, 0);
            for (int i = 0; i < _scenarios.Count; i++)
            {
                var scenarioContainer = _compositor.CreateSpriteVisual();
                scenarioContainer.Size = new Vector2((float)MainCanvas.ActualWidth, (float)MainCanvas.ActualHeight);
                scenarioContainer.Offset = nextOffset;
                _mainContainer.Children.InsertAtTop(scenarioContainer);

                _scenarioContainersMapping.Add(_scenarios[i], scenarioContainer);

                nextOffset = new Vector3(nextOffset.X + scenarioContainer.Size.X, 0, 0);
            }

            _currentScenario = _scenarios[0];
            _currentScenario.IsActive = true;

            InitializeContent();

            // For each scenario, add a button navigation/progress indicator
            foreach (Scenario scenario in _scenarios)
            {
                Button bt = new Button();
                bt.Click += ProgressIndicatorButton_Click;
                bt.Name = scenario.Identifier.ToString();
                bt.Content = "Scenario " + scenario.Identifier;
                bt.Background = new SolidColorBrush(Colors.Transparent);

                ProgressIndicatorStackPanel.Children.Add(bt);
            }
        }

        private void InitializeContent()
        {
            foreach (Scenario scenario in _scenarios)
            {
                scenario.ImplementScenario(_compositor, _scenarioContainersMapping[scenario]);
            }
        }

        public void ConfigureInteractionTracker()
        {
            var backgroundVisual = ElementCompositionPreview.GetElementVisual(MainGrid);
            backgroundVisual.Size = new Vector2((float)MainGrid.ActualWidth, (float)MainGrid.ActualHeight);

            // Configure interaction tracker
            _tracker = InteractionTracker.CreateWithOwner(_compositor, this);
            _tracker.MaxPosition = new Vector3((float)backgroundVisual.Size.X * _scenarios.Count, backgroundVisual.Size.Y, 0);
            _tracker.MinPosition = new Vector3();

            // Configure interaction source
            _interactionSource = VisualInteractionSource.Create(backgroundVisual);
            _interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;
            _tracker.InteractionSources.Add(_interactionSource);

            // Bind interaction tracker output to animation for now
            var positionExpression = -_tracker.GetReference().Position.X;
            _mainContainer.StartAnimation("Offset.X", positionExpression);

            ConfigureRestingPoints();

            var nestedVisuals = _nestedScenario.GetVisuals();
            var exp = _compositor.CreateExpressionAnimation();
            for (int i = 0; i < nestedVisuals.Count; i++)
            {
                ConfigureParallax(i, nestedVisuals[i]);
            }
        }
        private void ConfigureParallax(int index, Visual visual)
        {
            var parallaxExpression = _compositor.CreateExpressionAnimation(
                                            "this.startingvalue + " +
                                            "tracker.PositionVelocityInPixelsPerSecond.X *" +
                                            "index / 100");
            parallaxExpression.SetReferenceParameter("tracker", _tracker);
            parallaxExpression.SetScalarParameter("index", index);

            var parallaxExpression2 = ExpressionValues.StartingValue.CreateScalarStartingValue() +
                                        _tracker.GetReference().PositionVelocityInPixelsPerSecond.X *
                                        index / 50;

            visual.StartAnimation("Offset.X", parallaxExpression2);
        }
        private void ConfigureRestingPoints()
        {
            var size = (_tracker.MaxPosition.X - _tracker.MinPosition.X);
            var props = _compositor.CreatePropertySet();
            props.InsertScalar("size", (_tracker.MaxPosition.X - _tracker.MinPosition.X));
            props.InsertScalar("numScenarios", (_scenarios.Count));

            var endpoint1 = InteractionTrackerInertiaRestingValue.Create(_compositor);

            var endpoint1ExpressionAnimation = _tracker.GetReference().NaturalRestingPosition.X < (size / _scenarios.Count);
            endpoint1.SetCondition(endpoint1ExpressionAnimation);

            endpoint1.RestingValue = _compositor.CreateExpressionAnimation("this.target.MinPosition.x");

            // Update endpoints for number of scenarios
            InteractionTrackerInertiaModifier[] endpoints = new InteractionTrackerInertiaModifier[_scenarios.Count];
            endpoints[0] = endpoint1;
            for (int i = 1; i < _scenarios.Count - 1; i++)
            {
                var endpoint = InteractionTrackerInertiaRestingValue.Create(_compositor);

                var endpointExpressionAnimation = _tracker.GetReference().NaturalRestingPosition.X > (size / _scenarios.Count) * i &
                                                    _tracker.GetReference().NaturalRestingPosition.X <= (i + 1) * size / _scenarios.Count;
                endpoint.SetCondition(endpointExpressionAnimation);

                var restingValueExpressionAnimation = i * _tracker.GetReference().MaxPosition.X / _scenarios.Count;
                endpoint.SetRestingValue(restingValueExpressionAnimation);


                endpoints[i] = endpoint;
            }

            var finalEndpoint = InteractionTrackerInertiaRestingValue.Create(_compositor);

            var finalEndpointExpressionAnimation = _tracker.GetReference().NaturalRestingPosition.X > (_scenarios.Count - 1) * size / _scenarios.Count;
            finalEndpoint.SetCondition(finalEndpointExpressionAnimation);

            var finalRestingValueExpressionAnimation = (_scenarios.Count - 1) * _tracker.GetReference().MaxPosition.X / _scenarios.Count;
            finalEndpoint.SetRestingValue(finalRestingValueExpressionAnimation);

            endpoints[_scenarios.Count - 1] = finalEndpoint;

            _tracker.ConfigurePositionXInertiaModifiers(endpoints);
        }

        #region PointerHandlers
        private void MainGrid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                try
                {
                    _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(MainGrid));
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignoring the failed redirect to prevent app crashing
                }
            }
        }

        private void MainCanvas_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Pass event on to the scenario that's in view
            foreach (Scenario scenario in _scenarios)
            {
                if (_scenarioContainersMapping[scenario].Offset.X == _tracker.Position.X)
                {
                    scenario.CanvasPointerPressed(sender, e, MainCanvas);
                }
            }
        }

        private void MainCanvas_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Pass event on to the scenario that's in view
            foreach (Scenario scenario in _scenarios)
            {
                if (_scenarioContainersMapping[scenario].Offset.X == _tracker.Position.X)
                {
                    scenario.CanvasPointerMoved(sender, e, MainCanvas);
                }
            }
        }

        private void MainCanvas_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            // Pass event on to the scenario that's in view
            foreach (Scenario scenario in _scenarios)
            {
                if (_scenarioContainersMapping[scenario].Offset.X == _tracker.Position.X)
                {
                    scenario.CanvasDoubleTapped(sender, e, MainCanvas);
                }
            }
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_scenarios.Count > 0)
            {
                // Resize the container for each scenario
                ContainerVisual previousScenarioContainer = null;
                for (int i = 0; i < _scenarios.Count; i++)
                {
                    var scenarioContainer = _scenarioContainersMapping[_scenarios[i]];
                    scenarioContainer.Size = new Vector2((float)MainCanvas.ActualWidth, (float)MainCanvas.ActualHeight);

                    // Update offset when applicable
                    if (previousScenarioContainer != null)
                    {
                        scenarioContainer.Offset = new Vector3(previousScenarioContainer.Offset.X + previousScenarioContainer.Size.X, 0, 0);
                    }

                    // Update content on a per-scenario basis
                    _scenarios[i].SizeChanged();

                    previousScenarioContainer = scenarioContainer;
                }

                // Update tracker position
                var newOffset = _scenarioContainersMapping[_currentScenario].Offset;
                _tracker.TryUpdatePosition(newOffset);

                // Update resting points
                var backgroundVisual = ElementCompositionPreview.GetElementVisual(MainGrid);
                backgroundVisual.Size = new Vector2((float)MainGrid.ActualWidth, (float)MainGrid.ActualHeight);
                _tracker.MaxPosition = new Vector3((float)backgroundVisual.Size.X * _scenarios.Count, backgroundVisual.Size.Y, 0);
                ConfigureRestingPoints();
            }
        }

        private void ProgressIndicatorButton_Click(object sender, RoutedEventArgs e)
        {
            TryNavigateToScenario(int.Parse((sender as Button).Name));
        }

        private async void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            // Pull up scenario help popup
            string helpTextInfo = _currentScenario.HelpTextInstructions;

            var messageDialog = new MessageDialog(helpTextInfo);
            await messageDialog.ShowAsync();
        }
        #endregion

        #region Navigation
        private void TryNavigateToScenario(int num)
        {
            foreach (Scenario scenario in _scenarios)
            {
                if (scenario.Identifier == num && _currentScenario != scenario)
                {
                    var oldScenario = _currentScenario;
                    _currentScenario = scenario;

                    var newOffset = _scenarioContainersMapping[scenario].Offset;
                    var currOffset = _tracker.Position;

                    // Offset animation
                    Vector3KeyFrameAnimation offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
                    offsetAnimation.InsertKeyFrame(0.0f, currOffset);
                    offsetAnimation.InsertKeyFrame(1.0f, newOffset);
                    offsetAnimation.Duration = ConfigurationConstants.FocusAnimationDuration;
                    offsetAnimation.Target = "Position";

                    _tracker.TryUpdatePositionWithAnimation(offsetAnimation);

                    UpdateActiveScenarioIndicator(oldScenario);

                    break;
                }
            }
        }

        private Scenario GetActiveScenarioByTrackerPosition(Vector3 trackerPosition)
        {
            foreach (Scenario scenario in _scenarios)
            {
                if (_scenarioContainersMapping[scenario].Offset == trackerPosition)
                {
                    return scenario;
                }
            }
            return null;
        }

        private void UpdateActiveScenarioIndicator(Scenario oldScenario)
        {
            oldScenario.IsActive = false;
            _currentScenario.IsActive = true;

            var activeScenarioNum = _scenarios.IndexOf(_currentScenario);

            // Update nav active scenario indicator
            // Get offset/size of new active scenario button
            var buttonWidth = (float)ProgressIndicatorStackPanel.Children[activeScenarioNum].RenderSize.Width;
            var buttonOffset = ProgressIndicatorStackPanel.Children[activeScenarioNum].GetOffset(ProgressIndicatorStackPanel);
            var offsetDeltaX = Math.Abs(buttonOffset.X - _activeScenarioVisualIndicator.Offset.X);

            // Scoped batch for first half of the animation
            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Batch_Completed;

            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();

            // Animate line size
            ScalarKeyFrameAnimation sizeGrowAnimation = _compositor.CreateScalarKeyFrameAnimation();
            sizeGrowAnimation.Duration = ConfigurationConstants.NavAnimationDuration;
            sizeGrowAnimation.InsertKeyFrame(0.0f, _activeScenarioVisualIndicator.Size.X);
            sizeGrowAnimation.InsertKeyFrame(1.0f, offsetDeltaX + _activeScenarioVisualIndicator.Size.X);
            sizeGrowAnimation.Target = "Size.X";
            if (buttonOffset.X < _activeScenarioVisualIndicator.Offset.X)
            {
                // Add offset animation to size change, to make the line appear to move to the left
                ScalarKeyFrameAnimation offsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
                offsetAnimation.InsertKeyFrame(0.0f, _activeScenarioVisualIndicator.Offset.X);
                offsetAnimation.InsertKeyFrame(1.0f, buttonOffset.X);
                offsetAnimation.Duration = ConfigurationConstants.NavAnimationDuration;
                offsetAnimation.Target = "Offset.X";
                animationGroup.Add(offsetAnimation);
            }

            animationGroup.Add(sizeGrowAnimation);

            _activeScenarioVisualIndicator.StartAnimationGroup(animationGroup);

            batch.End();
        }

        private void Batch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            // Start the second part of the animations

            // Get offset/size of new active scenario button
            var activeScenarioNum = _scenarios.IndexOf(_currentScenario);
            var buttonWidth = (float)ProgressIndicatorStackPanel.Children[activeScenarioNum].RenderSize.Width;
            var buttonOffset = ProgressIndicatorStackPanel.Children[activeScenarioNum].GetOffset(ProgressIndicatorStackPanel);
            var offsetDeltaX = Math.Abs(buttonOffset.X - _activeScenarioVisualIndicator.Offset.X);

            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();

            // Animate line size
            ScalarKeyFrameAnimation sizeShrinkAnimation = _compositor.CreateScalarKeyFrameAnimation();
            sizeShrinkAnimation.Duration = ConfigurationConstants.NavAnimationDuration;
            sizeShrinkAnimation.InsertKeyFrame(0.0f, _activeScenarioVisualIndicator.Size.X);
            sizeShrinkAnimation.InsertKeyFrame(1.0f, buttonWidth);
            sizeShrinkAnimation.Target = "Size.X";
            animationGroup.Add(sizeShrinkAnimation);

            if (buttonOffset.X > _activeScenarioVisualIndicator.Offset.X)
            {
                // Animate line to new offset
                ScalarKeyFrameAnimation offsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
                offsetAnimation.InsertKeyFrame(0.0f, _activeScenarioVisualIndicator.Offset.X);
                offsetAnimation.InsertKeyFrame(1.0f, buttonOffset.X);
                offsetAnimation.Duration = ConfigurationConstants.NavAnimationDuration;
                offsetAnimation.Target = "Offset.X";
                animationGroup.Add(offsetAnimation);
            }

            _activeScenarioVisualIndicator.StartAnimationGroup(animationGroup);
        }

        private void ProgressIndicatorStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var x = ProgressIndicatorStackPanel;
            var y = ProgressIndicatorStackPanel.Children[0];

            if (ProgressIndicatorStackPanel.Children.Count > 0)
            {
                var buttonHeight = (float)ProgressIndicatorStackPanel.Children[0].RenderSize.Height;
                var buttonOffset = ProgressIndicatorStackPanel.Children[0].GetOffset(ProgressIndicatorStackPanel);

                _activeScenarioVisualIndicator.Offset = new Vector3(0, buttonHeight + buttonOffset.Y, 0);
                _activeScenarioVisualIndicator.Size = new Vector2((float)ProgressIndicatorStackPanel.ActualWidth / _scenarios.Count, 3);
            }
        }
        #endregion

        #region Callbacks

        public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args) { }

        public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
        {
            // Check offset for scenario and update progress indicator
            var newScenario = GetActiveScenarioByTrackerPosition(sender.Position);
            if (_currentScenario != newScenario && newScenario != null)
            {
                var oldScenario = _currentScenario;
                _currentScenario = newScenario;

                UpdateActiveScenarioIndicator(oldScenario);
            }
        }

        public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args) { }

        public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args) { }

        public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args) { }

        public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args) { }

        #endregion Callbacks

    }


    public static class UIElementExtensions
    {
        public static Vector3 GetOffset(this UIElement element, UIElement relativeTo = null)
        {
            var transform = element.TransformToVisual(relativeTo ?? Window.Current.Content);
            var point = transform.TransformPoint(new Point(0, 0));
            Vector3 offset = new Vector3((float)point.X, (float)point.Y, 0);

            return offset;
        }
    }

}