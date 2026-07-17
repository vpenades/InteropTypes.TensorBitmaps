using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

using COLOR = System.Drawing.Color;
using POINT = System.Drawing.PointF;
using RECTANGLE = System.Drawing.RectangleF;

namespace InteropTypes.TensorBitmaps.Diagnostics
{
    public readonly ref struct DiagnosticsDrawing<TBitmap, TPixel>
            where TBitmap : IBitmapOperand<TBitmap, TPixel>, allows ref struct
            where TPixel : unmanaged
    {
        public DiagnosticsDrawing(TBitmap bitmap)
        {
            _Bitmap = bitmap;

            // System.Drawing.Color is Bgra8 format
            _Colors = IPixelConverter<int, TPixel>.Create(KnownPixelFormats.Bgra8, _Bitmap.Format, true);
        }

        private readonly TBitmap _Bitmap;
        private readonly IPixelConverter<int, TPixel> _Colors;

        public TPixel GetColorPixel(COLOR color)
        {
            Span<int> src = stackalloc int[1];
            Span<TPixel> dst = stackalloc TPixel[1];

            src[0] = color.ToArgb();
            _Colors.ConvertPixels(src, dst);
            return dst[0];
        }

        public void DrawRectangle(RECTANGLE rectf, COLOR color)
        {
            var a = new Vector2(rectf.X, rectf.Y);
            var b = new Vector2(rectf.X + rectf.Width, rectf.Y);
            var c = new Vector2(rectf.X + rectf.Width, rectf.Y + rectf.Height);
            var d = new Vector2(rectf.X, rectf.Y + rectf.Height);

            DrawLine(a, b, color);
            DrawLine(b, c, color);
            DrawLine(c, d, color);
            DrawLine(d, a, color);
        }

        public void DrawLine(POINT a, POINT b, COLOR color)
        {
            DrawLine(new Vector2(a.X, b.X), new Vector2(b.X, b.Y), color);
        }

        public void DrawLine(System.Numerics.Vector2 a, System.Numerics.Vector2 b, COLOR color)
        {
            var bounds = new System.Drawing.Rectangle(0,0, _Bitmap.Width,_Bitmap.Height);
            var pixel = GetColorPixel(color);

            var ab = b-a;

            if (Math.Abs(ab.X) <= 1 && Math.Abs(ab.Y) <= 1)
            // can't loop; draw a single pixel
            {
                int x = (int)a.X;
                int y = (int)a.Y;

                if (bounds.Contains(y, x)) _Bitmap.GetRowPixelsSpan(y)[x] = pixel;

                return;
            }

            if (Math.Abs(ab.X) > Math.Abs(ab.Y))
            // loop from left to right
            {
                var ptr = a;
                var max = b.X;

                if (ab.X < 0) { ab = -ab; ptr = b; max = a.X; }

                var d = new Vector2(1, ab.Y / ab.X);

                while (ptr.X <= max)
                {
                    int x = (int)ptr.X;
                    int y = (int)ptr.Y;

                    if (bounds.Contains(y, x)) _Bitmap.GetRowPixelsSpan(y)[x] = pixel;

                    ptr += d;
                }
            }
            else
            // loop from top to bottom
            {
                var ptr = a;
                var max = b.Y;

                if (ab.Y < 0) { ab = -ab; ptr = b; max = a.Y; }

                var d = new Vector2(ab.X / ab.Y, 1);

                while (ptr.Y <= max)
                {
                    int x = (int)ptr.X;
                    int y = (int)ptr.Y;

                    if (bounds.Contains(y, x)) _Bitmap.GetRowPixelsSpan(y)[x] = pixel;

                    ptr += d;
                }
            }
        }
    }
}
