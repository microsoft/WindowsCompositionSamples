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

using System.Numerics;
using Windows.UI.Composition;

namespace SamplesCommon
{
    public enum PennerType
    {
        Sine = 1,
        Quad = 11,
        Cubic = 101,
        Quartic = 1001,
        Quintic = 10001,
        Exponential = 100001,
        Circle = 1000001,
        Back = 10000001
    }

    public enum PennerVariation
    {
        EaseIn = 2,
        EaseOut = 3,
        EaseInOut = 4
    }

    public static class PennerEquationBuilder
    {
        public static CompositionEasingFunction CreatePennerEquation(this Compositor compositor, PennerType type, PennerVariation variation)
        {
            int pennerType = (int)type;
            int pennerVariation = (int)variation;
            int sum = pennerType + pennerVariation;

            Vector2 controlPoint1, controlPoint2;

            switch (sum)
            {
                // Sine, EaseIn
                case 3:
                    controlPoint1 = new Vector2(0.47f, 0.0f);
                    controlPoint2 = new Vector2(0.745f, 0.715f);
                    break;
                // Sine, EaseOut
                case 4:
                    controlPoint1 = new Vector2(0.39f, 0.575f);
                    controlPoint2 = new Vector2(0.565f, 1.0f);
                    break;
                // Sine EaseInOut
                case 5:
                    controlPoint1 = new Vector2(0.445f, 0.05f);
                    controlPoint2 = new Vector2(0.55f, 0.95f);
                    break;
                // Quad, EaseIn
                case 13:
                    controlPoint1 = new Vector2(0.55f, 0.085f);
                    controlPoint2 = new Vector2(0.68f, 0.53f);
                    break;
                // Quad, EaseOut
                case 14:
                    controlPoint1 = new Vector2(0.25f, 0.46f);
                    controlPoint2 = new Vector2(0.45f, 0.94f);
                    break;
                // Quad, EaseInOut
                case 15:
                    controlPoint1 = new Vector2(0.445f, 0.03f);
                    controlPoint2 = new Vector2(0.515f, 0.955f);
                    break;
                // Cubic, EaseIn
                case 103:
                    controlPoint1 = new Vector2(0.55f, 0.055f);
                    controlPoint2 = new Vector2(0.675f, 0.19f);
                    break;
                // Cubic, EaseOut
                case 104:
                    controlPoint1 = new Vector2(0.215f, 0.61f);
                    controlPoint2 = new Vector2(0.355f, 1.0f);
                    break;
                // Cubic EaseInOut
                case 105:
                    controlPoint1 = new Vector2(0.645f, 0.045f);
                    controlPoint2 = new Vector2(0.355f, 1.0f);
                    break;
                // Quartic, EaseIn
                case 1003:
                    controlPoint1 = new Vector2(0.895f, 0.03f);
                    controlPoint2 = new Vector2(0.685f, 0.22f);
                    break;
                // Quartic EaseOut
                case 1004:
                    controlPoint1 = new Vector2(0.165f, 0.84f);
                    controlPoint2 = new Vector2(0.44f, 1.0f);
                    break;
                // Quartic EaseInOut
                case 1005:
                    controlPoint1 = new Vector2(0.77f, 0.0f);
                    controlPoint2 = new Vector2(0.175f, 1.0f);
                    break;
                // Quintic, EaseIn
                case 10003:
                    controlPoint1 = new Vector2(0.755f, 0.05f);
                    controlPoint2 = new Vector2(0.855f, 0.06f);
                    break;
                // Quintic, EaseOut
                case 10004:
                    controlPoint1 = new Vector2(0.23f, 1.0f);
                    controlPoint2 = new Vector2(0.32f, 1.0f);
                    break;
                // Quintic, EaseInOut
                case 10005:
                    controlPoint1 = new Vector2(0.86f, 0.0f);
                    controlPoint2 = new Vector2(0.07f, 1.0f);
                    break;
                // Exponential, EaseIn
                case 100003:
                    controlPoint1 = new Vector2(0.95f, 0.05f);
                    controlPoint2 = new Vector2(0.795f, 0.035f);
                    break;
                // Exponential, EaseOut
                case 100004:
                    controlPoint1 = new Vector2(0.19f, 1.0f);
                    controlPoint2 = new Vector2(0.22f, 1.0f);
                    break;
                // Exponential, EaseInOut
                case 100005:
                    controlPoint1 = new Vector2(1.0f, 0.0f);
                    controlPoint2 = new Vector2(0.0f, 1.0f);
                    break;
                // Circle, EaseIn
                case 1000003:
                    controlPoint1 = new Vector2(0.6f, 0.04f);
                    controlPoint2 = new Vector2(0.98f, 0.335f);
                    break;
                // Circle, EaseOut
                case 1000004:
                    controlPoint1 = new Vector2(0.075f, 0.82f);
                    controlPoint2 = new Vector2(0.165f, 1.0f);
                    break;
                // Circle, EaseInOut
                case 1000005:
                    controlPoint1 = new Vector2(0.785f, 0.135f);
                    controlPoint2 = new Vector2(0.15f, 0.86f);
                    break;
                // Back, EaseIn
                case 10000003:
                    controlPoint1 = new Vector2(0.6f, -0.28f);
                    controlPoint2 = new Vector2(0.735f, 0.045f);
                    break;
                // Back, EaseOut
                case 10000004:
                    controlPoint1 = new Vector2(0.175f, 0.885f);
                    controlPoint2 = new Vector2(0.32f, 1.275f);
                    break;
                // Back, EaseInOut
                case 10000005:
                    controlPoint1 = new Vector2(0.68f, -0.55f);
                    controlPoint2 = new Vector2(0.265f, 1.55f);
                    break;
                default:
                    controlPoint1 = new Vector2(0.0f);
                    controlPoint2 = new Vector2(0.0f);
                    break;
            }

            CompositionEasingFunction pennerEquation = compositor.CreateCubicBezierEasingFunction(controlPoint1, controlPoint2);
            return pennerEquation;
        }
    }
}
