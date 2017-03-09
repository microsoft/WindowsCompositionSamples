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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls;

namespace CompositionSampleGallery
{

    public enum SampleType
    {
        Reference,
        EndToEnd
    };

    public enum SampleCategory
    {
        Conceptual,
        NaturalMotion,
        ContextualUI,
        SeamlessTransitions,
        RealWorldUI,
        DynamicHumanInteractions,
    }

    public class SampleDefinition
    {
        private const string MissingThumbnailAsset = "ms-appx:///Assets/MissingThumbnail.png";
        private string _name;
        private Type _pageType;
        private SampleType _sampleType;
        private SampleCategory _sampleCategory;
        private string _imageUrl;
        private string _description;
        private bool _featured;
        private bool _requiresFastEffects;
        private bool _requiresEffects;
        private DateTime _dateAdded;
        private RuntimeSupportedSDKs.SDKVERSION _sdkVersion;

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
            bool featured = false, 
            DateTime dateAdded = new DateTime(), 
            RuntimeSupportedSDKs.SDKVERSION sdkVersion = RuntimeSupportedSDKs.SDKVERSION._10586
        )
        {
            _name = name;
            _pageType = pageType;
            _sampleType = sampleType;
            _sampleCategory = sampleArea;
            _imageUrl = imageUrl;
            _description = description;                  // used when showing more information about a sample, such as for featured samples
            _featured = featured;
            _requiresEffects = requiresEffects;
            _requiresFastEffects = requiresFastEffects;
            _sdkVersion = sdkVersion;
            _dateAdded = dateAdded;
        }

        public string Name { get { return _name; } }
        public Type Type { get { return _pageType; } }
        public SampleType SampleType { get { return _sampleType; } }
        public SampleCategory SampleCategory { get { return _sampleCategory; } }
        public string DisplayName { get { return _name; } }
        public string ImageUrl { get { return _imageUrl; } }
        public string Description { get { return _description; } }
        public bool Featured { get { return _featured; } }
        public bool RequiresEffects { get { return _requiresEffects; } }
        public bool RequiresFastEffects { get { return _requiresFastEffects; } }
        public DateTime DateAdded { get { return _dateAdded; } }
        public RuntimeSupportedSDKs.SDKVERSION SDKVersion { get { return _sdkVersion; } }
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
            var result = from sampleDef in _allDefinitions
                         where MainPage.RuntimeCapabilities.AllSupportedSdkVersions.Contains(sampleDef.SDKVersion) &&
                         (!sampleDef.RequiresFastEffects || MainPage.AreEffectsFast) &&
                         (!sampleDef.RequiresEffects || MainPage.AreEffectsSupported)
                         select sampleDef;
            _definitions = result.ToList();
        }

        // A filtered list of runtime-supported samples
        static List<SampleDefinition> _definitions = new List<SampleDefinition>();

        // Full list of all definitions
        static SampleDefinition[] _allDefinitions =
        {
#if SDKVERSION_10586
                new SampleDefinition(PropertySets.StaticSampleName,                 typeof(PropertySets),                 SampleType.Reference, SampleCategory.Conceptual,                       false, false, "ms-appx:///Assets/SampleThumbnails/ExpressionsAndPropertySets.PNG"),
                new SampleDefinition(PointerEnterEffects.StaticSampleName,          typeof(PointerEnterEffects),          SampleType.EndToEnd,  SampleCategory.ContextualUI,                     true,  false, "ms-appx:///Assets/SampleThumbnails/PointerEnterExitEffects.PNG"),
                new SampleDefinition(ParallaxingListItems.StaticSampleName,         typeof(ParallaxingListItems),         SampleType.EndToEnd,  SampleCategory.NaturalMotion,                    false, false, "ms-appx:///Assets/SampleThumbnails/ParallaxingListviewItem.PNG"),
                new SampleDefinition(Z_OrderScrolling.StaticSampleName,             typeof(Z_OrderScrolling),             SampleType.EndToEnd,  SampleCategory.NaturalMotion,                    false, false, "ms-appx:///Assets/SampleThumbnails/Z-OrderScrolling.PNG",              null, true),
                new SampleDefinition(BasicXamlInterop.StaticSampleName,             typeof(BasicXamlInterop),             SampleType.Reference, SampleCategory.RealWorldUI,                      false, false, "ms-appx:///Assets/SampleThumbnails/BasicXAMLInterop.PNG"),
                new SampleDefinition(ZoomWithPerspective.StaticSampleName,          typeof(ZoomWithPerspective),          SampleType.EndToEnd,  SampleCategory.RealWorldUI,                      false, false, "ms-appx:///Assets/SampleThumbnails/ZoomWithPerspective.PNG",           "Demonstrates how to apply and animate a perspective transform."),
                new SampleDefinition(BasicLayoutAndTransforms.StaticSampleName,     typeof(BasicLayoutAndTransforms),     SampleType.Reference, SampleCategory.RealWorldUI,                      false, false, "ms-appx:///Assets/SampleThumbnails/BasicLayoutAndTransitions.PNG"),
                new SampleDefinition(Perspective.StaticSampleName,                  typeof(Perspective),                  SampleType.Reference, SampleCategory.RealWorldUI,                      false, false, "ms-appx:///Assets/SampleThumbnails/Perspective.png"),
                new SampleDefinition(ColorBloomTransition.StaticSampleName,         typeof(ColorBloomTransition),         SampleType.EndToEnd,  SampleCategory.SeamlessTransitions,              false, false, "ms-appx:///Assets/SampleThumbnails/ColorBloom.jpg",                    "Demonstrates how to use Visuals and Animations to create a color bloom effect during page or state transitions."),
                new SampleDefinition(ColorSlideTransition.StaticSampleName,         typeof(ColorSlideTransition),         SampleType.EndToEnd,  SampleCategory.SeamlessTransitions,              false, false, "ms-appx:///Assets/SampleThumbnails/ColorSlide.png"),
                new SampleDefinition(FlipToReveal.StaticSampleName,                 typeof(FlipToReveal),                 SampleType.EndToEnd,  SampleCategory.SeamlessTransitions,              false, false, "ms-appx:///Assets/SampleThumbnails/FlipToReveal.png"),
                new SampleDefinition(ConnectedAnimationShell.StaticSampleName,      typeof(ConnectedAnimationShell),      SampleType.EndToEnd,  SampleCategory.SeamlessTransitions,              false, false, "ms-appx:///Assets/SampleThumbnails/ContinuityAnimations.jpg",          "Connected animations communicate context across page navigations. Click on one of the thumbnails and see it transition continuously across from one page navigate to another.", true),
#endif

#if SDKVERSION_14393
                new SampleDefinition(BackDropSample.StaticSampleName,               typeof(BackDropSample),               SampleType.Reference, SampleCategory.Conceptual,                 true,  true,  "ms-appx:///Assets/SampleThumbnails/BackDropControlSample.PNG",         sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(Gears.StaticSampleName,                        typeof(Gears),                        SampleType.EndToEnd,  SampleCategory.Conceptual,                 false, false, "ms-appx:///Assets/SampleThumbnails/Gears.PNG",                         sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(ImplicitAnimationTransformer.StaticSampleName, typeof(ImplicitAnimationTransformer), SampleType.Reference, SampleCategory.Conceptual,                 false, false, "ms-appx:///Assets/SampleThumbnails/ImplicitAnimations.PNG",            sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(BlurPlayground.StaticSampleName,               typeof(BlurPlayground),               SampleType.Reference, SampleCategory.Conceptual,                 true,  true,  "ms-appx:///Assets/SampleThumbnails/BlurPlayground.PNG",                sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(VideoPlayground.StaticSampleName,              typeof(VideoPlayground),              SampleType.Reference, SampleCategory.Conceptual,                 true,  true,  "ms-appx:///Assets/SampleThumbnails/VideoPlayground.PNG",               sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(Photos.StaticSampleName,                       typeof(Photos),                       SampleType.EndToEnd,  SampleCategory.Conceptual,                 false, false, "ms-appx:///Assets/SampleThumbnails/LayoutAnimations.PNG",              null, true,  sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(TreeEffects.StaticSampleName,                  typeof(TreeEffects),                  SampleType.Reference, SampleCategory.Conceptual,                 true,  true,  "ms-appx:///Assets/SampleThumbnails/TreeEffect.PNG",                    sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(LayerVisualAnd3DTransform.StaticSampleName,    typeof(LayerVisualAnd3DTransform),    SampleType.EndToEnd,  SampleCategory.Conceptual,                 false, false, "ms-appx:///Assets/SampleThumbnails/LayerVisualSample.PNG",             sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(ForegroundFocusEffects.StaticSampleName,       typeof(ForegroundFocusEffects),       SampleType.EndToEnd,  SampleCategory.ContextualUI,               true,  true,  "ms-appx:///Assets/SampleThumbnails/ForegroundFocusEffects.PNG",        sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(PhotoViewer.StaticSampleName,                  typeof(PhotoViewer),                  SampleType.EndToEnd,  SampleCategory.ContextualUI,               true,  false, "ms-appx:///Assets/SampleThumbnails/PhotoPopupViewer.PNG",              null, true,  sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(ThumbnailLighting.StaticSampleName,            typeof(ThumbnailLighting),            SampleType.EndToEnd,  SampleCategory.ContextualUI,               true,  true,  "ms-appx:///Assets/SampleThumbnails/ThumbnailLighting.jpg",             "Demonstrates how to apply Image Lighting to ListView Items.  Switch between different combinations of light types(point, spot, distant) and lighting properties such as diffuse and specular.",  sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(Curtain.StaticSampleName,                      typeof(Curtain),                      SampleType.Reference, SampleCategory.DynamicHumanInteractions,   false, false, "ms-appx:///Assets/SampleThumbnails/Curtain.PNG",                       null, true,  sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(PullToAnimate.StaticSampleName,                typeof(PullToAnimate),                SampleType.EndToEnd,  SampleCategory.DynamicHumanInteractions,   true,  true,  "ms-appx:///Assets/SampleThumbnails/PullToAnimate.jpg",                 "Demonstrates the use of InteractionTracker to drive smooth animations of effect properties.",  sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(Interactions3D.StaticSampleName,               typeof(Interactions3D),               SampleType.EndToEnd,  SampleCategory.DynamicHumanInteractions,   false, false, "ms-appx:///Assets/SampleThumbnails/Interaction3D.PNG",                 sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(NowPlaying.StaticSampleName,                   typeof(NowPlaying),                   SampleType.EndToEnd,  SampleCategory.RealWorldUI,                true,  true,  "ms-appx:///Assets/SampleThumbnails/NowPlaying.PNG",                    sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(ShadowPlayground.StaticSampleName,             typeof(ShadowPlayground),             SampleType.Reference, SampleCategory.RealWorldUI,                true,  true,  "ms-appx:///Assets/SampleThumbnails/ShadowPlayground.jpg",              "Experiment with the available properties on the DropShadow object to create interesting shadows.",  sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(ShadowInterop.StaticSampleName,                typeof(ShadowInterop),                SampleType.Reference, SampleCategory.RealWorldUI,                false, false, "ms-appx:///Assets/SampleThumbnails/ShadowInterop.PNG",                 sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(TextShimmer.StaticSampleName,                  typeof(TextShimmer),                  SampleType.EndToEnd,  SampleCategory.RealWorldUI,                true,  true,  "ms-appx:///Assets/SampleThumbnails/TextShimmer.png",                   null, true,  sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(NineGridResizing.StaticSampleName,             typeof(NineGridResizing),             SampleType.Reference, SampleCategory.RealWorldUI,                false, false, "ms-appx:///Assets/SampleThumbnails/NineGridResizing.PNG",              sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(LayerDepth.StaticSampleName,                   typeof(LayerDepth),                   SampleType.EndToEnd,  SampleCategory.SeamlessTransitions,        true,  true,  "ms-appx:///Assets/SampleThumbnails/LayerDepth.PNG",                    sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(LightSphere.StaticSampleName,                  typeof(LightSphere),                  SampleType.Reference, SampleCategory.RealWorldUI,                true,  true,  imageUrl: "ms-appx:///Assets/SampleThumbnails/LightSpheres.PNG",        sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(ShadowsAdvanced.StaticSampleName,              typeof(ShadowsAdvanced),              SampleType.Reference, SampleCategory.RealWorldUI,                false, false, imageUrl: "ms-appx:///Assets/SampleThumbnails/AdvancedShadows.PNG",     sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
                new SampleDefinition(SwipeScroller.StaticSampleName,                typeof(SwipeScroller),                SampleType.EndToEnd,  SampleCategory.DynamicHumanInteractions,   false, false, dateAdded: new DateTime(2017,03,05),  imageUrl: "ms-appx:///Assets/SampleThumbnails/SwipeScroller.PNG",                 sdkVersion: RuntimeSupportedSDKs.SDKVERSION._14393),
#endif

#if SDKVERSION_INSIDER
                new SampleDefinition(BorderPlayground.StaticSampleName,             typeof(BorderPlayground),             SampleType.Reference, SampleCategory.RealWorldUI,                false, true,   dateAdded: new DateTime(2017,02,08), imageUrl: "ms-appx:///Assets/SampleThumbnails/BorderEffects.PNG",           sdkVersion: RuntimeSupportedSDKs.SDKVERSION._INSIDER),
                new SampleDefinition(CompCapabilities.StaticSampleName,             typeof(CompCapabilities),             SampleType.Reference, SampleCategory.Conceptual,                 false, false,  dateAdded: new DateTime(2017,02,08), imageUrl: "ms-appx:///Assets/SampleThumbnails/CompositionCapabilities.PNG", sdkVersion: RuntimeSupportedSDKs.SDKVERSION._INSIDER),
                new SampleDefinition(TransparentWindow.StaticSampleName,            typeof(TransparentWindow),            SampleType.EndToEnd,  SampleCategory.RealWorldUI,                true,  true,   dateAdded: new DateTime(2017,02,08), imageUrl: "ms-appx:///Assets/SampleThumbnails/TransparentWindow.PNG",       sdkVersion: RuntimeSupportedSDKs.SDKVERSION._INSIDER),
                new SampleDefinition(NavigationFlow.StaticSampleName,               typeof(NavigationFlow),               SampleType.EndToEnd,  SampleCategory.SeamlessTransitions,        false, false,  dateAdded: new DateTime(2017,02,08), imageUrl: "ms-appx:///Assets/SampleThumbnails/NavigationFlow.PNG",          sdkVersion: RuntimeSupportedSDKs.SDKVERSION._INSIDER),
                new SampleDefinition(ShowHideImplicitWebview.StaticSampleName,      typeof(ShowHideImplicitWebview),      SampleType.EndToEnd,  SampleCategory.SeamlessTransitions,        false, false,  dateAdded: new DateTime(2017,02,28), imageUrl: "ms-appx:///Assets/SampleThumbnails/ShowHideImplicitWebview.PNG", sdkVersion: RuntimeSupportedSDKs.SDKVERSION._INSIDER),
#endif
    };

        public static List<SampleDefinition> Definitions
        {
            get { return _definitions; }
        }
    }
}
