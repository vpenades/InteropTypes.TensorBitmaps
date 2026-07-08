using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InteropTypes.TensorBitmaps
{
    public interface IPixelConverter<TSrcPixel, TDstPixel>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        void ConvertPixels(ReadOnlySpan<TSrcPixel> source, Span<TDstPixel> target);
    }           

    static class PixelConverters
    {
        public static IPixelConverter<TSrcPixel, TDstPixel> Create<TSrc, TSrcPixel, TDst, TDstPixel>(TensorPixelFormat sourceFmt, TensorPixelFormat targetFmt, bool initPixels)
            where TSrc : unmanaged, INumber<TSrc>
            where TSrcPixel : unmanaged
            where TDst : unmanaged, INumber<TDst>
            where TDstPixel : unmanaged
        {
            // direct converter when both types and formats are exactly the same:

            if (typeof(TSrcPixel) == typeof(TDstPixel) && sourceFmt == targetFmt)
            {
                return new DirectPixelConverter<TSrcPixel, TDstPixel>();
            }

            // converters using floating point:

            if (typeof(TSrc) == typeof(double) || typeof(TDst) == typeof(double))
            {
                return new ComponentWisePixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, ComponentConverter<TSrc, double, TDst>>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            if (typeof(TSrc) == typeof(float) || typeof(TDst) == typeof(float))
            {
                return new ComponentWisePixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, ComponentConverter<TSrc, float, TDst>>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            if (typeof(TSrc) == typeof(Half) || typeof(TDst) == typeof(Half))
            {
                return new ComponentWisePixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, ComponentConverter<TSrc, Half, TDst>>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            // converter using integer:
            
            return new ComponentWisePixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, ComponentConverterInteger<TSrc, TDst>>(sourceFmt.Components, targetFmt.Components, initPixels);            
        }
    }

    /// <summary>
    /// When src and dst pixels match in type and format, use fast copy.
    /// </summary>
    /// <typeparam name="TSrcPixel"></typeparam>
    /// <typeparam name="TDstPixel"></typeparam>
    sealed class DirectPixelConverter<TSrcPixel, TDstPixel> : IPixelConverter<TSrcPixel, TDstPixel>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public void ConvertPixels(ReadOnlySpan<TSrcPixel> source, Span<TDstPixel> target)
        {
            var sourcex = System.Runtime.InteropServices.MemoryMarshal.Cast<TSrcPixel, TDstPixel>(source);

            var len = Math.Min(sourcex.Length, target.Length);
            sourcex.Slice(0, len).CopyTo(target);
        }
    }


    /// <summary>
    /// Naive Component per component pixel converter
    /// </summary>    
    sealed class ComponentWisePixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, TConverter> : IPixelConverter<TSrcPixel,TDstPixel>
        where TSrc: unmanaged, INumber<TSrc>
        where TSrcPixel: unmanaged
        where TDst: unmanaged, INumber<TDst>
        where TDstPixel: unmanaged
        where TConverter : IComponentConverter<TSrc, TDst>
    {
        public ComponentWisePixelConverter(IReadOnlyList<TensorPixelComponent> src, IReadOnlyList<TensorPixelComponent> dst, bool initPixels)
        {
            var ccc = new List<TConverter>();
            var uuu = new List<(int, TDst)>();

            for (int i = 0; i < dst.Count; ++i)
            {
                var srcIdx = -1;
                for (int j = 0; j < src.Count; ++j)
                {
                    if (src[j].Semantic == dst[i].Semantic) { srcIdx = j; break; }
                }
                
                var dc = dst[i] as TensorPixelComponent<TDst>;
                if (dc == null) continue;

                if (srcIdx < 0)
                {                    
                    uuu.Add((i, dc.DefaultValue));
                    continue;
                }

                var sc = src[srcIdx] as TensorPixelComponent<TSrc>;
                if (sc == null) continue;

                var c = CreateComponentConverter(sc, dc, srcIdx, i);
                ccc.Add(c);
            }

            if (!initPixels) uuu.Clear();

            _Converters = ccc.ToArray();
            if (uuu.Count > 0) _Unfilled = uuu.ToArray();
        }

        private static TConverter CreateComponentConverter(TensorPixelComponent<TSrc> src, TensorPixelComponent<TDst> dst, int srcIdx, int tgtIdx)
        {
            if (typeof(TConverter) == typeof(ComponentConverterInteger<TSrc, TDst>))
            {
                return (TConverter)(object)new ComponentConverterInteger<TSrc, TDst>(src, dst, srcIdx, tgtIdx);
            }

            if (typeof(TConverter) == typeof(ComponentConverter<TSrc, Half, TDst>))
            {
                return (TConverter)(object)new ComponentConverter<TSrc, Half, TDst>(src, dst, srcIdx, tgtIdx);
            }

            if (typeof(TConverter) == typeof(ComponentConverter<TSrc, float, TDst>))
            {
                return (TConverter)(object)new ComponentConverter<TSrc, float, TDst>(src, dst, srcIdx, tgtIdx);
            }

            if (typeof(TConverter) == typeof(ComponentConverter<TSrc, double, TDst>))
            {
                return (TConverter)(object)new ComponentConverter<TSrc, double, TDst>(src, dst, srcIdx, tgtIdx);
            }

            throw new NotImplementedException(typeof(TConverter).Name);
        }

        private readonly TConverter[] _Converters;

        private readonly (int, TDst)[] _Unfilled;

        public void ConvertPixels(ReadOnlySpan<TSrcPixel> src, Span<TDstPixel> dst)
        {
            var len = Math.Min(src.Length, dst.Length);            

            var srcc = System.Runtime.InteropServices.MemoryMarshal.Cast<TSrcPixel, TSrc>(src.Slice(0, len));
            var dstc = System.Runtime.InteropServices.MemoryMarshal.Cast<TDstPixel, TDst>(dst.Slice(0, len));
            int srcstride = Unsafe.SizeOf<TSrcPixel>() / Unsafe.SizeOf<TSrc>();
            int dststride = Unsafe.SizeOf<TDstPixel>() / Unsafe.SizeOf<TDst>();

            if (_Unfilled == null)
            {
                while (dstc.Length > 0)
                {
                    CopyPixel(srcc, dstc);
                    srcc = srcc.Slice(srcstride);
                    dstc = dstc.Slice(dststride);
                }
            }
            else
            {
                while (dstc.Length > 0)
                {
                    CopyPixel(srcc, dstc);

                    foreach(var (idx,v) in _Unfilled) { dstc[idx] = v; }

                    srcc = srcc.Slice(srcstride);
                    dstc = dstc.Slice(dststride);
                }
            }
        }        

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private void CopyPixel(ReadOnlySpan<TSrc> srcPixel, Span<TDst> dstPixel)
        {
            for (int i = 0; i < _Converters.Length; ++i)
            {
                var cvt = _Converters[i];

                dstPixel[cvt.TargetIndex] = cvt.Convert(srcPixel[cvt.SourceIndex]);
            }
        }        
    }

    interface IComponentConverter<TSrc, TDst>
        where TSrc:unmanaged, INumber<TSrc>
        where TDst:unmanaged, INumber<TDst>
    {
        /// <summary>
        /// Index to the component in the source pixel.
        /// </summary>
        int SourceIndex { get; }

        /// <summary>
        /// Index to the component in the target pixel.
        /// </summary>
        int TargetIndex { get; }

        /// <summary>
        /// Converts the component from src to dst
        /// </summary>
        TDst Convert(TSrc value);
    }    

    /// <summary>
    /// Generic converter that uses an intermediate type to convert between src and dst
    /// </summary>
    /// <typeparam name="TSrc"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TDst"></typeparam>
    sealed class ComponentConverter<TSrc, T, TDst> : IComponentConverter<TSrc, TDst>
        where TSrc : unmanaged, INumber<TSrc>
        where T : unmanaged, INumber<T>
        where TDst : unmanaged, INumber<TDst>
    {
        public ComponentConverter(TensorPixelComponent<TSrc> src, TensorPixelComponent<TDst> dst, int srcIdx, int tgtIdx)
        {
            SourceIndex = srcIdx;

            _SrcOffset = -T.CreateChecked(src.MinValue);

            var s = T.CreateChecked(src.MaxValue - src.MinValue);
            var d = T.CreateChecked(dst.MaxValue - dst.MinValue);

            _Scale = T.IsZero(s) ? T.CreateChecked(0) : d / s;

            _DstOffset = T.CreateChecked(dst.MinValue);

            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly T _SrcOffset;
        private readonly T _Scale;
        private readonly T _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TDst Convert(TSrc value)
        {
            var v = T.CreateChecked(value);

            v += _SrcOffset;
            v *= _Scale;            
            v += _DstOffset;

            return TDst.CreateChecked(v);
        }
    }

    /// <summary>
    /// Specialised converter for integer to integer
    /// </summary>
    /// <typeparam name="TSrc"></typeparam>
    /// <typeparam name="TDst"></typeparam>
    sealed class ComponentConverterInteger<TSrc, TDst> : IComponentConverter<TSrc, TDst>
        where TSrc : unmanaged, INumber<TSrc>
        where TDst : unmanaged, INumber<TDst>
    {
        public ComponentConverterInteger(TensorPixelComponent<TSrc> src, TensorPixelComponent<TDst> dst, int srcIdx, int tgtIdx)
        {
            SourceIndex = srcIdx;

            _SrcOffset = -nint.CreateChecked(src.MinValue);

            var s = nint.CreateChecked(src.MaxValue - src.MinValue);
            var d = nint.CreateChecked(dst.MaxValue - dst.MinValue);

            d <<= 16;

            _Scale = s == 0 ? 0 : d / s;

            _DstOffset = nint.CreateChecked(dst.MinValue);

            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly nint _SrcOffset;
        private readonly nint _Scale;
        private readonly nint _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TDst Convert(TSrc value)
        {
            var v = nint.CreateChecked(value);

            v += _SrcOffset;
            v *= _Scale;
            v >>= 16;
            v += _DstOffset;

            return TDst.CreateChecked(v);
        }
    }
}
