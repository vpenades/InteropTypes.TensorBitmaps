using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps.Primitives
{
    public readonly ref struct Kernel5x5<TPixel>
        where TPixel : unmanaged
    {
        #region delegates

        public delegate void RowPixelDelegate(int x, Kernel5x5<TPixel> kernel);

        public delegate void BitmapPixelDelegate(int x, int y, Kernel5x5<TPixel> kernel);

        private delegate ReadOnlySpan<TSrcPixel> _RowPixelsDelegate<TSrcPixel>(int y) where TSrcPixel : unmanaged;

        #endregion

        #region lifecycle
        public Kernel5x5(ReadOnlySpan<TPixel> row0, ReadOnlySpan<TPixel> row1, ReadOnlySpan<TPixel> row2, ReadOnlySpan<TPixel> row3, ReadOnlySpan<TPixel> row4)
        {
            System.Diagnostics.Debug.Assert(row0.Length == 5);
            System.Diagnostics.Debug.Assert(row1.Length == 5);
            System.Diagnostics.Debug.Assert(row2.Length == 5);
            System.Diagnostics.Debug.Assert(row3.Length == 5);
            System.Diagnostics.Debug.Assert(row4.Length == 5);

            Row0 = row0;
            Row1 = row1;
            Row2 = row2;
            Row3 = row3;
            Row4 = row4;
        }

        #endregion

        #region data

        public readonly ReadOnlySpan<TPixel> Row0;
        public readonly ReadOnlySpan<TPixel> Row1;
        public readonly ReadOnlySpan<TPixel> Row2;
        public readonly ReadOnlySpan<TPixel> Row3;
        public readonly ReadOnlySpan<TPixel> Row4;

        #endregion

        #region properties

        public TPixel Center => Row2[2];

        public TPixel LaplacianLeft => Row2[1];
        public TPixel LaplacianRight => Row2[3];

        public TPixel LaplacianTop => Row1[2];
        public TPixel LaplacianBottom => Row3[2];

        #endregion

        #region API - direct

        public static void ProcessBitmap(IReadOnlyBitmap<TPixel> src, BitmapPixelDelegate kernelAction)
        {
            if (src.Width <= 0 || src.Height <= 0) return;

            for (int y = 0; y < src.Height; ++y)
            {
                void _bmpDelegate(int x, Kernel5x5<TPixel> k)
                {
                    kernelAction(x, y, k);
                }

                ProcessRow(src, y, _bmpDelegate);
            }
        }

        public static void ProcessRow(IReadOnlyBitmap<TPixel> src, int y, RowPixelDelegate kernelAction)
        {
            if (src.Width <= 0 || src.Height <= 0) return;

            Span<TPixel> tmp = stackalloc TPixel[5 * 5];

            int maxx = src.Width - 1;
            int maxy = src.Height - 1;

            var r0 = src.GetRowPixelsSpan(Math.Max(0, y - 2));
            var r1 = src.GetRowPixelsSpan(Math.Max(0, y - 1));
            var r2 = src.GetRowPixelsSpan(Math.Max(0, y + 0));
            var r3 = src.GetRowPixelsSpan(Math.Min(maxy, y + 1));
            var r4 = src.GetRowPixelsSpan(Math.Min(maxy, y + 2));

            for (int x = 0; x < src.Width; ++x)
            {
                int xx = x - 2;

                if (xx >= 0 && xx <= src.Width - 5)
                {
                    var k = new Kernel5x5<TPixel>(r0.Slice(xx, 5), r1.Slice(xx, 5), r2.Slice(xx, 5), r3.Slice(xx, 5), r4.Slice(xx, 5));
                    kernelAction(x, k);
                }
                else
                {
                    _FillKernel(tmp, x, maxx, r0, r1, r2, r3, r4);

                    var k = new Kernel5x5<TPixel>(tmp.Slice(0, 5), tmp.Slice(5, 5), tmp.Slice(10, 5), tmp.Slice(15, 5), tmp.Slice(20, 5));
                    kernelAction(x, k);
                }
            }
        }

        public static void ProcessBitmap<TBitmap>(TBitmap src, BitmapPixelDelegate kernelAction)
            where TBitmap : IReadOnlyBitmap<TBitmap, TPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
        {
            if (src.Width <= 0 || src.Height <= 0) return;

            for (int y = 0; y < src.Height; ++y)
            {
                void _bmpDelegate(int x, Kernel5x5<TPixel> k)
                {
                    kernelAction(x, y, k);
                }

                ProcessRow(src, y, _bmpDelegate);                
            }
        }

        public static void ProcessRow<TBitmap>(TBitmap src, int y, RowPixelDelegate kernelAction)
            where TBitmap : IReadOnlyBitmap<TBitmap, TPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
        {
            if (src.Width <= 0 || src.Height <= 0) return;

            Span<TPixel> tmp = stackalloc TPixel[5 * 5];

            int maxx = src.Width - 1;
            int maxy = src.Height - 1;
            
            var r0 = src.GetRowPixelsSpan(Math.Max(0, y - 2));
            var r1 = src.GetRowPixelsSpan(Math.Max(0, y - 1));
            var r2 = src.GetRowPixelsSpan(Math.Max(0, y + 0));
            var r3 = src.GetRowPixelsSpan(Math.Min(maxy, y + 1));
            var r4 = src.GetRowPixelsSpan(Math.Min(maxy, y + 2));

            for (int x = 0; x < src.Width; ++x)
            {
                int xx = x - 2;

                if (xx >= 0 && xx <= src.Width - 5)
                {
                    var k = new Kernel5x5<TPixel>(r0.Slice(xx, 5), r1.Slice(xx, 5), r2.Slice(xx, 5), r3.Slice(xx, 5), r4.Slice(xx, 5));
                    kernelAction(x, k);
                }
                else
                {
                    _FillKernel(tmp, x, maxx, r0, r1, r2, r3, r4);

                    var k = new Kernel5x5<TPixel>(tmp.Slice(0, 5), tmp.Slice(5, 5), tmp.Slice(10, 5), tmp.Slice(15, 5), tmp.Slice(20, 5));
                    kernelAction(x, k);
                }
            }            
        }        

        private static void _FillKernel(Span<TPixel> tmp, int x, int maxx, ReadOnlySpan<TPixel> r0, ReadOnlySpan<TPixel> r1, ReadOnlySpan<TPixel> r2, ReadOnlySpan<TPixel> r3, ReadOnlySpan<TPixel> r4)
        {
            var xll = Math.Max(0, x - 2);
            var xl = Math.Max(0, x - 1);
            var xr = Math.Min(maxx, x + 1);
            var xrr = Math.Min(maxx, x + 2);

            tmp[0] = r0[xll];
            tmp[1] = r0[xl];
            tmp[2] = r0[x];
            tmp[3] = r0[xr];
            tmp[4] = r0[xrr];

            tmp[5] = r1[xll];
            tmp[6] = r1[xl];
            tmp[7] = r1[x];
            tmp[8] = r1[xr];
            tmp[9] = r1[xrr];

            tmp[10] = r2[xll];
            tmp[11] = r2[xl];
            tmp[12] = r2[x];
            tmp[13] = r2[xr];
            tmp[14] = r2[xrr];

            tmp[15] = r3[xll];
            tmp[16] = r3[xl];
            tmp[17] = r3[x];
            tmp[18] = r3[xr];
            tmp[19] = r3[xrr];

            tmp[20] = r4[xll];
            tmp[21] = r4[xl];
            tmp[22] = r4[x];
            tmp[23] = r4[xr];
            tmp[24] = r4[xrr];
        }

        #endregion

        #region API with conversion

        public static void ProcessBitmap<TSrcPixel>(IReadOnlyBitmap<TSrcPixel> src, IPixelConverter<TSrcPixel,TPixel> converter, BitmapPixelDelegate kernelAction)
            where TSrcPixel: unmanaged
        {
            if (src.Width <= 0 || src.Height <= 0) return;

            var kwidth = src.Width + 4; // extend 2 pixels on each side to allocate edges
            Span<TPixel> row0 = stackalloc TPixel[kwidth];
            Span<TPixel> row1 = stackalloc TPixel[kwidth];
            Span<TPixel> row2 = stackalloc TPixel[kwidth];
            Span<TPixel> row3 = stackalloc TPixel[kwidth];
            Span<TPixel> row4 = stackalloc TPixel[kwidth];

            _FillRow(src.Height, src.GetRowPixelsSpan, -2, converter, row1);
            _FillRow(src.Height, src.GetRowPixelsSpan, -1, converter, row2);
            _FillRow(src.Height, src.GetRowPixelsSpan, 0, converter, row3);
            _FillRow(src.Height, src.GetRowPixelsSpan, 1, converter, row4);

            for (int y = 0; y < src.Height; ++y)
            {
                // roll
                var tmp = row0;
                row0 = row1;
                row1 = row2;
                row2 = row3;
                row3 = row4;
                row4 = tmp;

                // fill new one
                _FillRow(src.Height, src.GetRowPixelsSpan, y + 2, converter, row4);

                for (int x = 0; x < src.Width; ++x)
                {
                    var k = new Kernel5x5<TPixel>(row0.Slice(x, 5), row1.Slice(x, 5), row2.Slice(x, 5), row3.Slice(x, 5), row4.Slice(x, 5));
                    kernelAction(x, y, k);
                }
            }
        }

        private static void _FillRow<TSrcPixel>(int height, _RowPixelsDelegate<TSrcPixel> bitmapRows, int y, IPixelConverter<TSrcPixel, TPixel> pixelConverter, Span<TPixel> dst)
            where TSrcPixel: unmanaged
        {
            if (y < 0) y = -y;
            else if (y >= height) y -= 2 + y - height;
            y = Math.Clamp(y, 0, height - 1);

            var src = bitmapRows(y);

            pixelConverter.ConvertPixels(src, dst.Slice(2, src.Length));

            // over extend

            dst[1] = dst[3];
            dst[0] = dst[4];

            dst[dst.Length - 1] = dst[dst.Length - 5];
            dst[dst.Length - 2] = dst[dst.Length - 4];
        }

        #endregion
    }
}
