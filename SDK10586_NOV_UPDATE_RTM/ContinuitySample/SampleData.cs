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
using System.Linq;
using Windows.UI.Composition;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


namespace ContinuitySample
{

    public class Item : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        private string _Title = string.Empty;
        public string Title
        {
            get
            {
                return this._Title;
            }

            set
            {
                if (this._Title != value)
                {
                    this._Title = value;
                    this.OnPropertyChanged("Title");
                }
            }
        }
        public ContainerVisual imageContainerVisual { get; set; }
        public Rectangle rectImage { get; set; }
        public Uri imageUri { get; set; }

        private string _Subtitle = string.Empty;
        public string Subtitle
        {
            get
            {
                return this._Subtitle;
            }

            set
            {
                if (this._Subtitle != value)
                {
                    this._Subtitle = value;
                    this.OnPropertyChanged("Subtitle");
                }
            }
        }

        private ImageSource _Image = null;
        public ImageSource Image
        {
            get
            {
                return this._Image;
            }

            set
            {
                if (this._Image != value)
                {
                    this._Image = value;
                    this.OnPropertyChanged("Image");
                }
            }
        }

        public void SetImage(Uri baseUri, String path)
        {
            Image = new BitmapImage(new Uri(baseUri, path));
        }

        private string _Link = string.Empty;
        public string Link
        {
            get
            {
                return this._Link;
            }

            set
            {
                if (this._Link != value)
                {
                    this._Link = value;
                    this.OnPropertyChanged("Link");
                }
            }
        }

        private string _Category = string.Empty;
        public string Category
        {
            get
            {
                return this._Category;
            }

            set
            {
                if (this._Category != value)
                {
                    this._Category = value;
                    this.OnPropertyChanged("Category");
                }
            }
        }

        private string _Description = string.Empty;
        public string Description
        {
            get
            {
                return this._Description;
            }

            set
            {
                if (this._Description != value)
                {
                    this._Description = value;
                    this.OnPropertyChanged("Description");
                }
            }
        }

        private string _Content = string.Empty;
        public string Content
        {
            get
            {
                return this._Content;
            }

            set
            {
                if (this._Content != value)
                {
                    this._Content = value;
                    this.OnPropertyChanged("Content");
                }
            }
        }
    }

    public class GroupInfoList<T> : List<object>
    {

        public object Key { get; set; }


        public new IEnumerator<object> GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<object>)base.GetEnumerator();
        }
    }


    public class StoreData
    {
        public StoreData()
        {
            Item item;
            String LONG_LOREM_IPSUM = String.Format("{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");
            Uri baseUri = new Uri("ms-appx:///");

            item = new Item();
            item.Title = "Banana Blast Frozen Yogurt";
            item.Subtitle = "Maecenas class nam praesent cras aenean mauris aliquam nullam aptent accumsan duis nunc curae donec integer auctor sed congue amet";
            item.imageUri = new Uri(baseUri + "Images/60Banana.png");
            item.Link = "http://www.adatum.com/";
            item.Category = "Low-fat frozen yogurt";
            item.Description = "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Lavish Lemon Ice";
            item.Subtitle = "Quisque vivamus bibendum cursus dictum dictumst dis aliquam aliquet etiam lectus eleifend fusce libero ante facilisi ligula est";
            item.imageUri = new Uri(baseUri + "Images/60Lemon.png");
            item.Link = "http://www.adventure-works.com/";
            item.Category = "Sorbet";
            item.Description = "Enim cursus nascetur dictum habitasse hendrerit nec gravida vestibulum pellentesque vestibulum adipiscing iaculis erat consectetuer pellentesque parturient lacinia himenaeos pharetra condimentum non sollicitudin eros dolor vestibulum per lectus pellentesque nibh imperdiet laoreet consectetuer placerat libero malesuada pellentesque fames penatibus ligula scelerisque litora nisi luctus vestibulum nisl ullamcorper sed sem natoque suspendisse felis sit condimentum pulvinar nunc posuere magnis vel scelerisque sagittis porttitor potenti tincidunt mattis ipsum adipiscing sollicitudin parturient mauris nam senectus ullamcorper mollis tristique sociosqu suspendisse ultricies montes sed condimentum dis nostra suscipit justo ornare pretium odio pellentesque lacus lorem torquent orci";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Marvelous Mint";
            item.Subtitle = "Litora luctus magnis arcu lorem morbi blandit faucibus mattis commodo hac habitant inceptos conubia cubilia nulla mauris diam proin augue eget dolor mollis interdum lobortis";
            item.imageUri = new Uri(baseUri + "Images/60Mint.png");
            item.Link = "http://www.adventure-works.com/";
            item.Category = "Gelato";
            item.Description = "Vestibulum vestibulum magna scelerisque ultrices consectetuer vehicula rhoncus pellentesque massa adipiscing platea primis sodales parturient metus sollicitudin morbi vestibulum pellentesque consectetuer pellentesque volutpat rutrum sollicitudin sapien pellentesque vestibulum venenatis consectetuer viverra est aliquam semper hac maecenas integer adipiscing sociis vulputate ullamcorper curabitur pellentesque parturient praesent neque sollicitudin pellentesque vestibulum suspendisse consectetuer leo quisque phasellus pede vestibulum quam pellentesque sollicitudin quis mus adipiscing parturient pellentesque vestibulum";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Creamy Orange";
            item.Subtitle = "Leo mus nec nascetur dapibus non fames per felis ipsum pharetra egestas montes elit nostra placerat euismod enim justo ornare feugiat platea pulvinar sed sagittis";
            item.imageUri = new Uri(baseUri + "Images/60Orange.png");
            item.Link = "http://www.alpineskihouse.com/";
            item.Category = "Sorbet";
            item.Description = "Consequat condimentum consectetuer vivamus urna vestibulum netus pellentesque cras nec taciti non scelerisque adipiscing parturient tellus sollicitudin per vestibulum pellentesque aliquam convallis ullamcorper nulla porta aliquet accumsan suspendisse duis bibendum nunc condimentum consectetuer pellentesque scelerisque tempor sed dictumst eleifend amet vestibulum sem tempus facilisi ullamcorper adipiscing tortor ante purus parturient sit dignissim vel nam turpis sed sollicitudin elementum arcu vestibulum risus blandit suspendisse faucibus pellentesque commodo dis condimentum consectetuer varius aenean conubia cubilia facilisis velit mauris nullam aptent dapibus habitant";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Succulent Strawberry";
            item.Subtitle = "Senectus sem lacus erat sociosqu eros suscipit primis nibh nisi nisl gravida torquent";
            item.imageUri = new Uri(baseUri + "Images/60Strawberry.png");
            item.Link = "http://www.baldwinmuseumofscience.com/";
            item.Category = "Sorbet";
            item.Description = "Est auctor inceptos congue interdum egestas scelerisque pellentesque fermentum ullamcorper cursus dictum lectus suspendisse condimentum libero vitae vestibulum lobortis ligula fringilla euismod class scelerisque feugiat habitasse diam litora adipiscing sollicitudin parturient hendrerit curae himenaeos imperdiet ullamcorper suspendisse nascetur hac gravida pharetra eget donec leo mus nec non malesuada vestibulum pellentesque elit penatibus vestibulum per condimentum porttitor sed adipiscing scelerisque ullamcorper etiam iaculis enim tincidunt erat parturient sem vestibulum eros";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Very Vanilla";
            item.Subtitle = "Ultrices rutrum sapien vehicula semper lorem volutpat sociis sit maecenas praesent taciti magna nunc odio orci vel tellus nam sed accumsan iaculis dis est";
            item.imageUri = new Uri(baseUri + "Images/60Vanilla.png");
            item.Link = "http://www.blueyonderairlines.com/";
            item.Category = "Ice Cream";
            item.Description = "Consectetuer lacinia vestibulum tristique sit adipiscing laoreet fusce nibh suspendisse natoque placerat pulvinar ultricies condimentum scelerisque nisi ullamcorper nisl parturient vel suspendisse nam venenatis nunc lorem sed dis sagittis pellentesque luctus sollicitudin morbi posuere vestibulum potenti magnis pellentesque vulputate mattis mauris mollis consectetuer pellentesque pretium montes vestibulum condimentum nulla adipiscing sollicitudin scelerisque ullamcorper pellentesque odio orci rhoncus pede sodales suspendisse parturient viverra curabitur proin aliquam integer augue quam condimentum quisque senectus quis urna scelerisque nostra phasellus ullamcorper cras duis suspendisse sociosqu dolor vestibulum condimentum consectetuer vivamus est fames felis suscipit hac";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Creamy Caramel Frozen Yogurt";
            item.Subtitle = "Maecenas class nam praesent cras aenean mauris aliquam nullam aptent accumsan duis nunc curae donec integer auctor sed congue amet";
            item.imageUri = new Uri(baseUri + "Images/60SauceCaramel.png");
            item.Link = "http://www.adatum.com/";
            item.Category = "Low-fat frozen yogurt";
            item.Description = "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Chocolate Lovers Frozen Yogurt";
            item.Subtitle = "Quisque vivamus bibendum cursus dictum dictumst dis aliquam aliquet etiam lectus eleifend fusce libero ante facilisi ligula est";
            item.imageUri = new Uri(baseUri + "Images/60SauceChocolate.png");
            item.Link = "http://www.adventure-works.com/";
            item.Category = "Low-fat frozen yogurt";
            item.Description = "Enim cursus nascetur dictum habitasse hendrerit nec gravida vestibulum pellentesque vestibulum adipiscing iaculis erat consectetuer pellentesque parturient lacinia himenaeos pharetra condimentum non sollicitudin eros dolor vestibulum per lectus pellentesque nibh imperdiet laoreet consectetuer placerat libero malesuada pellentesque fames penatibus ligula scelerisque litora nisi luctus vestibulum nisl ullamcorper sed sem natoque suspendisse felis sit condimentum pulvinar nunc posuere magnis vel scelerisque sagittis porttitor potenti tincidunt mattis ipsum adipiscing sollicitudin parturient mauris nam senectus ullamcorper mollis tristique sociosqu suspendisse ultricies montes sed condimentum dis nostra suscipit justo ornare pretium odio pellentesque lacus lorem torquent orci";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Roma Strawberry";
            item.Subtitle = "Litora luctus magnis arcu lorem morbi blandit faucibus mattis commodo hac habitant inceptos conubia cubilia nulla mauris diam proin augue eget dolor mollis interdum lobortis";
            item.imageUri = new Uri(baseUri + "Images/60Strawberry.png");
            item.Link = "http://www.adventure-works.com/";
            item.Category = "Gelato";
            item.Description = "Vestibulum vestibulum magna scelerisque ultrices consectetuer vehicula rhoncus pellentesque massa adipiscing platea primis sodales parturient metus sollicitudin morbi vestibulum pellentesque consectetuer pellentesque volutpat rutrum sollicitudin sapien pellentesque vestibulum venenatis consectetuer viverra est aliquam semper hac maecenas integer adipiscing sociis vulputate ullamcorper curabitur pellentesque parturient praesent neque sollicitudin pellentesque vestibulum suspendisse consectetuer leo quisque phasellus pede vestibulum quam pellentesque sollicitudin quis mus adipiscing parturient pellentesque vestibulum";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Italian Rainbow";
            item.Subtitle = "Leo mus nec nascetur dapibus non fames per felis ipsum pharetra egestas montes elit nostra placerat euismod enim justo ornare feugiat platea pulvinar sed sagittis";
            item.imageUri = new Uri(baseUri + "Images/60SprinklesRainbow.png");
            item.Link = "http://www.alpineskihouse.com/";
            item.Category = "Gelato";
            item.Description = "Consequat condimentum consectetuer vivamus urna vestibulum netus pellentesque cras nec taciti non scelerisque adipiscing parturient tellus sollicitudin per vestibulum pellentesque aliquam convallis ullamcorper nulla porta aliquet accumsan suspendisse duis bibendum nunc condimentum consectetuer pellentesque scelerisque tempor sed dictumst eleifend amet vestibulum sem tempus facilisi ullamcorper adipiscing tortor ante purus parturient sit dignissim vel nam turpis sed sollicitudin elementum arcu vestibulum risus blandit suspendisse faucibus pellentesque commodo dis condimentum consectetuer varius aenean conubia cubilia facilisis velit mauris nullam aptent dapibus habitant";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Strawberry";
            item.Subtitle = "Ultrices rutrum sapien vehicula semper lorem volutpat sociis sit maecenas praesent taciti magna nunc odio orci vel tellus nam sed accumsan iaculis dis est";
            item.imageUri = new Uri(baseUri + "Images/60Strawberry.png");
            item.Link = "http://www.blueyonderairlines.com/";
            item.Category = "Ice Cream";
            item.Description = "Consectetuer lacinia vestibulum tristique sit adipiscing laoreet fusce nibh suspendisse natoque placerat pulvinar ultricies condimentum scelerisque nisi ullamcorper nisl parturient vel suspendisse nam venenatis nunc lorem sed dis sagittis pellentesque luctus sollicitudin morbi posuere vestibulum potenti magnis pellentesque vulputate mattis mauris mollis consectetuer pellentesque pretium montes vestibulum condimentum nulla adipiscing sollicitudin scelerisque ullamcorper pellentesque odio orci rhoncus pede sodales suspendisse parturient viverra curabitur proin aliquam integer augue quam condimentum quisque senectus quis urna scelerisque nostra phasellus ullamcorper cras duis suspendisse sociosqu dolor vestibulum condimentum consectetuer vivamus est fames felis suscipit hac";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Strawberry Frozen Yogurt";
            item.Subtitle = "Maecenas class nam praesent cras aenean mauris aliquam nullam aptent accumsan duis nunc curae donec integer auctor sed congue amet";
            item.imageUri = new Uri(baseUri + "Images/60Strawberry.png");
            item.Link = "http://www.adatum.com/";
            item.Category = "Low-fat frozen yogurt";
            item.Description = "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Bongo Banana";
            item.Subtitle = "Quisque vivamus bibendum cursus dictum dictumst dis aliquam aliquet etiam lectus eleifend fusce libero ante facilisi ligula est";
            item.imageUri = new Uri(baseUri + "Images/60Banana.png");
            item.Link = "http://www.adventure-works.com/";
            item.Category = "Sorbet";
            item.Description = "Enim cursus nascetur dictum habitasse hendrerit nec gravida vestibulum pellentesque vestibulum adipiscing iaculis erat consectetuer pellentesque parturient lacinia himenaeos pharetra condimentum non sollicitudin eros dolor vestibulum per lectus pellentesque nibh imperdiet laoreet consectetuer placerat libero malesuada pellentesque fames penatibus ligula scelerisque litora nisi luctus vestibulum nisl ullamcorper sed sem natoque suspendisse felis sit condimentum pulvinar nunc posuere magnis vel scelerisque sagittis porttitor potenti tincidunt mattis ipsum adipiscing sollicitudin parturient mauris nam senectus ullamcorper mollis tristique sociosqu suspendisse ultricies montes sed condimentum dis nostra suscipit justo ornare pretium odio pellentesque lacus lorem torquent orci";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Firenze Vanilla";
            item.Subtitle = "Litora luctus magnis arcu lorem morbi blandit faucibus mattis commodo hac habitant inceptos conubia cubilia nulla mauris diam proin augue eget dolor mollis interdum lobortis";
            item.imageUri = new Uri(baseUri + "Images/60Vanilla.png");
            item.Link = "http://www.adventure-works.com/";
            item.Category = "Gelato";
            item.Description = "Vestibulum vestibulum magna scelerisque ultrices consectetuer vehicula rhoncus pellentesque massa adipiscing platea primis sodales parturient metus sollicitudin morbi vestibulum pellentesque consectetuer pellentesque volutpat rutrum sollicitudin sapien pellentesque vestibulum venenatis consectetuer viverra est aliquam semper hac maecenas integer adipiscing sociis vulputate ullamcorper curabitur pellentesque parturient praesent neque sollicitudin pellentesque vestibulum suspendisse consectetuer leo quisque phasellus pede vestibulum quam pellentesque sollicitudin quis mus adipiscing parturient pellentesque vestibulum";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Choco-wocko";
            item.Subtitle = "Leo mus nec nascetur dapibus non fames per felis ipsum pharetra egestas montes elit nostra placerat euismod enim justo ornare feugiat platea pulvinar sed sagittis";
            item.imageUri = new Uri(baseUri + "Images/60SauceChocolate.png");
            item.Link = "http://www.alpineskihouse.com/";
            item.Category = "Sorbet";
            item.Description = "Consequat condimentum consectetuer vivamus urna vestibulum netus pellentesque cras nec taciti non scelerisque adipiscing parturient tellus sollicitudin per vestibulum pellentesque aliquam convallis ullamcorper nulla porta aliquet accumsan suspendisse duis bibendum nunc condimentum consectetuer pellentesque scelerisque tempor sed dictumst eleifend amet vestibulum sem tempus facilisi ullamcorper adipiscing tortor ante purus parturient sit dignissim vel nam turpis sed sollicitudin elementum arcu vestibulum risus blandit suspendisse faucibus pellentesque commodo dis condimentum consectetuer varius aenean conubia cubilia facilisis velit mauris nullam aptent dapibus habitant";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Chocolate";
            item.Subtitle = "Ultrices rutrum sapien vehicula semper lorem volutpat sociis sit maecenas praesent taciti magna nunc odio orci vel tellus nam sed accumsan iaculis dis est";
            item.imageUri = new Uri(baseUri + "Images/60SauceChocolate.png");
            item.Link = "http://www.blueyonderairlines.com/";
            item.Category = "Ice Cream";
            item.Description = "Consectetuer lacinia vestibulum tristique sit adipiscing laoreet fusce nibh suspendisse natoque placerat pulvinar ultricies condimentum scelerisque nisi ullamcorper nisl parturient vel suspendisse nam venenatis nunc lorem sed dis sagittis pellentesque luctus sollicitudin morbi posuere vestibulum potenti magnis pellentesque vulputate mattis mauris mollis consectetuer pellentesque pretium montes vestibulum condimentum nulla adipiscing sollicitudin scelerisque ullamcorper pellentesque odio orci rhoncus pede sodales suspendisse parturient viverra curabitur proin aliquam integer augue quam condimentum quisque senectus quis urna scelerisque nostra phasellus ullamcorper cras duis suspendisse sociosqu dolor vestibulum condimentum consectetuer vivamus est fames felis suscipit hac";
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);
        }



        private ItemCollection _Collection = new ItemCollection();

        public ItemCollection Collection
        {
            get
            {
                return this._Collection;
            }
        }

        internal List<GroupInfoList<object>> GetGroupsByCategory()
        {
            List<GroupInfoList<object>> groups = new List<GroupInfoList<object>>();

            var query = from item in Collection
                        orderby ((Item)item).Category
                        group item by ((Item)item).Category into g
                        select new { GroupName = g.Key, Items = g };
            foreach (var g in query)
            {
                GroupInfoList<object> info = new GroupInfoList<object>();
                info.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    info.Add(item);
                }
                groups.Add(info);
            }

            return groups;

        }

        internal List<GroupInfoList<object>> GetGroupsByLetter()
        {
            List<GroupInfoList<object>> groups = new List<GroupInfoList<object>>();

            var query = from item in Collection
                        orderby ((Item)item).Title
                        group item by ((Item)item).Title[0] into g
                        select new { GroupName = g.Key, Items = g };
            foreach (var g in query)
            {
                GroupInfoList<object> info = new GroupInfoList<object>();
                info.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    info.Add(item);
                }
                groups.Add(info);
            }

            return groups;

        }
    }

    public class MessageData
    {
        public MessageData()
        {
            Item item;
            String LONG_LOREM_IPSUM = String.Format("{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");
            Uri baseUri = new Uri("ms-appx:///");

            item = new Item();
            item.Title = "New Flavors out this week!";
            item.Subtitle = "Adam Barr";
            item.imageUri = new Uri(baseUri + "Images/60Mail01.png");
            item.Content = "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum";
            Collection.Add(item);

            item = new Item();
            item.Title = "Check out this topping!";
            item.Subtitle = "David Alexander";
            item.imageUri = new Uri(baseUri + "Images/60Mail01.png");
            item.Content = "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue";
            Collection.Add(item);

            item = new Item();
            item.Title = "Come to the Ice Cream Party";
            item.Subtitle = "Josh Bailey";
            item.imageUri = new Uri(baseUri + "Images/60Mail01.png");
            item.Content = "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse";
            Collection.Add(item);

            item = new Item();
            item.Title = "How about gluten free?";
            item.Subtitle = "Chris Berry";
            item.imageUri = new Uri(baseUri + "Images/60Mail01.png");
            item.Content = LONG_LOREM_IPSUM;
            Collection.Add(item);

            item = new Item();
            item.Title = "Summer promotion - BYGO";
            item.Subtitle = "Sean Bentley";
            item.imageUri = new Uri(baseUri + "Images/60Mail01.png");
            item.Content = "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat";
            Collection.Add(item);

            item = new Item();
            item.Title = "Awesome flavor combination";
            item.Subtitle = "Adrian Lannin";
            item.imageUri = new Uri(baseUri + "Images/60Mail01.png");
            item.Content = "Curabitur class aliquam vestibulum nam curae maecenas sed integer";
            Collection.Add(item);

        }
        private ItemCollection _Collection = new ItemCollection();

        public ItemCollection Collection
        {
            get
            {
                return this._Collection;
            }
        }
    }

    public class CommentData
    {
        public CommentData()
        {
            Item item;
            Uri baseUri = new Uri("ms-appx:///");

            item = new Item();
            item.Title = "The Smoothest";
            item.Content = "I loved this ice cream. I thought it was maybe the smoothest ice cream that i have ever had!";
            Collection.Add(item);

            item = new Item();
            item.Title = "What a great flavor!";
            item.Content = "Although the texture was a bit lacking, this one has the best flavor I have tasted!";
            Collection.Add(item);

            item = new Item();
            item.Title = "Didn't like the 'choco bits";
            item.Content = "The little bits of chocolate just weren't working for me";
            Collection.Add(item);

            item = new Item();
            item.Title = "Loved the peanut butter";
            item.Content = "The peanut butter was the best part of this delicious snack";
            Collection.Add(item);

            item = new Item();
            item.Title = "Wish there was more sugar";
            item.Content = "This wasn't sweet enough for me. I will have to try your other flavors, but maybe this is too healthy for me";
            Collection.Add(item);

            item = new Item();
            item.Title = "Texture was perfect";
            item.Content = "This was the smoothest ice cream I have ever had";
            Collection.Add(item);

            item = new Item();
            item.Title = "Kept wishing there was more";
            item.Content = "When I got to the end of each carton I kept wishing there was more ice cream. It was delicious!";
            Collection.Add(item);

        }
        private ItemCollection _Collection = new ItemCollection();

        public ItemCollection Collection
        {
            get
            {
                return this._Collection;
            }
        }
    }

    public class ToppingsData
    {
        public ToppingsData()
        {
            Item item;
            Uri baseUri = new Uri("ms-appx:///");

            item = new Item();
            item.Title = "Caramel Sauce";
            item.Category = "Sauces";
            item.imageUri = new Uri(baseUri + "Images/60SauceCaramel.png");
            Collection.Add(item);

            item = new Item();
            item.Title = "Chocolate Sauce";
            item.Category = "Sauces";
            item.imageUri = new Uri(baseUri + "Images/60SauceChocolate.png");
            Collection.Add(item);

            item = new Item();
            item.Title = "Strawberry Sauce";
            item.Category = "Sauces";
            item.imageUri = new Uri(baseUri + "Images/60SauceStrawberry.png");
            Collection.Add(item);

            item = new Item();
            item.Title = "Chocolate Sprinkles";
            item.Category = "Sprinkles";
            item.imageUri = new Uri(baseUri + "Images/60SprinklesChocolate.png");
            Collection.Add(item);

            item = new Item();
            item.Title = "Rainbow Sprinkles";
            item.Category = "Sprinkles";
            item.imageUri = new Uri(baseUri + "Images/60SprinklesRainbow.png");
            Collection.Add(item);

            item = new Item();
            item.Title = "Vanilla Sprinkles";
            item.Category = "Sprinkles";
            item.imageUri = new Uri(baseUri + "Images/60SprinklesVanilla.png");
            Collection.Add(item);

        }
        private ItemCollection _Collection = new ItemCollection();

        public ItemCollection Collection
        {
            get
            {
                return this._Collection;
            }
        }
    }


    // Workaround: data binding works best with an enumeration of objects that does not implement IList
    public class ItemCollection : IEnumerable<Object>
    {
        private System.Collections.ObjectModel.ObservableCollection<Item> itemCollection = new System.Collections.ObjectModel.ObservableCollection<Item>();

        public IEnumerator<Object> GetEnumerator()
        {
            return itemCollection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Item item)
        {
            itemCollection.Add(item);
        }
    }

}