using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.UI.Text;

namespace EffectiveEffect1607
{
    public sealed class TheEffect : IBasicVideoEffect
    {
        private IPropertySet _configuration;
        private CanvasDevice _canvasDevice;

        private int range = 1;
        private float displacementAmount = 0f;
        int speed = 10;
        int step = 10;

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (var inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(_canvasDevice, context.InputFrame.Direct3DSurface))
            using (var drawingSurface = CanvasRenderTarget.CreateFromDirect3D11Surface(_canvasDevice, context.OutputFrame.Direct3DSurface).CreateDrawingSession())
            {
                #region grayscale

                // grayscale effect
                var grayscaleEffect = new SaturationEffect()
                {
                    Saturation = 0,
                    Source = inputBitmap
                };

                #endregion


                #region fun

                step = (speed++ % 20) == 0 ? step * (new Random().Next(0, 1) * 2 - 1) : step;
                displacementAmount = (displacementAmount + step) % 200;

                var funEffect = new BorderEffect()
                {
                    ExtendX = CanvasEdgeBehavior.Wrap,
                    ExtendY = CanvasEdgeBehavior.Wrap,
                    Source = new ScaleEffect()
                    {
                        Source = new DisplacementMapEffect()
                        {
                            Source = inputBitmap,
                            Amount = displacementAmount + 50,
                            Displacement = new TurbulenceEffect()
                            {
                                Size = new Vector2(1024, 1024),
                                Noise = TurbulenceEffectNoise.Turbulence
                            }
                        },
                        Scale = new Vector2(1 / (float)Math.Pow(2, range), 1 / (float)Math.Pow(2, range))
                    }
                };

                var brush = new CanvasImageBrush(_canvasDevice, inputBitmap);
                var textCommandList = new CanvasCommandList(_canvasDevice);

                using (var clds = textCommandList.CreateDrawingSession())
                {
                    clds.DrawText(
                        "It's Effective",
                        (float)inputBitmap.Size.Width / 2,
                        (float)inputBitmap.Size.Height / 2,
                        brush,
                        new CanvasTextFormat()
                        {
                            FontSize = (float)inputBitmap.Size.Width / 7,
                            FontWeight = new FontWeight() { Weight = 999 },
                            HorizontalAlignment = CanvasHorizontalAlignment.Center,
                            VerticalAlignment = CanvasVerticalAlignment.Center
                        });
                }

                var playEffect = new CompositeEffect()
                {
                    Sources = { funEffect, textCommandList }
                };

                #endregion

                // replace grayscaleEffect with playEffect for
                // a fun experience
                drawingSurface.DrawImage(grayscaleEffect);
            }
        }

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            _canvasDevice = CanvasDevice.CreateFromDirect3D11Device(device);
        }

        public bool IsReadOnly { get { return false; } }

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var properties = new List<VideoEncodingProperties>();
                properties.Add(VideoEncodingProperties.CreateUncompressed("ARGB32", 640, 480));
                return properties;
            }
        }

        public MediaMemoryTypes SupportedMemoryTypes { get { return MediaMemoryTypes.Gpu; } }

        public bool TimeIndependent { get { return false; } }

        public void DiscardQueuedFrames() { }

        public void Close(MediaEffectClosedReason reason)
        {
            if (_canvasDevice != null)
                _canvasDevice.Dispose();
        }

        public void SetProperties(IPropertySet configuration)
        {
            _configuration = configuration;
        }
    }
}
