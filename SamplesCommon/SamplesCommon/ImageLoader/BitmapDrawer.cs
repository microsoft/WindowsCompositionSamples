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
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI.Composition;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;

namespace SamplesCommon
{
    public class BitmapDrawer : IContentDrawer
    {
        Uri _uri;
        LoadTimeEffectHandler _handler;

        public BitmapDrawer(Uri uri, LoadTimeEffectHandler handler)
        {
            _uri = uri;
            _handler = handler;
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public async Task Draw(CompositionGraphicsDevice device, Object drawingLock, CompositionDrawingSurface surface, Size size)
        {
            var canvasDevice = CanvasComposition.GetCanvasDevice(device);
            using (var canvasBitmap = await CanvasBitmap.LoadAsync(canvasDevice, _uri))
            {
                var bitmapSize = canvasBitmap.Size;

                //
                // Because the drawing is done asynchronously and multiple threads could
                // be trying to get access to the device/surface at the same time, we need
                // to do any device/surface work under a lock.
                //
                lock (drawingLock)
                {
                    Size surfaceSize = size;
                    if (surface.Size != size || surface.Size == new Size(0, 0))
                    {
                        // Resize the surface to the size of the image
                        CanvasComposition.Resize(surface, bitmapSize);
                        surfaceSize = bitmapSize;
                    }

                    // Allow the app to process the bitmap if requested
                    if (_handler != null)
                    {
                        _handler(surface, canvasBitmap, device);
                    }
                    else
                    {
                        // Draw the image to the surface
                        using (var session = CanvasComposition.CreateDrawingSession(surface))
                        {
                            session.Clear(Windows.UI.Color.FromArgb(0, 0, 0, 0));
                            session.DrawImage(canvasBitmap, new Rect(0, 0, surfaceSize.Width, surfaceSize.Height), new Rect(0, 0, bitmapSize.Width, bitmapSize.Height));
                        }
                    }
                }
            }
        }
    }
}
