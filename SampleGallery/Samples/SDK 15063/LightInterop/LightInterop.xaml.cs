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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CompositionSampleGallery.Samples.LightInterop;


namespace CompositionSampleGallery
{

    public sealed partial class LightInterop : SamplePage
    {
        public LightInterop()
        {
            this.InitializeComponent();

            // Target Grid with lights in code behind because SDK MinVersion > 14393 is needed for <Grid.Ligts> in markup (see .xaml file for comments)
            BackdropGrid.Lights.Add(new HoverLight());
            BackdropGrid.Lights.Add(new AmbLight());
        }

        public static string    StaticSampleName => "Light Interop"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Use XamlLights and XamlCompositionBrushes to create Lights and Materials in XAML"; 
        public override string  SampleDescription => StaticSampleDescription;
        public override string  SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868948";
    }
}
