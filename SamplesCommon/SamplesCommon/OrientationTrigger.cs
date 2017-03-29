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

using Windows.UI.Xaml;

namespace SamplesCommon
{
    /// <summary>
    /// Scrren Orientation Enum
    /// </summary>
    public enum Orientation
    {
        Landscape,
        Portrait
    }

    /// <summary>
    /// OrientationTriggeris a state trigger that responds to changes of the size of the window, 
    /// and is set to active when the current orientation matches the requested orientation (property).
    /// </summary>
    public class OrientationTrigger : StateTriggerBase
    {
        // Private members
        private Orientation _orientation;

        /// <summary>
        /// Property that determines the orientation that this trigger will be active in.
        /// </summary>
        public Orientation Orientation { get { return _orientation; } set { _orientation = value; EvaluateCurrentOrientation(GetCurrentOrientation()); } }

        /// <summary>
        /// Constructor
        /// </summary>
        public OrientationTrigger()
        {
            // Get the current orientation
            Orientation currentOrientation = GetCurrentOrientation();

            // See if the current orientation matches the requested orientation.
            EvaluateCurrentOrientation(currentOrientation);

            Window.Current.SizeChanged += Current_SizeChanged;
        }

        /// <summary>
        /// Event handler to the window's SizeChanged event that determines the current orientation.
        /// </summary>
        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            // Get the current orientation
            Orientation currentOrientation = GetCurrentOrientation();

            // See if the current orientation matches the requested orientation.
            EvaluateCurrentOrientation(currentOrientation);
        }

        /// <summary>
        /// Determines the current orientation of the window/screen.
        /// </summary>
        /// <returns>Orientation</returns>
        private Orientation GetCurrentOrientation()
        {
            var width = Window.Current.Bounds.Width;
            var height = Window.Current.Bounds.Height;

            // If our width is greater than our height, we are in landscape. Otherwise we are in 
            // portrait. 
            return width > height ? Orientation.Landscape : Orientation.Portrait;
        }

        /// <summary>
        /// Evaluates whether the current orientation matches the given/desired orientation.
        /// </summary>
        /// <param name="currentOrientation">The current orientation to be evaluated.</param>
        private void EvaluateCurrentOrientation(Orientation currentOrientation)
        {
            SetActive(currentOrientation == Orientation);
        }
    }
}
