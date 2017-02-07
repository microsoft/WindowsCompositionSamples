//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED �AS IS�, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using System;
using System.ComponentModel;

namespace CompositionSampleGallery
{
    public enum SampleType
    {
        Reference,
        EndToEnd
    };

    public enum SampleCategory
    {
        Animations,
        Effects,
        ExpressionAnimations,
        Transitions,
        Visuals,
        Interactions
    }

    public class SampleDefinition
    {
        private string _name;
        private Type _pageType;
        private SampleType _sampleType;
        private SampleCategory _sampleCategory;
        private bool _requiresFastEffects;
        private bool _requiresEffects;

        public SampleDefinition(string name, Type pageType, SampleType sampleType, SampleCategory sampleArea, bool requiresEffects, bool requiresFastEffects)
        {
            _name = name;
            _pageType = pageType;
            _sampleType = sampleType;
            _sampleCategory = sampleArea;
            _requiresEffects = requiresEffects;
            _requiresFastEffects = requiresFastEffects;
        }

        public string Name { get { return _name; } }
        public Type Type { get { return _pageType; } }
        public SampleType SampleType { get { return _sampleType; } }
        public SampleCategory SampleCategory { get { return _sampleCategory; } }
        public string DisplayName
        {
            get
            {
                if (_sampleType == SampleType.Reference)
                {
                    return _name + " (Reference)";
                }
                else
                {
                    return _name;
                }
            }
        }

        public bool RequiresEffects { get { return _requiresEffects; } }
        public bool RequiresFastEffects {  get { return _requiresFastEffects; } }
    }

    public class SampleDefinitions
    {
        static SampleDefinition[] definitions =
        {
#if SDKVERSION_INSIDER
            new SampleDefinition(BorderPlayground.StaticSampleName,             typeof(BorderPlayground),               SampleType.Reference, SampleCategory.Effects),
            new SampleDefinition(CompCapabilities.StaticSampleName,             typeof(CompCapabilities),               SampleType.Reference, SampleCategory.Effects),
            new SampleDefinition(TransparentWindow.StaticSampleName,            typeof(TransparentWindow),              SampleType.EndToEnd,  SampleCategory.Effects),
            new SampleDefinition(NavigationFlow.StaticSampleName,               typeof(NavigationFlow),                 SampleType.EndToEnd,  SampleCategory.Transitions),
#endif

#if SDKVERSION_14393
            new SampleDefinition(BackDropSample.StaticSampleName,               typeof(BackDropSample),                 SampleType.Reference, SampleCategory.Effects,               true,  true),
            new SampleDefinition(Curtain.StaticSampleName,                      typeof(Curtain),                        SampleType.Reference, SampleCategory.Interactions,          false, false),
            new SampleDefinition(ForegroundFocusEffects.StaticSampleName,       typeof(ForegroundFocusEffects),         SampleType.EndToEnd,  SampleCategory.Effects,               true,  true),
            new SampleDefinition(Gears.StaticSampleName,                        typeof(Gears),                          SampleType.EndToEnd,  SampleCategory.ExpressionAnimations,  false, false),
            new SampleDefinition(ImplicitAnimationTransformer.StaticSampleName, typeof(ImplicitAnimationTransformer),   SampleType.Reference, SampleCategory.Animations,            false, false),
            new SampleDefinition(NowPlaying.StaticSampleName,                   typeof(NowPlaying),                     SampleType.EndToEnd,  SampleCategory.Effects,               true,  true),
            new SampleDefinition(PhotoViewer.StaticSampleName,                  typeof(PhotoViewer),                    SampleType.EndToEnd,  SampleCategory.Effects,               true,  false),
            new SampleDefinition(PullToAnimate.StaticSampleName,                typeof(PullToAnimate),                  SampleType.EndToEnd,  SampleCategory.Interactions,          true,  true),
            new SampleDefinition(ShadowPlayground.StaticSampleName,             typeof(ShadowPlayground),               SampleType.Reference, SampleCategory.Visuals,               true,  true),
            new SampleDefinition(ShadowInterop.StaticSampleName,                typeof(ShadowInterop),                  SampleType.Reference, SampleCategory.Visuals,               false, false),
            new SampleDefinition(ShadowsAdvanced.StaticSampleName,              typeof(ShadowsAdvanced),                SampleType.Reference, SampleCategory.Visuals,               false, false),
            new SampleDefinition(TextShimmer.StaticSampleName,                  typeof(TextShimmer),                    SampleType.EndToEnd,  SampleCategory.Effects,               true,  true),
            new SampleDefinition(ThumbnailLighting.StaticSampleName,            typeof(ThumbnailLighting),              SampleType.EndToEnd,  SampleCategory.Effects,               true,  true),
            new SampleDefinition(BlurPlayground.StaticSampleName,               typeof(BlurPlayground),                 SampleType.Reference, SampleCategory.Effects,               true,  true),
            new SampleDefinition(VideoPlayground.StaticSampleName,              typeof(VideoPlayground),                SampleType.Reference, SampleCategory.Effects,               true,  true),
            new SampleDefinition(LayerDepth.StaticSampleName,                   typeof(LayerDepth),                     SampleType.EndToEnd,  SampleCategory.Effects,               true,  true),
            new SampleDefinition(Photos.StaticSampleName,                       typeof(Photos),                         SampleType.EndToEnd,  SampleCategory.Animations,            false, false),
            new SampleDefinition(Interactions3D.StaticSampleName,               typeof(Interactions3D),                 SampleType.EndToEnd,  SampleCategory.Interactions,          false, false),
            new SampleDefinition(TreeEffects.StaticSampleName,                  typeof(TreeEffects),                    SampleType.Reference, SampleCategory.Effects,               true,  true),
            new SampleDefinition(LayerVisualAnd3DTransform.StaticSampleName,    typeof(LayerVisualAnd3DTransform),      SampleType.EndToEnd,  SampleCategory.Effects,               true,  true),
            new SampleDefinition(NineGridResizing.StaticSampleName,             typeof(NineGridResizing),               SampleType.Reference, SampleCategory.Visuals,               false, false),
            new SampleDefinition(LightSphere.StaticSampleName,                  typeof(LightSphere),                    SampleType.Reference, SampleCategory.Effects,               true,  true),
#endif

#if SDKVERSION_10586
            new SampleDefinition(BasicXamlInterop.StaticSampleName,             typeof(BasicXamlInterop),           SampleType.Reference, SampleCategory.Visuals,               false, false),
            new SampleDefinition(ParallaxingListItems.StaticSampleName,         typeof(ParallaxingListItems),       SampleType.EndToEnd,  SampleCategory.ExpressionAnimations,  false, false),
            new SampleDefinition(Perspective.StaticSampleName,                  typeof(Perspective),                SampleType.Reference, SampleCategory.Visuals,               false, false),
            new SampleDefinition(PointerEnterEffects.StaticSampleName,          typeof(PointerEnterEffects),        SampleType.EndToEnd,  SampleCategory.Effects,               true,  false),
            new SampleDefinition(PropertySets.StaticSampleName,                 typeof(PropertySets),               SampleType.Reference, SampleCategory.ExpressionAnimations,  false, false),
            new SampleDefinition(ColorBloomTransition.StaticSampleName,         typeof(ColorBloomTransition),       SampleType.EndToEnd,  SampleCategory.Transitions,           false, false),
            new SampleDefinition(ColorSlideTransition.StaticSampleName,         typeof(ColorSlideTransition),       SampleType.EndToEnd,  SampleCategory.Transitions,           false, false),
            new SampleDefinition(ZoomWithPerspective.StaticSampleName,          typeof(ZoomWithPerspective),        SampleType.EndToEnd,  SampleCategory.Visuals,               false, false),
            new SampleDefinition(FlipToReveal.StaticSampleName,                 typeof(FlipToReveal),               SampleType.EndToEnd,  SampleCategory.Transitions,           false, false),
            new SampleDefinition(Z_OrderScrolling.StaticSampleName,             typeof(Z_OrderScrolling),           SampleType.EndToEnd,  SampleCategory.ExpressionAnimations,  false, false),
            new SampleDefinition(ConnectedAnimationShell.StaticSampleName,      typeof(ConnectedAnimationShell),    SampleType.EndToEnd,  SampleCategory.Transitions,           false, false),
            new SampleDefinition(BasicLayoutAndTransforms.StaticSampleName,     typeof(BasicLayoutAndTransforms),   SampleType.Reference, SampleCategory.Visuals,               false, false),
#endif
        };

        public static SampleDefinition[] Definitions { get { return definitions; } }
    }
}
