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


namespace CompositionSampleGallery
{
    public sealed partial class BrushInterop : SamplePage
    {
        public BrushInterop()
        {
            this.InitializeComponent();
        }
        
        public static string        StaticSampleName => "Brush Interop"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Paint XAML UIElements with CompositionBrushes (and load Surfaces from XAML to CompositionBrushes): Grids, Text, and Shapes painted with EffectBrushes"; 
        public override string      SampleDescription => StaticSampleDescription;
        public override string      SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868945";
    }
}
