using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;

namespace SamplesCommon
{
    public class InteropDrawer : IContentDrawer
    {
        public InteropDrawer(InteropDrawHandler handler)
        {
            _handler = handler;
        }

        private InteropDrawHandler _handler = null;

        public Task Draw(CompositionGraphicsDevice device, Object drawingLock, CompositionDrawingSurface surface, Size size)
        {
            var canvasDevice = CanvasComposition.GetCanvasDevice(device);
            lock (drawingLock)
            {
                if (_handler != null)
                {
                    _handler(surface, device);
                }
            }

            return Task.CompletedTask;
        }
    }
}
