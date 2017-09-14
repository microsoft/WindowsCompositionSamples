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
    public sealed partial class SampleTemplate : SamplePage
    {
        public SampleTemplate()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName => "Sample Template Page"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Put your description here"; 
        public override string      SampleDescription => StaticSampleDescription; 
    }
}
