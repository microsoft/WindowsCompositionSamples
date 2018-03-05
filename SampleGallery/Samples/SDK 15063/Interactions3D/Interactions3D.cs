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

using ExpressionBuilder;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.Text;

using EF = ExpressionBuilder.ExpressionFunctions;
using CompositionSampleGallery.Shared;
using System.Collections.ObjectModel;

namespace CompositionSampleGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Interactions3D : SamplePage, IInteractionTrackerOwner
    {
        public static string    StaticSampleName => "Interactions 3D"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Demonstrates the use of an InteractionTracker to manipulate a 3D space.  Touch the screen to pinch and pan around the 3D scene.";
        public override string  SampleDescription => StaticSampleDescription;
        public override string  SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868947";

        public Interactions3D()
        {
            this.InitializeComponent();
            Model = new LocalDataSource();
            _thumbnails = Model.AggregateDataSources(new ObservableCollection<Thumbnail>[] { Model.Landscapes, Model.Nature });
             
            _random = new Random();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Configure the camera and root scene structure
            ConfigureWorldView();

            // Configure the InteractionTrackers to handle the appropriate types of input
            ConfigureInteractionTracker();

            // Load the nodes (data) which will be used to render the images and position them in the correct location
            LoadNodes();

            // Load the images to be displayed
            LoadImages();

            // Setup some ambient animations to keep the scene moving even if no one is actively interacting
            ConfigureAmbientAnimations();
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _tracker.Dispose();
            _tracker = null;

            foreach(var brush in _imageBrushes)
            {
                brush.Dispose();
            }
        }


        private void ConfigureInteractionTracker()
        {
            _interactionSource = VisualInteractionSource.Create(_rootContainer);
            _interactionSource.ScaleSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;

            _tracker = InteractionTracker.CreateWithOwner(_compositor, this);
            _tracker.MinScale = 0.6f;
            _tracker.MaxScale = 5.0f;

            _tracker.MaxPosition = new Vector3((float)Root.ActualWidth * 1.5f, 0, 0);
            _tracker.MinPosition = _tracker.MaxPosition * -1;

            _tracker.ScaleInertiaDecayRate = 0.96f;

            _tracker.InteractionSources.Add(_interactionSource);

            var tracker = _tracker.GetReference();

            //
            // Here's the trick: we take the scale output from the tracker, and convert it into a
            // value that represents Z.  Then we bind it to the world container's Z position.
            //

            var scaleExpression = EF.Lerp(0, 1000, (1 - tracker.Scale) / (1 - tracker.MaxScale));
            _worldContainer.StartAnimation("Offset.Z", scaleExpression);

            //
            // Bind the output of the tracker to the world container's XY position.
            //

            _worldContainer.StartAnimation("Offset.XY", -tracker.Position.XY);


            //
            // Scaling usually affects position.  This depends on the center point of the scale.  
            // But for our UI, we want don't scale to adjust the position (since we're using scale 
            // to change Offset.Z).  So to prevent scale from affecting position, we must always use
            // the top-left corner of the WorldContainer as the center point (note: we could also 
            // use the tracker's negated position, since that's where WorldContainer is getting its 
            // offset).  
            //
            // Create input modifiers to override the center point value.
            //

            var centerpointXModifier = CompositionConditionalValue.Create(_compositor);
            var centerpointYModifier = CompositionConditionalValue.Create(_compositor);

            centerpointXModifier.Condition = _compositor.CreateExpressionAnimation("true");
            centerpointXModifier.Value = _compositor.CreateExpressionAnimation("world.Offset.X");
            centerpointXModifier.Value.SetReferenceParameter("world", _worldContainer);

            _interactionSource.ConfigureCenterPointXModifiers(new[] { centerpointXModifier });
            _tracker.ConfigureCenterPointXInertiaModifiers(new[] { centerpointXModifier });


            centerpointYModifier.Condition = _compositor.CreateExpressionAnimation("true");
            centerpointYModifier.Value = _compositor.CreateExpressionAnimation("world.Offset.Y");
            centerpointYModifier.Value.SetReferenceParameter("world", _worldContainer);

            _interactionSource.ConfigureCenterPointYModifiers(new[] { centerpointYModifier });
            _tracker.ConfigureCenterPointYInertiaModifiers(new[] { centerpointYModifier });
        }


        private void ConfigureWorldView()
        {
            _rootContainer = ElementCompositionPreview.GetElementVisual(Root);
            _rootContainer.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);

            _compositor = _rootContainer.Compositor;

            //
            // Setup a container to represent the camera and define the perspective transform.
            //

            const float cameraDistance = -400.0f;
            var perspectiveMatrix = new Matrix4x4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 1 / cameraDistance,
                    0, 0, cameraDistance, 1);

            ContainerVisual cameraVisual = _compositor.CreateContainerVisual();
            cameraVisual.TransformMatrix = perspectiveMatrix;
            cameraVisual.Offset = new Vector3(_rootContainer.Size.X * 0.5f,
                                                   _rootContainer.Size.Y * 0.5f,
                                                   0);

            ElementCompositionPreview.SetElementChildVisual(Root, cameraVisual);

            //
            // Create a container to contain the world content.
            //

            _worldContainer = _compositor.CreateContainerVisual();
            _worldContainer.Offset = new Vector3(0, 0, -900);
            cameraVisual.Children.InsertAtTop(_worldContainer);
        }


        private void LoadNodes()
        {
            _nodes = new List<NodeInfo>(200);

            //
            // First, add in the text nodes
            //

            CanvasTextFormat monthsTextFormat = new CanvasTextFormat();
            monthsTextFormat.FontFamily = "Segoe UI";
            monthsTextFormat.FontSize = 120.0f;

            CanvasTextFormat textFormat = new CanvasTextFormat();
            textFormat.FontFamily = "Segoe UI";
            textFormat.FontSize = 36.0f;

            _textNodes = new TextNodeInfo[]
            {
                new TextNodeInfo("JAN    FEB    MAR    APR    MAY", monthsTextFormat, new Vector2(1920, 200), new Vector3(0.0f, -2500.0f, -3000.0f), 10.0f, 0.1f, false),
                new TextNodeInfo("LAYOUT", textFormat, new Vector2(180, 52), new Vector3(76.1f, 131.2f, -100.0f), 1.0f, 0.5f, true),
                new TextNodeInfo("ADVENTURE", textFormat, new Vector2(240, 52), new Vector3(-259.8f, -101.1f, -500.0f), 0.8f, 0.5f, true),
                new TextNodeInfo("ENGINEERING", textFormat, new Vector2(280, 52), new Vector3(509.8f, 51.1f, -322.2f), 0.8f, 0.5f, true),
            };

            _nodes.AddRange(_textNodes);



            //
            // Add some extra random images to fill up the world
            //


            //
            // Set the min/max of the images to limits how far in each directly we randomly place each image.
            //
            // Note: for z-position, the items are also randomly placed on one of several planes.  It looks 
            // a little too scattered/chaotic otherwise.
            //

            var min = new Vector3(_tracker.MinPosition.X, -_rootContainer.Size.Y * 0.65f, -25);
            var max = new Vector3(_tracker.MaxPosition.X, _rootContainer.Size.Y * 0.65f, 25);


            NodeManager.Instance.MinPosition = min;
            NodeManager.Instance.MaxPosition = max;

            NodeManager.Instance.MinScale = 0.05f;
            NodeManager.Instance.MaxScale = 0.4f;

            for (int i = _nodes.Count; i < _nodes.Capacity; i++)
            {
                _nodes.Add(NodeManager.Instance.GenerateRandomImageNode(_thumbnails.Count));
            }


            //
            // Sort the list by z, then x, then y so that they draw in the correct order.
            //

            _nodes.Sort();
        }


        async private void LoadImages()
        {
            int loadedImageCount = 0;

            //
            // Populate/load our unique list of image textures.
            //

            _imageBrushes = new ManagedSurface[_thumbnails.Count];

            for (int i = 0; i < _thumbnails.Count; i++)
            {
                Uri uri = new Uri(_thumbnails[i].ImageUrl);
                _imageBrushes[i] = await ImageLoader.Instance.LoadFromUriAsync(uri, new Size(500, 300));

                loadedImageCount++;
            }


            //
            // Populate/load our unique list of "text" image textures.
            //

            _textBrushes = new CompositionSurfaceBrush[_textNodes.Length];

            for (int i = 0; i < _textNodes.Length; i++)
            {
                var textNode = _textNodes[i];

                var textSurface = ImageLoader.Instance.LoadText(textNode.Text,  new Size(textNode.TextureSize.X, textNode.TextureSize.Y), textNode.TextFormat, Colors.Black, Colors.Transparent);

                _textBrushes[i] = _compositor.CreateSurfaceBrush(textSurface.Surface);

                //
                // Remember the index of the brush so that we can refer to it later.
                //

                textNode.BrushIndex = i;

                loadedImageCount++;
            }




            //
            // Once we've loaded all of the images, we can continue populating the world.
            //

            if (loadedImageCount == _imageBrushes.Length + _textBrushes.Length)
            {
                PopulateWorld();
            }
        }


        private void PopulateWorld()
        {

            //
            // Iterate through the list and populate the world with items.
            //

            foreach (var node in _nodes)
            {
                var imageNode = node as ImageNodeInfo;

                if (imageNode != null)
                {
                    AddImage(imageNode);
                }
                else
                {
                    var textNode = node as TextNodeInfo;

                    if (textNode != null)
                    {
                        AddText(textNode);
                    }
                }
            }
        }


        private void AddImage(ImageNodeInfo imageNodeInfo)
        {
            AddImage(_imageBrushes[(int)imageNodeInfo.ImageIndex].Brush, imageNodeInfo);
        }


        private void AddText(TextNodeInfo textNodeInfo)
        {
            var visual = AddImage(_textBrushes[textNodeInfo.BrushIndex], textNodeInfo, textNodeInfo.Opacity, textNodeInfo.ApplyDistanceEffects);

            if (!textNodeInfo.ApplyDistanceEffects)
            {
                visual.Opacity = textNodeInfo.Opacity;
            }
        }


        private Visual AddImage(CompositionSurfaceBrush imageBrush, NodeInfo nodeInfo, float defaultOpacity = 1.0f, bool applyDistanceEffects = true)
        {
            var sprite = _compositor.CreateSpriteVisual();

            var size = ((CompositionDrawingSurface)imageBrush.Surface).Size;
            size.Width *= nodeInfo.Scale;
            size.Height *= nodeInfo.Scale;

            sprite.Size = new Vector2((float)size.Width, (float)size.Height);
            sprite.AnchorPoint = new Vector2(0.5f, 0.5f);
            sprite.Offset = nodeInfo.Offset;
            _worldContainer.Children.InsertAtTop(sprite);


            if (applyDistanceEffects)
            {
                //
                // Use an ExpressionAnimation to fade the image out when it goes too close or 
                // too far away from the camera.
                //

                if (_opacityExpression == null)
                {
                    var visualTarget = ExpressionValues.Target.CreateVisualTarget();
                    var world = _worldContainer.GetReference();
                    _opacityExpression = EF.Conditional(
                                                defaultOpacity * (visualTarget.Offset.Z + world.Offset.Z) > -200,
                                                1 - EF.Clamp(visualTarget.Offset.Z + world.Offset.Z, 0, 300) / 300,
                                                EF.Clamp(visualTarget.Offset.Z + world.Offset.Z + 1300, 0, 300) / 300);
                }

                sprite.StartAnimation("Opacity", _opacityExpression);


            }
            sprite.Brush = imageBrush;

            return sprite;
        }

        public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
        {
            if (_timer != null)
            {
                _timer.Start();
            }
        }


        public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
        {
            if (_timer != null)
            {
                //
                // No need to queue up another transition when the user is interacting.  
                //
                // Note: We could still attempt to play the animations, but the Try* request would just be ignored by the tracker.
                //

                _timer.Stop();
            }
        }


        private void ConfigureAmbientAnimations()
        {
            Vector3 positionMargin = new Vector3(700, 0, 0);

            Vector3KeyFrameAnimation positionAnimation;
            ScalarKeyFrameAnimation scaleForZAnimation;
            double timeScale = 0.6;

            _ambientAnimations = new List<Tuple<AmbientAnimationTarget, CompositionAnimation>>();

            var tracker = _tracker.GetReference();

            //
            // Move by an amount 1/2 the distance to the near X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            var vec3StartingValue = ExpressionValues.StartingValue.CreateVector3StartingValue();
            var minPosExp = positionMargin + (tracker.MinPosition + vec3StartingValue) * 0.5f;
            positionAnimation.InsertExpressionKeyFrame(1, minPosExp);

            positionAnimation.Duration = TimeSpan.FromSeconds(15 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));

            //
            // Move by an amount 1/2 the distance to the far X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            var maxPosExp = -positionMargin + (tracker.MaxPosition - vec3StartingValue) * 0.5f;
            positionAnimation.InsertExpressionKeyFrame(1f, maxPosExp);
            positionAnimation.Duration = TimeSpan.FromSeconds(15 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));


            //
            // Move all the way to the near X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertKeyFrame(1, _tracker.MinPosition + positionMargin * 2);
            positionAnimation.Duration = TimeSpan.FromSeconds(15 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));



            //
            // Move all the way to the far X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertKeyFrame(1, _tracker.MaxPosition - positionMargin);
            positionAnimation.Duration = TimeSpan.FromSeconds(20 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));


            //
            // Move to 85% of the far X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertExpressionKeyFrame(1, -positionMargin + tracker.MaxPosition * 0.85f);

            positionAnimation.Duration = TimeSpan.FromSeconds(15 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));


            //
            // Move to mid-X.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertKeyFrame(1, _tracker.MinPosition + (_tracker.MaxPosition - _tracker.MinPosition) * 0.5f);
            positionAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));



            //
            // Move to min-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, _tracker.MinScale);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(18 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));

            //
            // Move to 30% of min-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, _tracker.MinScale * 1.3f);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));


            //
            // Move to mid-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, (_tracker.MaxScale - _tracker.MinScale) / 2.0f);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(11 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));


            //
            // Move to 70% of max-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, _tracker.MaxScale * 0.3f);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));



            //
            // Move to max-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, _tracker.MaxScale);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));


            //
            // Move forward Z by 50%.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            var scalarStartingValue = ExpressionValues.StartingValue.CreateScalarStartingValue();
            var scaleMinExp = EF.Min(tracker.MaxScale, scalarStartingValue * 1.5f);
            scaleForZAnimation.InsertExpressionKeyFrame(1, scaleMinExp);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));

            //
            // Move back Z by 50%.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            var scaleMaxExp = EF.Max(tracker.MinScale, scalarStartingValue * 0.5f);
            scaleForZAnimation.InsertExpressionKeyFrame(1, scaleMaxExp);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));


            // Start some ambient animations
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, object e)
        {
            _timer.Stop();

            int index = _lastAmbientAnimationIndex;


            //
            // Make sure the next one we pick isn't the same as the last one.
            //

            while (index == _lastAmbientAnimationIndex)
            {
                index = _random.Next(0, _ambientAnimations.Count - 1);
            }

            var transition = _ambientAnimations[index];

            switch (transition.Item1)
            {
                case AmbientAnimationTarget.PositionKeyFrame:
                    _tracker.TryUpdatePositionWithAnimation(transition.Item2);
                    break;

                case AmbientAnimationTarget.ScaleKeyFrame:
                    _tracker.TryUpdateScaleWithAnimation(transition.Item2, new Vector3());
                    break;
            }

            _lastAmbientAnimationIndex = index;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);
        }
        private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                // Tell the system to use the gestures from this pointer point (if it can).
                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(Root));

                // Stop ambient animations
                _tracker.TryUpdatePositionBy(Vector3.Zero);
            }
        }

        public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args)
        {
            // Unused for this sample
        }

        public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
        {
            // Unused for this sample
        }

        public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args)
        {
            // Unused for this sample
        }

        public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
        {
            // Unused for this sample
        }

        public LocalDataSource Model { get; set; }
        ObservableCollection<Thumbnail> _thumbnails;
        private ContainerVisual         _worldContainer;
        private Random                  _random;
        private InteractionTracker      _tracker;
        private VisualInteractionSource _interactionSource;
        private Visual                  _rootContainer;
        private List<NodeInfo>          _nodes;
        private Compositor              _compositor;
        private DispatcherTimer         _timer;
        private int                     _lastAmbientAnimationIndex = -1;
        private List<Tuple<AmbientAnimationTarget, CompositionAnimation>> 
                                        _ambientAnimations;

        private ManagedSurface[]        _imageBrushes;
        
        private CompositionSurfaceBrush[]   
                                        _textBrushes;
        private ExpressionNode          _opacityExpression;

        private TextNodeInfo[]          _textNodes;

        private enum AmbientAnimationTarget
        {
            PositionKeyFrame,
            ScaleKeyFrame,
        }        
    }
}
