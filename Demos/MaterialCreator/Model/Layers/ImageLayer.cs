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

using Microsoft.Graphics.Canvas.Effects;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MaterialCreator
{

    [DataContract]
    public class ImageLayer : Layer
    {
        protected StorageFile _file;
        protected ManagedSurface _surface;
        protected CompositionSurfaceBrush _brush;
        protected CompositionNineGridBrush _nineGrid;
        protected List<LayerProperty> _properties;
        protected EdgeMode _edgeMode;

        public ImageLayer() : base()
        {
            StreamingContext context;
            Initialize(context);
        }

        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            _type = LayerType.Image;
            _brush = Window.Current.Compositor.CreateSurfaceBrush();
            _nineGrid = Window.Current.Compositor.CreateNineGridBrush();
            _inputEffect = new BorderEffect()
            {
                Name = _id,
                Source = new CompositionEffectSourceParameter(_id + "Image"),
            };

            _properties = new List<LayerProperty>();
            _properties.Add(new LayerProperty("Offset", _brush));
            _properties.Add(new LayerProperty("Scale", _brush));
            _properties.Add(new LayerProperty("RotationAngleInDegrees", _brush));
            _properties.Add(new LayerProperty("AnchorPoint", _brush));
            _properties.Add(new LayerProperty("Stretch", _brush));
            _properties.Add(new LayerProperty("HorizontalAlignmentRatio", _brush));
            _properties.Add(new LayerProperty("VerticalAlignmentRatio", _brush));
            _properties.Add(new LayerProperty("ExtendX", _inputEffect));
            _properties.Add(new LayerProperty("ExtendY", _inputEffect));
            _properties.Add(new LayerProperty("LeftInset", _nineGrid));
            _properties.Add(new LayerProperty("TopInset", _nineGrid));
            _properties.Add(new LayerProperty("RightInset", _nineGrid));
            _properties.Add(new LayerProperty("BottomInset", _nineGrid));
            _properties.Add(new LayerProperty("LeftInsetScale", _nineGrid));
            _properties.Add(new LayerProperty("TopInsetScale", _nineGrid));
            _properties.Add(new LayerProperty("RightInsetScale", _nineGrid));
            _properties.Add(new LayerProperty("BottomInsetScale", _nineGrid));
            _properties.Add(new LayerProperty("IsCenterHollow", _nineGrid));
        }

        public async override Task LoadResources()
        {
            File = await LoadFromFilePath(FilePath);
        }

        private async Task<StorageFile> LoadFromFilePath(string filePath)
        {
            if (filePath == null)
            {
                return null;
            }

            try
            {
                return await StorageFile.GetFileFromPathAsync(filePath);
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is UnauthorizedAccessException)
                {
                    // Prompt the user about the file that can't be accessed
                    ContentDialog dialog = new ContentDialog()
                    {
                        Title = "Cannot find/access file",
                        Content = String.Format("Unable to find or access {0}.  Please specify file location.", filePath),
                        PrimaryButtonText = "OK",
                        IsSecondaryButtonEnabled = false,
                    };

                    await dialog.ShowAsync();

                    // Launch the picker
                    FileOpenPicker openPicker = new FileOpenPicker();
                    openPicker.ViewMode = PickerViewMode.Thumbnail;
                    openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                    openPicker.FileTypeFilter.Add(".jpg");
                    openPicker.FileTypeFilter.Add(".jpeg");
                    openPicker.FileTypeFilter.Add(".png");
                    return await openPicker.PickSingleFileAsync();
                }
                else
                {
                    throw e;
                }
            }
        }


        [DataMember]
        public string FilePath { get; set; }

        public virtual void SetProperty(string property, object value)
        {
            object[] targets = new object[] { _brush, _inputEffect, _nineGrid };

            bool found = SetPropertyHelper(targets, property, value);
            Debug.Assert(found);
        }

        protected bool SetPropertyHelper(object[] targets, string property, object value)
        {
            bool propertyFound = false;

            foreach (object target in targets)
            {
                PropertyInfo info = target.GetType().GetProperties().FirstOrDefault(x => x.Name == property);
                if (info != null)
                {
                    propertyFound = true;

                    object currentValue = info.GetValue(target);

                    if (!currentValue.Equals(value))
                    {
                        info.SetValue(target, value);
                        PropertyChangeHandler(this, property);
                    }
                }
            }

            return propertyFound;
        }

        public CompositionSurfaceBrush Brush
        {
            get { return _brush; }
        }

        public virtual BorderEffect BorderEffect
        {
            get { return (BorderEffect)_inputEffect; }
        }

        public CompositionNineGridBrush NineGrid
        {
            get { return _nineGrid; }
        }

        public virtual StorageFile File
        {
            set
            {
                if (_file == null || (value == null && _file != null) || _file.Path != value.Path)
                {
                    _file = value;

                    if (_file != null)
                    {
                        FilePath = _file.Path;
                        _surface = ImageLoader.Instance.LoadFromFile(_file, Size.Empty, null);
                    }
                    else
                    {
                        FilePath = null;
                        if (_surface != null)
                        {
                            _surface.Dispose();
                            _surface = null;
                        }
                    }

                    PropertyChangeHandler(this, nameof(File));
                }
            }
        }

        public override void UpdateResourceBindings(CompositionEffectBrush brush)
        {
            if (_surface != null)
            {
                _brush.Surface = _surface.Surface;
                if (EdgeMode == EdgeMode.Ninegrid)
                {
                    _nineGrid.Source = _brush;
                    brush.SetSourceParameter(_id + "Image", _nineGrid);
                }
                else
                {
                    brush.SetSourceParameter(_id + "Image", _brush);
                }
            }
        }

        protected struct LayerProperty
        {
            public string Name { get; }
            public object TargetObject { get; }

            public LayerProperty(string name, object obj)
            {
                Name = name;
                TargetObject = obj;
            }
        };

        [DataMember]
        public EdgeMode EdgeMode
        {
            get { return _edgeMode; }
            set
            {
                if (value != _edgeMode)
                {
                    _edgeMode = value;
                    PropertyChangeHandler(this, nameof(EdgeMode));
                }
            }
        }

        [DataMember]
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (LayerProperty property in _properties)
                {
                    object value = property.TargetObject.GetType().GetProperties().FirstOrDefault(x => x.Name == property.Name).GetValue(property.TargetObject);

                    dict.Add(property.Name, Helpers.ToString(value));
                }

                return dict;
            }

            set
            {
                foreach (LayerProperty property in _properties)
                {
                    if (value.ContainsKey(property.Name))
                    {
                        PropertyInfo info = property.TargetObject.GetType().GetProperties().FirstOrDefault(x => x.Name == property.Name);
                        SetProperty(property.Name, Helpers.FromString(info.PropertyType, value[property.Name]));
                    }
                }
            }
        }
    }
}