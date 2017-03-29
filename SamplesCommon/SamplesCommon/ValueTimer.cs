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
using System.Threading;

namespace SamplesCommon
{
    /// <summary>
    /// Value wrapper
    /// </summary>
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public T Value { get; private set; }

        public ValueChangedEventArgs(T value)
        {
            Value = value;
        }
    }

    public delegate void ValueChangedHandler<T>(ValueTimer<T> timer, ValueChangedEventArgs<T> args);

    /// <summary>
    /// ValueTimer provides timed callback capability with a specified value.
    /// </summary>
    public class ValueTimer<T> : IDisposable
    {
        /// <summary>
        /// An event that signifies that the timer interval has been met for a given value change.
        /// This event fires on a threadpool thread, and not on the UI thread.
        /// </summary>
        public event ValueChangedHandler<T> ValueChanged;

        public int IntervalMilliseconds { get; set; }
        public T Value { get { return _value; } }

        /// <summary>
        /// Constructor, calls to initialize timer with timeout interval of zero.
        /// </summary>
        public ValueTimer()
        {
            Initialize(0);
        }

        /// <summary>
        /// Overloaded constructor initializes timer with a given interval timeout.
        /// </summary>
        /// <param name="intervalMilliseconds ">The timeout amount to use with the timer</param>
        public ValueTimer(int intervalMilliseconds)
        {
            Initialize(intervalMilliseconds);
        }

        /// <summary>
        /// Restarts the timer and sets the internal value to the given value.
        /// </summary>
        public void Restart(T value)
        {
            _timer.Change(IntervalMilliseconds, Timeout.Infinite);
            _value = value;
        }

        /// <summary>
        /// Initialize timer and store interval value, specify callback for timeout.
        /// </summary>
        /// <param name="intervalMilliseconds "></param>
        private void Initialize(int intervalMilliseconds)
        {
            IntervalMilliseconds = intervalMilliseconds;
            // We're using the threading timer here instead of the DispatchTimer in order
            // to be able to effectively "cancel" a previously scheduled tick. However, 
            // this also means that the event will fire on a threadpool thread and not on 
            // the UI thread.
            _timer = new Timer(TimerCallback, this, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Call previously specified callback method with value wrapped in ValueChangedEventArgs object.
        /// </summary>
        private void TimerCallback(object state)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(_value));
        }

        public void Dispose()
        {
            _timer.Dispose();
            _timer = null;
        }

        private Timer _timer;
        private T _value;
    }
}