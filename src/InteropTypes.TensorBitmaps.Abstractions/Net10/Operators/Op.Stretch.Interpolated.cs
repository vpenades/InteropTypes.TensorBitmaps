using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps.Operators
{
    readonly struct _InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>
       : IFillOperation<TSrcPixel, TDstPixel, Matrix3x2>
       where TSrcPixel : unmanaged
       where TDstPixel : unmanaged
    {

        public static _InterpolatedStretchToFitOperator<TSrcPixel,TDstPixel> CreateBicubic()
        {
            return new _InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>(new WeightContributions.Bicubic());
        }

        public static _InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel> CreateLanczos2()
        {
            return new _InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>(new WeightContributions.Lanczos(2));
        }

        public static _InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel> CreateLanczos3()
        {
            return new _InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>(new WeightContributions.Lanczos(3));
        }

        public _InterpolatedStretchToFitOperator(WeightContributions.IFactory algorythm)
        {
            _Algorythm = algorythm;
        }

        private readonly WeightContributions.IFactory _Algorythm;

        public Matrix3x2 Fill<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IBitmapReader<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            var ccc = src.Format.Components.Select(c => new PixelComponent<float>(c.Semantic)).ToImmutableArray();
            var fmt = new PixelFormat(ccc);

            switch (src.Format.Components.Length)
            {
                case 3:
                    return new _InterpolatedStretchToFitOperator<TSrcPixel, XYZ, TDstPixel>(fmt, _Algorythm, XYZ.GammaToLinear, XYZ.LinearToGamma).Fill(src, dst, null);
                case 4:
                    return new _InterpolatedStretchToFitOperator<TSrcPixel, XYZW, TDstPixel>(fmt, _Algorythm, XYZW.GammaIn, XYZW.GammaOut).Fill(src, dst, null);

                default: throw new NotImplementedException();
            }
        }

        public struct XYZ : InteropTypes.Numerics.IPixel<XYZ>
        {
            public XYZ(Vector3 packed)
            {
                Packed = packed;
            }

            public Vector3 Packed;

            static XYZ IAdditionOperators<XYZ, XYZ, XYZ>.operator +(XYZ left, XYZ right) { return new XYZ(left.Packed + right.Packed); }
            static XYZ IMultiplyOperators<XYZ, float, XYZ>.operator *(XYZ left, float right) { return new XYZ(left.Packed * right); }
            public PixelFormat Format => KnownPixelFormats.RgbF32;
            static XYZ IPixel<XYZ>.Zero => default;
            public XYZ Saturated() { return new XYZ(Vector3.Clamp(Packed, Vector3.Zero, Vector3.One)); }

            public static void GammaToLinear(Span<XYZ> pixels)
            {
                for(int i=0; i < pixels.Length; ++i)
                {
                    var p = pixels[i].Packed;
                    p.X = MathF.Pow(p.X, 2.2f);
                    p.Y = MathF.Pow(p.Y, 2.2f);
                    p.Z = MathF.Pow(p.Z, 2.2f);
                    pixels[i] = new XYZ(p);
                    System.Diagnostics.Debug.Assert(!pixels[i].IsNaN());
                }
            }

            public static void LinearToGamma(Span<XYZ> pixels)
            {
                const float pow = 1f / 2.2f;

                for (int i = 0; i < pixels.Length; ++i)
                {
                    var p = pixels[i].Packed;
                    p = Vector3.Max(Vector3.Zero, p);
                    p.X = MathF.Pow(p.X, pow);
                    p.Y = MathF.Pow(p.Y, pow);
                    p.Z = MathF.Pow(p.Z, pow);
                    pixels[i] = new XYZ(p);
                    System.Diagnostics.Debug.Assert(!pixels[i].IsNaN());
                }
            }

            public bool IsNaN()
            {
                return float.IsNaN(Packed.X) || float.IsNaN(Packed.Y) || float.IsNaN(Packed.Z);
            }
        }

        public struct XYZW : InteropTypes.Numerics.IPixel<XYZW>
        {
            public XYZW(Vector4 packed)
            {
                Packed = packed;
            }

            public Vector4 Packed;

            static XYZW IAdditionOperators<XYZW, XYZW, XYZW>.operator +(XYZW left, XYZW right) { return new XYZW(left.Packed + right.Packed); }
            static XYZW IMultiplyOperators<XYZW, float, XYZW>.operator *(XYZW left, float right) { return new XYZW(left.Packed * right); }
            public PixelFormat Format => KnownPixelFormats.RgbF32;
            static XYZW IPixel<XYZW>.Zero => default;
            public XYZW Saturated() { return new XYZW(Vector4.Clamp(Packed, Vector4.Zero, Vector4.One)); }

            public static void GammaIn(Span<XYZW> pixels)
            {
                for (int i = 0; i < pixels.Length; ++i)
                {
                    var p = pixels[i].Packed;
                    p.X = MathF.Pow(p.X, 2.2f);
                    p.Y = MathF.Pow(p.Y, 2.2f);
                    p.Z = MathF.Pow(p.Z, 2.2f);
                    p.W = MathF.Pow(p.W, 2.2f);
                    pixels[i] = new XYZW(p);
                    System.Diagnostics.Debug.Assert(!pixels[i].IsNaN());
                }
            }

            public static void GammaOut(Span<XYZW> pixels)
            {
                const float pow = 1f / 2.2f;

                for (int i = 0; i < pixels.Length; ++i)
                {                    
                    var p = pixels[i].Packed;
                    p = Vector4.Max(Vector4.Zero, p);
                    p.X = MathF.Pow(p.X, pow);
                    p.Y = MathF.Pow(p.Y, pow);
                    p.Z = MathF.Pow(p.Z, pow);
                    p.W = MathF.Pow(p.W, pow);
                    pixels[i] = new XYZW(p);                 
                }
            }

            public bool IsNaN()
            {
                return float.IsNaN(Packed.X) || float.IsNaN(Packed.Y) || float.IsNaN(Packed.Z) || float.IsNaN(Packed.W);
            }
        }
    }

    readonly struct _InterpolatedStretchToFitOperator<TSrcPixel, TPixel, TDstPixel>
        : IFillOperation<TSrcPixel, TDstPixel, Matrix3x2>
        where TSrcPixel : unmanaged
        where TPixel : unmanaged, IPixel<TPixel>
        where TDstPixel : unmanaged
    {
        #region lifecycle
        public _InterpolatedStretchToFitOperator(PixelFormat fmt, WeightContributions.IFactory algorythm, ProcessPixelsDelegate gtl, ProcessPixelsDelegate ltg)
        {
            _Format = fmt;
            _Algorythm = algorythm;
            _GammaToLinear = gtl;
            _LinearToGamma = ltg;
        }

        #endregion

        #region data

        private readonly PixelFormat _Format;
        private readonly WeightContributions.IFactory _Algorythm;

        public delegate void ProcessPixelsDelegate(Span<TPixel> pixels);

        private readonly ProcessPixelsDelegate _GammaToLinear;
        private readonly ProcessPixelsDelegate _LinearToGamma;

        #endregion

        #region API

        public Matrix3x2 Fill<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IBitmapReader<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            // Precompute horizontal and vertical contributor lists
            var hrz = _Algorythm.CreateContributions(src.Width, dst.Width);
            var vrt = _Algorythm.CreateContributions(src.Height, dst.Height);

            Span<TSrcPixel> srcRow = stackalloc TSrcPixel[src.Width];
            var srcToTmp = IPixelConverter<TSrcPixel, TPixel>.Create(src.Format, _Format, true);
            Span<TPixel> srcTmp = stackalloc TPixel[src.Width];

            Span<TPixel> dstTmp = stackalloc TPixel[dst.Width];
            var tmpToDst = IPixelConverter<TPixel, TDstPixel>.Create(_Format, dst.Format, true);
            Span<TDstPixel> dstRow = stackalloc TDstPixel[dst.Width];

            for (int yDst = 0; yDst < dst.Height; yDst++)
            {
                var vContrib = vrt[yDst];

                dstTmp.Fill(TPixel.Zero);

                // For each contributing source row
                for (int k = 0; k < vContrib.Count; k++)
                {
                    int srcY = vContrib.Start + k;
                    float vWeight = vContrib.Weights[k];

                    // Pull the source row
                    src.ReadRowPixelsSpan(Math.Clamp(srcY, 0, src.Height - 1), srcRow);
                    srcToTmp.ConvertPixels(srcRow, srcTmp);
                    _GammaToLinear(srcTmp);

                    // For each dst column, compute horizontal sample on-the-fly using the precomputed horizontal weights,
                    // then multiply by the vertical weight and accumulate.
                    // This approach avoids creating a full intermediate row.
                    for (int xDst = 0; xDst < dst.Width; xDst++)
                    {
                        var hContrib = hrz[xDst];
                        var sample = TPixel.Zero;
                        int start = hContrib.Start;
                        var w = hContrib.Weights;
                        int len = w.Length;

                        // Weighted sum over source X's for this destination X
                        // Clamp source x indices to valid bounds
                        int srcIndex = start;
                        int srcMax = src.Width - 1;
                        for (int i = 0; i < len; i++, srcIndex++)
                        {
                            int xi = srcIndex;
                            if (xi < 0) xi = 0;
                            else if (xi > srcMax) xi = srcMax;
                            sample += srcTmp[xi] * w[i];
                        }

                        System.Diagnostics.Debug.Assert(!sample.IsNaN());

                        // accumulate vertical weight
                        dstTmp[xDst] += sample * vWeight;
                    }
                }                

                _LinearToGamma(dstTmp);

                for (int k = 0; k < dstTmp.Length; ++k)
                {
                    dstTmp[k] = dstTmp[k].Saturated();
                    System.Diagnostics.Debug.Assert(!dstTmp[k].IsNaN());
                }

                tmpToDst.ConvertPixels(dstTmp, dstRow);
                dst.WriteRowPixelsSpan(yDst, dstRow);
            }

            return Matrix3x2.CreateScale(src.Width / (float)dst.Width, src.Height / (float)dst.Height);
        }

        #endregion
    }

    /// <summary>
    /// Contributor structure: start index and weights array (length = Count)
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{string.Join(' ', Weights)}")]
    readonly struct WeightContributions
    {
        #region lifecycle            

        private WeightContributions(int start, float[] weights)
        {
            System.Diagnostics.Debug.Assert(weights.Length > 0);
            System.Diagnostics.Debug.Assert(weights.All(item => !float.IsNaN(item)));
            System.Diagnostics.Debug.Assert(Math.Abs(weights.Sum() - 1) < 0.000001f);

            Start = start;
            Weights = weights;
        }

        #endregion

        #region data

        public int Start { get; }
        public float[] Weights { get; }
        public int Count => Weights.Length;

        #endregion

        #region nested

        public interface IFactory
        {
            WeightContributions[] CreateContributions(int srcSize, int dstSize);
        }

        public class Bicubic : IFactory
        {
            public WeightContributions[] CreateContributions(int srcSize, int dstSize)
            {
                if (srcSize <= 0) throw new ArgumentOutOfRangeException(nameof(srcSize));
                if (dstSize <= 0) throw new ArgumentOutOfRangeException(nameof(dstSize));

                var result = new WeightContributions[dstSize];

                double scale = (double)srcSize / dstSize;

                for (int i = 0; i < dstSize; i++)
                {
                    // Map dst pixel center to source space
                    double center = (i + 0.5) * scale - 0.5;

                    // Initial left index (we take four samples: floor(center)-1 .. floor(center)+2)
                    int left = (int)Math.Floor(center) - 1;

                    // Clamp left so [left .. left+3] lies inside [0 .. srcSize-1]
                    int maxLeft = Math.Max(0, srcSize - 4);
                    int start = Math.Min(Math.Max(0, left), maxLeft);

                    var weights = new float[4];
                    for (int j = 0; j < 4; j++)
                    {
                        double pos = start + j;
                        double w = Cubic(center - pos);
                        weights[j] = (float)w;
                    }

                    // Normalize weights so they sum to 1 (avoid darkening/brightening)
                    float sum = weights.Sum();
                    if (sum != 0f)
                    {
                        for (int j = 0; j < weights.Length; j++) weights[j] /= sum;
                    }

                    result[i] = new WeightContributions(start, weights);
                }

                return result;
            }

            // Cubic kernel (Catmull-Rom / a = -0.5). Input x is distance (can be negative).
            private static double Cubic(double x)
            {
                x = Math.Abs(x);
                const double a = -0.5; // Catmull-Rom
                if (x <= 1.0)
                {
                    return (a + 2.0) * Math.Pow(x, 3) - (a + 3.0) * Math.Pow(x, 2) + 1.0;
                }
                else if (x < 2.0)
                {
                    return a * Math.Pow(x, 3) - 5.0 * a * Math.Pow(x, 2) + 8.0 * a * x - 4.0 * a;
                }
                else
                {
                    return 0.0;
                }
            }
        }

        public class Lanczos : IFactory
        {
            public Lanczos(int a)
            {
                A = a;
            }

            public int A { get; set; }

            public WeightContributions[] CreateContributions(int srcSize, int dstSize)
            {
                if (A <= 0) throw new InvalidOperationException();

                var scale = (double)dstSize / (double)srcSize;

                var result = new WeightContributions[dstSize];
                double invScale = scale > 0 ? 1.0 / scale : 1.0;
                // For mapping dst -> src center: center = (dst + 0.5) / scale - 0.5  (pixel center mapping)
                for (int i = 0; i < dstSize; i++)
                {
                    double srcCenter = (i + 0.5) * invScale - 0.5;
                    // support window in source space is 'a' (Lanczos)
                    int left = (int)Math.Ceiling(srcCenter - this.A);
                    int right = (int)Math.Floor(srcCenter + this.A);

                    int len = right - left + 1;
                    var weights = new float[len];
                    double sum = 0.0;
                    for (int j = 0; j < len; j++)
                    {
                        int srcIndex = left + j;
                        double x = srcCenter - srcIndex;
                        double w = LanczosKernel(x, this.A);
                        weights[j] = (float)w;
                        sum += w;
                    }

                    // Normalize weights to sum to 1.0 to preserve brightness
                    if (sum != 0.0)
                    {
                        double invSum = 1.0 / sum;
                        for (int j = 0; j < len; j++) weights[j] = (float)(weights[j] * invSum);
                    }

                    result[i] = new WeightContributions(left, weights);
                }

                return result;
            }

            // Lanczos kernel L(x) = sinc(x) * sinc(x/a) for |x| < a, else 0. sinc(0)=1.
            private static double LanczosKernel(double x, int a)
            {
                x = Math.Abs(x);
                if (x < 1e-12) return 1.0;
                if (x >= a) return 0.0;
                return Sinc(Math.PI * x) * Sinc(Math.PI * x / a);
            }

            // sinc(t) = sin(t) / t
            private static double Sinc(double t)
            {
                if (Math.Abs(t) < 1e-12) return 1.0;
                return Math.Sin(t) / t;
            }
        }

        #endregion

    }
}
