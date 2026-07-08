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
            where TSrc : unmanaged
            where TSrcPixel : unmanaged
            where TDst : unmanaged
            where TDstPixel : unmanaged
        {
            if (typeof(TSrcPixel) == typeof(TDstPixel) && sourceFmt == targetFmt)
            {
                return new DirectConverter<TSrcPixel, TDstPixel>();
            }

            if (typeof(TSrc) == typeof(byte) && typeof(TDst) == typeof(byte))
            {
                return new PixelConverter<byte, TSrcPixel, byte, TDstPixel, ComponentConverterByteByte>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            if (typeof(TSrc) == typeof(byte) && typeof(TDst) == typeof(float))
            {
                return new PixelConverter<byte, TSrcPixel, float, TDstPixel, ComponentConverterByteFloat>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            if (typeof(TSrc) == typeof(float) && typeof(TDst) == typeof(byte))
            {
                return new PixelConverter<float, TSrcPixel, byte, TDstPixel, ComponentConverterFloatByte>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            if (typeof(TSrc) == typeof(float) && typeof(TDst) == typeof(float))
            {
                return new PixelConverter<float, TSrcPixel, float, TDstPixel, ComponentConverterFloatFloat>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            throw new NotImplementedException($"{typeof(TSrc).Name} to {typeof(TDst).Name}");
        }
    }

    /// <summary>
    /// When src and dst pixels match in type and format, use fast copy.
    /// </summary>
    /// <typeparam name="TSrcPixel"></typeparam>
    /// <typeparam name="TDstPixel"></typeparam>
    sealed class DirectConverter<TSrcPixel, TDstPixel> : IPixelConverter<TSrcPixel, TDstPixel>
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
    /// this is a naive, component by component pixel converter
    /// </summary>
    /// <remarks>
    /// A more complex converter could take advantage of SIMD to convert entire pixel rows, but having this now is better than nothing.
    /// </remarks>    
    sealed class PixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, TConverter> : IPixelConverter<TSrcPixel,TDstPixel>
        where TSrc: unmanaged
        where TSrcPixel: unmanaged
        where TDst: unmanaged
        where TDstPixel: unmanaged
        where TConverter : IComponentConverter<TSrc, TDst>
    {
        public PixelConverter(IReadOnlyList<TensorPixelComponent> src, IReadOnlyList<TensorPixelComponent> dst, bool initPixels)
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

                var c = (TConverter)IComponentConverter<TSrc, TDst>.CreateFrom(sc, dc, srcIdx, i);
                ccc.Add(c);
            }

            if (!initPixels) uuu.Clear();

            _Converters = ccc.ToArray();
            if (uuu.Count > 0) _Unfilled = uuu.ToArray();
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
        where TSrc:unmanaged
        where TDst:unmanaged
    {
        public static IComponentConverter<TSrc,TDst> CreateFrom(TensorPixelComponent<TSrc> src, TensorPixelComponent<TDst> dst, int srcIdx,int tgtIdx)
        {
            if (src is TensorPixelComponent<byte> srcB)
            {
                switch(dst)
                {
                    case TensorPixelComponent<byte> dstB: return new ComponentConverterByteByte(srcB, dstB, srcIdx,tgtIdx) as IComponentConverter<TSrc, TDst>;
                    case TensorPixelComponent<float> dstF: return new ComponentConverterByteFloat(srcB, dstF, srcIdx, tgtIdx) as IComponentConverter<TSrc, TDst>;
                }

            }

            if (src is TensorPixelComponent<float> srcF)
            {
                switch (dst)
                {
                    case TensorPixelComponent<byte> dstB: return new ComponentConverterFloatByte(srcF, dstB, srcIdx, tgtIdx) as IComponentConverter<TSrc, TDst>;
                    case TensorPixelComponent<float> dstF: return new ComponentConverterFloatFloat(srcF, dstF, srcIdx, tgtIdx) as IComponentConverter<TSrc, TDst>;
                }
            }

            throw new NotImplementedException();
        }

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

    sealed class ComponentConverterFloatFloat : IComponentConverter<float, float>
    {
        public ComponentConverterFloatFloat(TensorPixelComponent<float> src, TensorPixelComponent<float> dst, int srcIdx, int tgtIdx)
        {
            _SrcOffset = -src.MinValue;
            _Scale = (dst.MaxValue - dst.MinValue) / (src.MaxValue - src.MinValue);
            _DstOffset = dst.MinValue;
            SourceIndex = srcIdx;
            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly float _SrcOffset;
        private readonly float _Scale;                
        private readonly float _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public float Convert(float value)
        {
            value += _SrcOffset;
            value *= _Scale;
            value += _DstOffset;
            return value;            
        }        
    }

    sealed class ComponentConverterFloatByte : IComponentConverter<float, byte>
    {
        public ComponentConverterFloatByte(TensorPixelComponent<float> src, TensorPixelComponent<byte> dst, int srcIdx, int tgtIdx)
        {
            _SrcOffset = -src.MinValue;
            _Scale = (dst.MaxValue - dst.MinValue) / (src.MaxValue - src.MinValue);
            _DstOffset = dst.MinValue;
            SourceIndex = srcIdx;
            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly float _SrcOffset;
        private readonly float _Scale;
        private readonly float _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public byte Convert(float value)
        {
            value += _SrcOffset;
            value *= _Scale;
            value += _DstOffset;
            return (byte)value;
        }
    }

    sealed class ComponentConverterByteFloat : IComponentConverter<byte, float>
    {
        public ComponentConverterByteFloat(TensorPixelComponent<byte> src, TensorPixelComponent<float> dst, int srcIdx, int tgtIdx)
        {
            _SrcOffset = -src.MinValue;
            _Scale = (dst.MaxValue - dst.MinValue) / (src.MaxValue - src.MinValue);
            _DstOffset = dst.MinValue;
            SourceIndex = srcIdx;
            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly float _SrcOffset;
        private readonly float _Scale;
        private readonly float _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public float Convert(byte value)
        {
            float t = value;
            t += _SrcOffset;
            t *= _Scale;
            t += _DstOffset;
            return t;
        }
    }

    sealed class ComponentConverterByteByte : IComponentConverter<byte, byte>
    {
        public ComponentConverterByteByte(TensorPixelComponent<byte> src, TensorPixelComponent<byte> dst, int srcIdx,int tgtIdx)
        {
            _SrcOffset = -(int)src.MinValue;
            _Scale = ((int)dst.MaxValue - (int)dst.MinValue) * 65536 / ((int)src.MaxValue - (int)src.MinValue);
            _DstOffset = (int)dst.MinValue;
            SourceIndex = srcIdx;
            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly int _SrcOffset;
        private readonly int _Scale;
        private readonly int _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public byte Convert(byte value)
        {
            int t = value;
            t += _SrcOffset;
            t *= _Scale;
            t >>= 16;
            t += _DstOffset;
            return (byte)t;
        }
    }
}
