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
using Microsoft.UI.Xaml.Controls;
using MUI = Microsoft.UI.Input;
using InputSystemCursorShape = Microsoft.UI.Input.InputSystemCursorShape;

namespace CompositionSampleGallery
{
	/* In order to access the ProtectedCursor property of a UIElement, we have to subclass it */
	/* We can subclass any control that derives UIElement (Button is a likely one)            */
	public class CursorPanel : Panel
	{
		public CursorPanel() { }

		/* With a proper subclass, we can now write our own function that will dynamically change the cursor. */
		/* Be careful not to change the ProtectedCursor before the control is fully loaded (After InitializeComponent() completes) */
		public void ChangeCursor(MUI::InputCursor cursor)
		{
			this.ProtectedCursor = cursor;
		}
	}

	public sealed partial class InputCursor : SamplePage
    {

        public InputCursor()
        {
            this.InitializeComponent();
        }

        public static string StaticSampleName => "InputCursor";
        public override string SampleName => StaticSampleName;
		public static string StaticSampleDescription => "Demonstrates creation and usage of InputCursor objects. Use the ComboBox to select a predefined cursor shape, " +
														"then hover the mouse cursor over the box below to see the new shape.";
        public override string SampleDescription => StaticSampleDescription;

		private void cursors_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (panel == null)
				return;

			InputSystemCursorShape shape;

			switch (cursors.SelectedValue)
			{
				case "Arrow":
					shape = InputSystemCursorShape.Arrow;
					break;
				case "Cross":
					shape = InputSystemCursorShape.Cross;
					break;
				case "Hand":
					shape = InputSystemCursorShape.Hand;
					break;
				case "Help":
					shape = InputSystemCursorShape.Help;
					break;
				case "IBeam":
					shape = InputSystemCursorShape.IBeam;
					break;
				case "Person":
					shape = InputSystemCursorShape.Person;
					break;
				case "Pin":
					shape = InputSystemCursorShape.Pin;
					break;
				case "SizeAll":
					shape = InputSystemCursorShape.SizeAll;
					break;
				case "SizeNortheastSouthwest":
					shape = InputSystemCursorShape.SizeNortheastSouthwest;
					break;
				case "SizeNorthSouth":
					shape = InputSystemCursorShape.SizeNorthSouth;
					break;
				case "SizeNorthwestSoutheast":
					shape = InputSystemCursorShape.SizeNorthwestSoutheast;
					break;
				case "SizeWestEast":
					shape = InputSystemCursorShape.SizeWestEast;
					break;
				case "UniversalNo":
					shape = InputSystemCursorShape.UniversalNo;
					break;
				case "UpArrow":
					shape = InputSystemCursorShape.UpArrow;
					break;
				case "Wait":
					shape = InputSystemCursorShape.Wait;
					break;
				default:
					shape = InputSystemCursorShape.Arrow;
					break;
			}

			// Creation of an InputSystemCursor requires that the static factory, 'Create' is used along with a prefined shape.
            // For custom cursors, we would call InputResourceCursor.Create with the appropriate cursor resource ID. 
			panel.ChangeCursor(MUI::InputSystemCursor.Create(shape));
		}


	}
}