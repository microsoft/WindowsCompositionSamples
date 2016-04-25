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
        Visuals
    }

    public class SampleDefinition
    {
        private string      _name;
        private Type        _pageType;
        private SampleType  _sampleType;
        private SampleCategory  _sampleCategory;


        public SampleDefinition(string name, Type pageType, SampleType sampleType, SampleCategory sampleArea)
        {
            _name           = name;
            _pageType       = pageType;
            _sampleType     = sampleType;
            _sampleCategory = sampleArea;
        }

        public string Name { get { return _name; } }
        public Type Type { get { return _pageType; } }
        public SampleType SampleType {  get { return _sampleType; } }
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
    }

    public class SampleDefinitions
    {
        static SampleDefinition[] definitions =
        {
#if SDKVERSION_INSIDER
            new SampleDefinition(BackDropSample.StaticSampleName,               typeof(BackDropSample),                 SampleType.Reference, SampleCategory.Effects),
            new SampleDefinition(Curtain.StaticSampleName,                      typeof(Curtain),                        SampleType.Reference, SampleCategory.ExpressionAnimations),
            new SampleDefinition(ForegroundFocusEffects.StaticSampleName,       typeof(ForegroundFocusEffects),         SampleType.EndToEnd,  SampleCategory.Effects),
            new SampleDefinition(Gears.StaticSampleName,                        typeof(Gears),                          SampleType.EndToEnd,  SampleCategory.ExpressionAnimations),
            new SampleDefinition(ImageLightingPlayground.StaticSampleName,      typeof(ImageLightingPlayground),        SampleType.Reference, SampleCategory.Effects),
            new SampleDefinition(ImplicitAnimationTransformer.StaticSampleName, typeof(ImplicitAnimationTransformer),   SampleType.Reference, SampleCategory.Animations),
            new SampleDefinition(NowPlaying.StaticSampleName,                   typeof(NowPlaying),                     SampleType.EndToEnd,  SampleCategory.Effects),
            new SampleDefinition(PhotoViewer.StaticSampleName,                  typeof(PhotoViewer),                    SampleType.EndToEnd,  SampleCategory.Effects),
            new SampleDefinition(PullToAnimate.StaticSampleName,                typeof(PullToAnimate),                  SampleType.EndToEnd,  SampleCategory.ExpressionAnimations),
            new SampleDefinition(ShadowPlayground.StaticSampleName,             typeof(ShadowPlayground),               SampleType.Reference, SampleCategory.Visuals),
            new SampleDefinition(TextShimmer.StaticSampleName,                  typeof(TextShimmer),                    SampleType.EndToEnd,  SampleCategory.Effects),
            new SampleDefinition(ThumbnailLighting.StaticSampleName,            typeof(ThumbnailLighting),              SampleType.EndToEnd,  SampleCategory.Effects),
#endif

#if SDKVERSION_10586
            new SampleDefinition(BasicXamlInterop.StaticSampleName,             typeof(BasicXamlInterop),       SampleType.Reference, SampleCategory.Visuals),
            new SampleDefinition(Continuity.StaticSampleName,                   typeof(Continuity),             SampleType.EndToEnd,  SampleCategory.Transitions),
            new SampleDefinition(ParallaxingListItems.StaticSampleName,         typeof(ParallaxingListItems),   SampleType.EndToEnd,  SampleCategory.ExpressionAnimations),
            new SampleDefinition(Perspective.StaticSampleName,                  typeof(Perspective),            SampleType.Reference, SampleCategory.Visuals),
            new SampleDefinition(PointerEnterEffects.StaticSampleName,          typeof(PointerEnterEffects),    SampleType.EndToEnd,  SampleCategory.Effects),
            new SampleDefinition(PropertySets.StaticSampleName,                 typeof(PropertySets),           SampleType.Reference, SampleCategory.ExpressionAnimations),
            new SampleDefinition(ColorBloomTransition.StaticSampleName,         typeof(ColorBloomTransition),   SampleType.EndToEnd,  SampleCategory.Transitions),
            new SampleDefinition(ColorSlideTransition.StaticSampleName,         typeof(ColorSlideTransition),   SampleType.EndToEnd,  SampleCategory.Transitions),
            new SampleDefinition(ZoomWithPerspective.StaticSampleName,          typeof(ZoomWithPerspective),    SampleType.EndToEnd,  SampleCategory.Visuals),
            new SampleDefinition(FlipToReveal.StaticSampleName,                 typeof(FlipToReveal),           SampleType.EndToEnd,  SampleCategory.Transitions),
            new SampleDefinition(Z_OrderScrolling.StaticSampleName,             typeof(Z_OrderScrolling),       SampleType.EndToEnd,  SampleCategory.ExpressionAnimations),
#endif
        };

        public static SampleDefinition[] Definitions { get { return definitions; } }
    }
}
