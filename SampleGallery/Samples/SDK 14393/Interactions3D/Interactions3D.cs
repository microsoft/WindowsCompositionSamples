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
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml.Hosting;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using SamplesCommon;
using Microsoft.Graphics.Canvas.Text;

namespace CompositionSampleGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Interactions3D : SamplePage, IInteractionTrackerOwner
    {
        public static string    StaticSampleName    { get { return "Interactions 3D"; } }
        public override string  SampleName          { get { return StaticSampleName; } }
        public override string  SampleDescription   { get { return "Demonstrates the use of an InteractionTracker to manipulate a 3D space.  Touch the screen to pinch and pan around the 3D scene."; } }

        public Interactions3D()
        {
            this.InitializeComponent();

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
            _positionTracker.Dispose();
            _positionTracker = null;

            _scaleTracker.Dispose();
            _scaleTracker = null;

            foreach(var surface in _managedSurfaces)
            {
                surface.Dispose();
            }
        }


        private void ConfigureInteractionTracker()
        {
            //
            // We're creating two interaction trackers, one nested in the other.  This allows us the user to use 
            // pinch/stretch to affect z-position, and not have it change the x-position.  The two work independently.
            //

            _interactionSource1 = VisualInteractionSource.Create(_rootContainer);
            _interactionSource1.ScaleSourceMode = InteractionSourceMode.EnabledWithoutInertia;

            _scaleTracker = InteractionTracker.CreateWithOwner(_compositor, this);
            _scaleTracker.MinScale = 0.6f;
            _scaleTracker.MaxScale = 5.0f;
            _scaleTracker.ScaleInertiaDecayRate = 0.96f;

            _scaleTracker.InteractionSources.Add(_interactionSource1);

            _interactionSource2 = VisualInteractionSource.Create(_rootContainer2);
            _interactionSource2.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;

            _positionTracker = InteractionTracker.CreateWithOwner(_compositor, this);
            _positionTracker.MaxPosition = new Vector3((float)Root.ActualWidth * 1.5f, 0, 0);
            _positionTracker.MinPosition = _positionTracker.MaxPosition * -1;

            _positionTracker.InteractionSources.Add(_interactionSource2);

            //
            // Here's the trick: we take the scale output from the outer (scale) Tracker, and convert 
            // it into a value that represents Z.  Then we bind it to the world container's Z position.
            //

            var scaleExpression = _compositor.CreateExpressionAnimation("lerp(0, 1000, (1 - tracker.Scale) / (1 - tracker.MaxScale))");
            scaleExpression.SetReferenceParameter("tracker", _scaleTracker);
            _worldContainer.StartAnimation("Offset.Z", scaleExpression);

            //
            // Bind the output of the inner (xy position) tracker to the world container's XY position.
            //

            var positionExpression = _compositor.CreateExpressionAnimation("-tracker.Position.XY");
            positionExpression.SetReferenceParameter("tracker", _positionTracker);
            _worldContainer.StartAnimation("Offset.XY", positionExpression);
        }


        private void ConfigureWorldView()
        {
            _rootContainer = ElementCompositionPreview.GetElementVisual(Root);
            _rootContainer.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);

            _compositor = _rootContainer.Compositor;

            _rootContainer2 = ElementCompositionPreview.GetElementVisual(Stage);
            _rootContainer2.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);


            // Setup a container to represent the camera and define the perspective transform
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
            //((ContainerVisual)_rootContainer2).Children.InsertAtTop(cameraVisual);
            ElementCompositionPreview.SetElementChildVisual(Stage, cameraVisual);

            // Create a container to contain the world content
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

            var min = new Vector3(_positionTracker.MinPosition.X, -_rootContainer.Size.Y * 0.65f, -25);
            var max = new Vector3(_positionTracker.MaxPosition.X, _rootContainer.Size.Y * 0.65f, 25);


            NodeManager.Instance.MinPosition = min;
            NodeManager.Instance.MaxPosition = max;

            NodeManager.Instance.MinScale = 0.05f;
            NodeManager.Instance.MaxScale = 0.4f;

            for (int i = _nodes.Count; i < _nodes.Capacity; i++)
            {
                _nodes.Add(NodeManager.Instance.GenerateRandomImageNode());
            }


            //
            // Sort the list by z, then x, then y so that they draw in the correct order.
            //

            _nodes.Sort();
        }


        private async void LoadImages()
        {
            int loadedImageCount = 0;


            //
            // Populate/load our unique list of image textures.
            //

            for (int i = 0; i < (int)NamedImage.Count; i++)
            {
                var name = (NamedImage)i;

                Uri uri = new Uri($"ms-appx:///Assets/Photos/{name.ToString()}.jpg");
                _managedSurfaces[i] = await ImageLoader.Instance.LoadFromUriAsync(uri);

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

            if (loadedImageCount == _managedSurfaces.Length + _textBrushes.Length)
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
            AddImage(_managedSurfaces[(int)imageNodeInfo.NamedImage].Brush, imageNodeInfo);
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

                if (_opacityAnimation == null)
                {
                    _opacityAnimation = _compositor.CreateExpressionAnimation(@"defaultOpacity * (this.target.Offset.z + world.Offset.z > -200 ?
                                                                                (1 - (clamp(this.target.Offset.z + world.Offset.z, 0, 300) / 300)) : 
                                                                                (clamp(this.target.Offset.z + world.Offset.z + 1300, 0, 300) / 300))");
                    _opacityAnimation.SetReferenceParameter("world", _worldContainer);
                }

                _opacityAnimation.SetScalarParameter("defaultOpacity", defaultOpacity);
                sprite.StartAnimation("Opacity", _opacityAnimation);


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


            //
            // Move by an amount 1/2 the distance to the near X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertExpressionKeyFrame(1, "positionMargin + (tracker.MinPosition + this.StartingValue) * 0.5");
            positionAnimation.Duration = TimeSpan.FromSeconds(15 * timeScale);
            positionAnimation.SetReferenceParameter("tracker", _positionTracker);
            positionAnimation.SetVector3Parameter("positionMargin", positionMargin);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));

            //
            // Move by an amount 1/2 the distance to the far X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertExpressionKeyFrame(1, "-positionMargin + (tracker.MaxPosition - this.StartingValue) * 0.5");
            positionAnimation.Duration = TimeSpan.FromSeconds(15 * timeScale);
            positionAnimation.SetReferenceParameter("tracker", _positionTracker);
            positionAnimation.SetVector3Parameter("positionMargin", positionMargin);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));


            //
            // Move all the way to the near X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertKeyFrame(1, _positionTracker.MinPosition + positionMargin * 2);
            positionAnimation.Duration = TimeSpan.FromSeconds(15 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));



            //
            // Move all the way to the far X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertKeyFrame(1, _positionTracker.MaxPosition - positionMargin);
            positionAnimation.Duration = TimeSpan.FromSeconds(20 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));


            //
            // Move to 85% of the far X edge.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertExpressionKeyFrame(1, "-positionMargin + tracker.MaxPosition * 0.85");
            positionAnimation.Duration = TimeSpan.FromSeconds(15 * timeScale);
            positionAnimation.SetReferenceParameter("tracker", _positionTracker);
            positionAnimation.SetVector3Parameter("positionMargin", positionMargin);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));


            //
            // Move to mid-X.
            //

            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();

            positionAnimation.InsertKeyFrame(1, _positionTracker.MinPosition + (_positionTracker.MaxPosition - _positionTracker.MinPosition) * 0.5f);
            positionAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.PositionKeyFrame,
                                                positionAnimation));



            //
            // Move to min-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, _scaleTracker.MinScale);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(18 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));

            //
            // Move to 30% of min-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, _scaleTracker.MinScale * 1.3f);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));


            //
            // Move to mid-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, (_scaleTracker.MaxScale - _scaleTracker.MinScale) / 2.0f);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(11 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));


            //
            // Move to 70% of max-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, _scaleTracker.MaxScale * 0.3f);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));



            //
            // Move to max-Z.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertKeyFrame(1, _scaleTracker.MaxScale);
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));


            //
            // Move forward Z by 50%.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertExpressionKeyFrame(1, "min(tracker.MaxScale, this.StartingValue * 1.50)");
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);
            scaleForZAnimation.SetReferenceParameter("tracker", _scaleTracker);

            _ambientAnimations.Add(new Tuple<AmbientAnimationTarget, CompositionAnimation>(
                                                AmbientAnimationTarget.ScaleKeyFrame,
                                                scaleForZAnimation));

            //
            // Move back Z by 50%.
            //

            scaleForZAnimation = _compositor.CreateScalarKeyFrameAnimation();

            scaleForZAnimation.InsertExpressionKeyFrame(1, "max(tracker.MinScale, this.StartingValue * 0.50)");
            scaleForZAnimation.Duration = TimeSpan.FromSeconds(10 * timeScale);
            scaleForZAnimation.SetReferenceParameter("tracker", _scaleTracker);

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
                    _positionTracker.TryUpdatePositionWithAnimation(transition.Item2);
                    break;

                case AmbientAnimationTarget.ScaleKeyFrame:
                    _scaleTracker.TryUpdateScaleWithAnimation(transition.Item2, new Vector3());
                    break;
            }

            _lastAmbientAnimationIndex = index;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);
        }
        private void Stage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                // Tell the system to use the gestures from this pointer point (if it can).
                _interactionSource2.TryRedirectForManipulation(e.GetCurrentPoint(Stage));
                _interactionSource1.TryRedirectForManipulation(e.GetCurrentPoint(Stage));

                // Stop ambient animations for both trackers
                _positionTracker.TryUpdatePositionBy(Vector3.Zero);
                _scaleTracker.TryUpdatePositionBy(Vector3.Zero);
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

        private ContainerVisual         _worldContainer;
        private Random                  _random;
        private InteractionTracker      _positionTracker;
        private InteractionTracker      _scaleTracker;
        private VisualInteractionSource _interactionSource1;
        private VisualInteractionSource _interactionSource2;
        private Visual                  _rootContainer;
        private Visual                  _rootContainer2;
        private List<NodeInfo>          _nodes;
        private Compositor              _compositor;
        private DispatcherTimer         _timer;
        private int                     _lastAmbientAnimationIndex = -1;
        private List<Tuple<AmbientAnimationTarget, CompositionAnimation>> 
                                        _ambientAnimations;
        private ManagedSurface[]       _managedSurfaces = new ManagedSurface[(int)NamedImage.Count];
        
        private CompositionSurfaceBrush[]   
                                        _textBrushes;
        private ExpressionAnimation     _opacityAnimation;

        private TextNodeInfo[]          _textNodes;

        private enum AmbientAnimationTarget
        {
            PositionKeyFrame,
            ScaleKeyFrame,
        }        
    }
}
