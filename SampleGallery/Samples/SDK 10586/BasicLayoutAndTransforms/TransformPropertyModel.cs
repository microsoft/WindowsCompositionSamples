using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace CompositionSampleGallery
{
    class TransformPropertyModel : INotifyPropertyChanged
    {
        private float _value;
        private Action<float> _action;

        public string PropertyName { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float StepFrequency { get; set; }
        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _action(_value);
                OnPropertyChanged();
            }
        }

        public TransformPropertyModel(Action<float> action)
        {
            _action = action;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
