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
using System.Linq;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class AnimationCompleted : SamplePage
    {
        private SpriteVisual _visual;
        private CompositionColorBrush _brush;
        private Compositor _compositor;
        private ScalarKeyFrameAnimation _showAnimation;
        private ScalarKeyFrameAnimation _hideAnimation;
        private CompositionScopedBatch _batch;
        private bool _isHidden;

        public AnimationCompleted()
        {
            this.InitializeComponent();
            Setup();
        }

        public static string StaticSampleName => "Animation Completed";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "This sample demonstrates how to get a notification and clean up resources when a CompositionAnimation completes";
        public override string SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761160";

        private void Setup()
        {
            var gridVisual = ElementCompositionPreview.GetElementVisual(MainGrid);
            _compositor = gridVisual.Compositor;

            _showAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _showAnimation.InsertKeyFrame(1.0f, 1.0f);
            _showAnimation.Duration = TimeSpan.FromSeconds(1);

            _hideAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _hideAnimation.InsertKeyFrame(1.0f, 0.0f);
            _hideAnimation.Duration = TimeSpan.FromSeconds(1);

            _isHidden = false;

            CreateVisualTree();
        }
        private void CreateVisualTree()
        {
            _visual = _compositor.CreateSpriteVisual();
            _visual.Size = new Vector2(300f);
            _brush = _compositor.CreateColorBrush(Colors.Blue);
            _visual.Brush = _brush;
            ElementCompositionPreview.SetElementChildVisual(MainGrid, _visual);
        }
        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isHidden)
            {
                CreateVisualTree();
                _visual.Opacity = 0.0f;
                _visual.StartAnimation("Opacity", _showAnimation);
                _isHidden = false;
            }
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isHidden)
            {
                _batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                _batch.Completed += OnBatchCompleted;
                _visual.StartAnimation("Opacity", _hideAnimation);
                _batch.End();
                _isHidden = true;
            }
        }
        private void OnBatchCompleted(object sender, CompositionBatchCompletedEventArgs args)
        {
            _visual.Dispose();
            _brush.Dispose();
            _visual = null;
            _brush = null;
        }
    }
}
