using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace InteropTypes.Numerics.BitmapOperators
{
    public readonly ref struct DiagnosticsDrawing<TBitmap, TPixel>
            where TBitmap : IBitmapOperand<TBitmap, TPixel>, allows ref struct
            where TPixel : unmanaged
    {

        private readonly TBitmap _Bitmap;

        public DiagnosticsDrawing(TBitmap bitmap)
        {
            _Bitmap = bitmap;
        }

        public void DrawRectangle(System.Drawing.RectangleF rectf, System.Drawing.Color color)
        {
            var rect = System.Drawing.Rectangle.Truncate(rectf);
            DrawRectangle(rect, color);
        }

        public void DrawRectangle(System.Drawing.Rectangle rect, System.Drawing.Color color)            
        {
            if (rect.X >= _Bitmap.Width) return;
            if (rect.X + rect.Width <= 0) return;

            if (!_Bitmap.Format.TryGetPixelValue<TPixel>(color, out var pixel)) return;
            

            for(int y=0; y < rect.Height; ++y)
            {
                var yy = rect.Y + y;

                if (yy < 0 || yy >= _Bitmap.Height) continue;

                var row = _Bitmap.GetRowPixelsSpan(yy);

                row = row.Slice(rect.X);
                row = row.Slice(0, Math.Min(rect.Width, row.Length));

                if (y == 0 || y == rect.Height - 1) row.Fill(pixel);
                else
                {
                    row[0] = pixel;
                    row[row.Length-1] = pixel;
                }                
            }
        }

    }
}
