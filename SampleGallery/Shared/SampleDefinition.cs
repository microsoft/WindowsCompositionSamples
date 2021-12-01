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

using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositionSampleGallery
{

    public enum SampleType
    {
        Reference,
        EndToEnd
    };

    public enum SampleCategory
    {
        None,
        Light,
        Depth,
        Motion,
        Material,
        Scale,
        APIReference,
        Input,
    }

    public class SampleDefinition
    {
        private const string MissingThumbnailAsset = "ms-appx:///Assets/Other/MissingThumbnail.png";
        private string _name;
        private Type _pageType;
        private SampleType _sampleType;
        private SampleCategory _sampleCategory;
        private string _imageUrl;
        private string _description;
        private string[] _tags;
        private bool _featured;
        private bool _requiresFastEffects;
        private bool _requiresEffects;
        private DateTime _dateAdded;

        public SampleDefinition
        (
            string name,
            Type pageType,
            SampleType sampleType,
            SampleCategory sampleArea,
            bool requiresEffects,
            bool requiresFastEffects,
            string imageUrl = MissingThumbnailAsset,
            string description = null,
            string[] tags = null,
            bool featured = false,
            DateTime dateAdded = new DateTime()
        )
        {
            _name = name;
            _pageType = pageType;
            _sampleType = sampleType;
            _sampleCategory = sampleArea;
            _imageUrl = imageUrl;
            _description = description;                  // used when showing more information about a sample, such as for featured samples
            _tags = tags;
            _featured = featured;
            _requiresEffects = requiresEffects;
            _requiresFastEffects = requiresFastEffects;
            _dateAdded = dateAdded;
        }

        public string Name { get { return _name; } }
        public Type Type { get { return _pageType; } }
        public SampleType SampleType { get { return _sampleType; } }
        public SampleCategory SampleCategory { get { return _sampleCategory; } }
        public string DisplayName { get { return _name; } }
        public string ImageUrl { get { return _imageUrl; } }
        public string Description { get { return _description; } }
        public string[] Tags { get { return _tags; } }
        public bool Featured { get { return _featured; } }
        public bool RequiresEffects { get { return _requiresEffects; } }
        public bool RequiresFastEffects { get { return _requiresFastEffects; } }
        public DateTime DateAdded { get { return _dateAdded; } }
    }

    public class SampleDefinitions
    {
        static SampleDefinitions()
        {
            RefreshSampleList();
        }

        static public void RefreshSampleList()
        {
            // Populate the _definitions array only with samples that are supported by the current runtime and hardware
            //
            // For now always display samples even if effects are slow. In the future, we should put a banner saying that
            // this sample is slow on your device, etc.

            var result = from sampleDef in _allDefinitions
                         where
                         //(!sampleDef.RequiresFastEffects || MainPage.AreEffectsFast) &&
                         (!sampleDef.RequiresEffects || MainPage.AreEffectsSupported)
                         select sampleDef;
            _definitions = result.ToList();
        }

        // A filtered list of runtime-supported samples
        static List<SampleDefinition> _definitions = new List<SampleDefinition>();

        // Full list of all definitions
        static SampleDefinition[] _allDefinitions =
        {
            //      StaticSampleName                                                Class                                 SampleType            SampleCategory                      Effects     FastEffects     ThumbnailURL                                                                         StaticSampleDescription                                                   Date Added                              Featured                   Tags
                new SampleDefinition(PropertySets.StaticSampleName,                 typeof(PropertySets),                 SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/ExpressionsAndPropertySets.PNG",        description: PropertySets.StaticSampleDescription,                                                                                                          tags: new string[1]{"ExpressionBuilder"}),
                new SampleDefinition(PointerEnterEffects.StaticSampleName,          typeof(PointerEnterEffects),          SampleType.EndToEnd,  SampleCategory.Material,            true,       false,          "ms-appx:///Assets/SampleThumbnails/PointerEnterExitEffects.PNG",           description: PointerEnterEffects.StaticSampleDescription,                                                                                                   tags: new string[1]{"ExpressionBuilder"}),
                new SampleDefinition(ParallaxingListItems.StaticSampleName,         typeof(ParallaxingListItems),         SampleType.EndToEnd,  SampleCategory.Depth,               false,      false,          "ms-appx:///Assets/SampleThumbnails/ParallaxingListviewItem.PNG",           description: ParallaxingListItems.StaticSampleDescription),
                new SampleDefinition(Z_OrderScrolling.StaticSampleName,             typeof(Z_OrderScrolling),             SampleType.EndToEnd,  SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/Z-OrderScrolling.PNG",                  description: Z_OrderScrolling.StaticSampleDescription,                                                                                                      tags: new string[2]{"ExpressionBuilder", "ZOrder"}),
                new SampleDefinition(BasicXamlInterop.StaticSampleName,             typeof(BasicXamlInterop),             SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/BasicXAMLInterop.PNG",                  description: BasicXamlInterop.StaticSampleDescription),
                new SampleDefinition(ZoomWithPerspective.StaticSampleName,          typeof(ZoomWithPerspective),          SampleType.EndToEnd,  SampleCategory.Depth,               false,      false,          "ms-appx:///Assets/SampleThumbnails/ZoomWithPerspective.PNG",               description: ZoomWithPerspective.StaticSampleDescription),
                new SampleDefinition(BasicLayoutAndTransforms.StaticSampleName,     typeof(BasicLayoutAndTransforms),     SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/BasicLayoutAndTransitions.PNG",         description: BasicLayoutAndTransforms.StaticSampleDescription),
                new SampleDefinition(Perspective.StaticSampleName,                  typeof(Perspective),                  SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/Perspective.png",                       description: Perspective.StaticSampleDescription),
                new SampleDefinition(ColorBloomTransition.StaticSampleName,         typeof(ColorBloomTransition),         SampleType.EndToEnd,  SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/ColorBloom.jpg",                        description: ColorBloomTransition.StaticSampleDescription),
                new SampleDefinition(ColorSlideTransition.StaticSampleName,         typeof(ColorSlideTransition),         SampleType.EndToEnd,  SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/ColorSlide.png",                        description: ColorSlideTransition.StaticSampleDescription),
                new SampleDefinition(FlipToReveal.StaticSampleName,                 typeof(FlipToReveal),                 SampleType.EndToEnd,  SampleCategory.Depth,               false,      false,          "ms-appx:///Assets/SampleThumbnails/FlipToReveal.png",                      description: FlipToReveal.StaticSampleDescription),
                new SampleDefinition(ConnectedAnimationShell.StaticSampleName,      typeof(ConnectedAnimationShell),      SampleType.EndToEnd,  SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/ContinuityAnimations.jpg",              description: ConnectedAnimationShell.StaticSampleDescription,                                                                featured: true,                 tags: new string[1]{"ExpressionBuilder"}),
                new SampleDefinition(BackDropSample.StaticSampleName,               typeof(BackDropSample),               SampleType.Reference, SampleCategory.APIReference,        true,       true,           "ms-appx:///Assets/SampleThumbnails/BackDropControlSample.PNG",             description: BackDropSample.StaticSampleDescription),
                new SampleDefinition(Gears.StaticSampleName,                        typeof(Gears),                        SampleType.Reference,  SampleCategory.APIReference,       false,      false,          "ms-appx:///Assets/SampleThumbnails/Gears.PNG",                             description: Gears.StaticSampleDescription),
                new SampleDefinition(ImplicitAnimationTransformer.StaticSampleName, typeof(ImplicitAnimationTransformer), SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/ImplicitAnimations.PNG",                description: ImplicitAnimationTransformer.StaticSampleDescription),
                //new SampleDefinition(VideoPlayground.StaticSampleName,              typeof(VideoPlayground),              SampleType.Reference, SampleCategory.APIReference,        true,       true,           "ms-appx:///Assets/SampleThumbnails/VideoPlayground.PNG",                   description: VideoPlayground.StaticSampleDescription,                   sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(Photos.StaticSampleName,                       typeof(Photos),                       SampleType.EndToEnd,  SampleCategory.Scale,               false,      false,          "ms-appx:///Assets/SampleThumbnails/LayoutAnimations.PNG",                  description: Photos.StaticSampleDescription),
                new SampleDefinition(TreeEffects.StaticSampleName,                  typeof(TreeEffects),                  SampleType.Reference, SampleCategory.Depth,               true,       true,           "ms-appx:///Assets/SampleThumbnails/TreeEffect.PNG",                        description: TreeEffects.StaticSampleDescription),
                new SampleDefinition(LayerVisualAnd3DTransform.StaticSampleName,    typeof(LayerVisualAnd3DTransform),    SampleType.EndToEnd,  SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/LayerVisualSample.PNG",                 description: LayerVisualAnd3DTransform.StaticSampleDescription),
                new SampleDefinition(ForegroundFocusEffects.StaticSampleName,       typeof(ForegroundFocusEffects),       SampleType.EndToEnd,  SampleCategory.Depth,               true,       true,           "ms-appx:///Assets/SampleThumbnails/ForegroundFocusEffects.PNG",            description: ForegroundFocusEffects.StaticSampleDescription),
                new SampleDefinition(PhotoViewer.StaticSampleName,                  typeof(PhotoViewer),                  SampleType.EndToEnd,  SampleCategory.Motion,              true,       false,          "ms-appx:///Assets/SampleThumbnails/PhotoPopupViewer.PNG",                  description: PhotoViewer.StaticSampleDescription),
                new SampleDefinition(ThumbnailLighting.StaticSampleName,            typeof(ThumbnailLighting),            SampleType.EndToEnd,  SampleCategory.Light,               true,       true,           "ms-appx:///Assets/SampleThumbnails/ThumbnailLighting.jpg",                 description: ThumbnailLighting.StaticSampleDescription),
                new SampleDefinition(Curtain.StaticSampleName,                      typeof(Curtain),                      SampleType.Reference, SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/Curtain.PNG",                           description: Curtain.StaticSampleDescription,                                                                                                                 tags: new string[1]{"ExpressionBuilder"}),
                new SampleDefinition(PullToAnimate.StaticSampleName,                typeof(PullToAnimate),                SampleType.EndToEnd,  SampleCategory.Depth,               true,       true,           "ms-appx:///Assets/SampleThumbnails/PullToAnimate.jpg",                     description: PullToAnimate.StaticSampleDescription,                                                                      featured: true),
                new SampleDefinition(NowPlaying.StaticSampleName,                   typeof(NowPlaying),                   SampleType.EndToEnd,  SampleCategory.APIReference,        true,       true,           "ms-appx:///Assets/SampleThumbnails/NowPlaying.PNG",                        description: NowPlaying.StaticSampleDescription),
                new SampleDefinition(ShadowPlayground.StaticSampleName,             typeof(ShadowPlayground),             SampleType.Reference, SampleCategory.APIReference,        true,       true,           "ms-appx:///Assets/SampleThumbnails/ShadowPlayground.jpg",                  description: ShadowPlayground.StaticSampleDescription),
                new SampleDefinition(ShadowInterop.StaticSampleName,                typeof(ShadowInterop),                SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/ShadowInterop.PNG",                     description: ShadowInterop.StaticSampleDescription),
                new SampleDefinition(TextShimmer.StaticSampleName,                  typeof(TextShimmer),                  SampleType.EndToEnd,  SampleCategory.Light,               true,       true,           "ms-appx:///Assets/SampleThumbnails/TextShimmer.png",                       description: TextShimmer.StaticSampleDescription,                                                                        featured: true),
                new SampleDefinition(NineGridResizing.StaticSampleName,             typeof(NineGridResizing),             SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/NineGridResizing.PNG",                  description: NineGridResizing.StaticSampleDescription),
                new SampleDefinition(LayerDepth.StaticSampleName,                   typeof(LayerDepth),                   SampleType.EndToEnd,  SampleCategory.Depth,               true,       true,           "ms-appx:///Assets/SampleThumbnails/LayerDepth.PNG",                        description: LayerDepth.StaticSampleDescription),
                new SampleDefinition(LightSphere.StaticSampleName,                  typeof(LightSphere),                  SampleType.Reference, SampleCategory.Light,               true,       true,           "ms-appx:///Assets/SampleThumbnails/LightSpheres.PNG",                      description: LightSphere.StaticSampleDescription),
                //new SampleDefinition(SwipeScroller.StaticSampleName,                typeof(SwipeScroller),                SampleType.EndToEnd,  SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/SwipeScroller.PNG",                     description: SwipeScroller.StaticSampleDescription,                     sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393,         dateAdded: new DateTime(2017,03,05),    featured: true),
                new SampleDefinition(ShyHeader.StaticSampleName,                    typeof(ShyHeader),                    SampleType.EndToEnd,  SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/ShyHeader.PNG",                         description: ShyHeader.StaticSampleDescription,                                   dateAdded: new DateTime(2017,04,25),    featured: true),
                new SampleDefinition(BlurPlayground.StaticSampleName,               typeof(BlurPlayground),               SampleType.Reference, SampleCategory.APIReference,        true,       true,           "ms-appx:///Assets/SampleThumbnails/BlurPlayground.PNG",                    description: BackDropSample.StaticSampleDescription),
                new SampleDefinition(Interactions3D.StaticSampleName,               typeof(Interactions3D),               SampleType.EndToEnd,  SampleCategory.Depth,               false,      false,          "ms-appx:///Assets/SampleThumbnails/Interaction3D.PNG",                     description: Interactions3D.StaticSampleDescription,                                                                      featured: true,                    tags: new string[2]{"3d", "InteractionTracker"}),
                new SampleDefinition(BorderPlayground.StaticSampleName,             typeof(BorderPlayground),             SampleType.Reference, SampleCategory.APIReference,        false,      true,           "ms-appx:///Assets/SampleThumbnails/BorderEffects.PNG",                     description: BorderPlayground.StaticSampleDescription,                            dateAdded: new DateTime(2017,02,08)),
                new SampleDefinition(CompCapabilities.StaticSampleName,             typeof(CompCapabilities),             SampleType.Reference, SampleCategory.Scale,               false,      false,          "ms-appx:///Assets/SampleThumbnails/CompositionCapabilities.PNG",           description: CompCapabilities.StaticSampleDescription,                            dateAdded: new DateTime(2017,02,08),    featured: true),
                //new SampleDefinition(TransparentWindow.StaticSampleName,            typeof(TransparentWindow),            SampleType.EndToEnd,  SampleCategory.APIReference,        true,       true,           "ms-appx:///Assets/SampleThumbnails/TransparentWindow.PNG",                 description: TransparentWindow.StaticSampleDescription,                 sdkVersion: RuntimeSupportedSDKs.SDKVERSION._15063,         dateAdded: new DateTime(2017,02,08),    featured: true),
                new SampleDefinition(NavigationFlow.StaticSampleName,               typeof(NavigationFlow),               SampleType.EndToEnd,  SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/NavigationFlow.PNG",                    description: NavigationFlow.StaticSampleDescription,                              dateAdded: new DateTime(2017,02,08),    featured: true),
                //new SampleDefinition(ShowHideImplicitWebview.StaticSampleName,      typeof(ShowHideImplicitWebview),      SampleType.EndToEnd,  SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/ShowHideImplicitWebview.PNG",           description: ShowHideImplicitWebview.StaticSampleDescription,           sdkVersion: RuntimeSupportedSDKs.SDKVERSION._15063,         dateAdded: new DateTime(2017,02,28)),
                new SampleDefinition(ShadowsAdvanced.StaticSampleName,              typeof(ShadowsAdvanced),              SampleType.Reference, SampleCategory.Depth,               false,      false,          "ms-appx:///Assets/SampleThumbnails/AdvancedShadows.PNG",                   description: ShadowsAdvanced.StaticSampleDescription,                                                                     featured: true),
                new SampleDefinition(OffsetStompingFix.StaticSampleName,            typeof(OffsetStompingFix),            SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/OffsetStompingFix.PNG",                 description: OffsetStompingFix.StaticSampleDescription,                           dateAdded: new DateTime(2017,04,18),    featured: true),
                new SampleDefinition(PointerRotate.StaticSampleName,                typeof(PointerRotate),                SampleType.Reference, SampleCategory.Depth,               false,      false,          "ms-appx:///Assets/SampleThumbnails/PointerRotate.PNG",                     description: PointerRotate.StaticSampleDescription,                               dateAdded: new DateTime(2017,04,25),    featured: true),
                new SampleDefinition(BrushInterop.StaticSampleName,                 typeof(BrushInterop),                 SampleType.Reference, SampleCategory.APIReference,        true,       true,           "ms-appx:///Assets/SampleThumbnails/BrushInterop.PNG",                      description: BrushInterop.StaticSampleDescription,                                dateAdded: new DateTime(2017,06,21),    featured: true),
                new SampleDefinition(LightInterop.StaticSampleName,                 typeof(LightInterop),                 SampleType.Reference, SampleCategory.Material,            true,       true,           "ms-appx:///Assets/SampleThumbnails/LightInterop.PNG",                      description: LightInterop.StaticSampleDescription,                                dateAdded: new DateTime(2017,06,21),    featured: true),
                new SampleDefinition(PullToRefresh.StaticSampleName,                typeof(PullToRefresh),                SampleType.EndToEnd,  SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/PullToRefresh.PNG",                     description: PullToRefresh.StaticSampleDescription,                               dateAdded: new DateTime(2017,09,12)),
                new SampleDefinition(SpringyImage.StaticSampleName,                 typeof(SpringyImage),                 SampleType.Reference, SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/SpringyImage.PNG",                      description: SpringyImage.StaticSampleDescription,                                dateAdded: new DateTime(2017,08,7),                                        tags: new string[1]{"ExpressionBuilder"}),
                new SampleDefinition(LinearGradients.StaticSampleName,              typeof(LinearGradients),              SampleType.Reference, SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/LinearGradients.PNG",                   description: LinearGradients.StaticSampleDescription,                             dateAdded: new DateTime(2019,02,27)),
                new SampleDefinition(AnimationControl.StaticSampleName,             typeof(AnimationControl),             SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/AnimationController.PNG",               description: AnimationControl.StaticSampleDescription,                            dateAdded: new DateTime(2018,12,3)),
                new SampleDefinition(Lottie.StaticSampleName,                       typeof(Lottie),                       SampleType.Reference, SampleCategory.Motion,              false,      false,          "ms-appx:///Assets/SampleThumbnails/Lottie.png",                            description: AnimationControl.StaticSampleDescription,                            dateAdded: new DateTime(2020,07,29)),
                //new SampleDefinition(SceneNodePlayground.StaticSampleName,          typeof(SceneNodePlayground),          SampleType.Reference, SampleCategory.APIReference,        false,      false,          "ms-appx:///Assets/SampleThumbnails/SceneNodePlayground.PNG",               description: SceneNodePlayground.StaticSampleDescription,                         dateAdded: new DateTime(2020,07,29)),
                new SampleDefinition(GestureRecognizer.StaticSampleName,            typeof(GestureRecognizer),            SampleType.Reference, SampleCategory.Input,               false,      false,          "ms-appx:///Assets/SampleThumbnails/GestureRecognizer.png",                 description: GestureRecognizer.StaticSampleDescription,                           dateAdded: new DateTime(2021,04,6),     featured: true),
                new SampleDefinition(GestureRecognizerManipulation.StaticSampleName,typeof(GestureRecognizerManipulation),SampleType.Reference, SampleCategory.Input,               false,      false,          "ms-appx:///Assets/SampleThumbnails/Manipulation.png",                      description: GestureRecognizerManipulation.StaticSampleDescription,               dateAdded: new DateTime(2021,04,15)),
                new SampleDefinition(InputCursor.StaticSampleName,                  typeof(InputCursor),                  SampleType.Reference, SampleCategory.Input,               false,      false,          "ms-appx:///Assets/SampleThumbnails/CoreCursor.png",                        description: InputCursor.StaticSampleDescription,                                 dateAdded: new DateTime(2021,10,26),    featured: true),
        };

        public static List<SampleDefinition> Definitions
        {
            get { return _definitions; }
        }
    }
}