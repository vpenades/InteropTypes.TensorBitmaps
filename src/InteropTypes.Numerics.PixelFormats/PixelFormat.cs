using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

using InteropTypes.Numerics.Internal;

namespace InteropTypes.Numerics
{
    public interface IPixelFormatSource
    {
        PixelFormat Format { get; }
    }

    /// <summary>
    /// Represents a pixel format
    /// </summary>
    [type: System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]
    public readonly struct PixelFormat : IEquatable<PixelFormat>
    {
        public PixelFormat(ImmutableArray<PixelComponent> components)
            : this(components, components.Sum(item => item.ByteSize)) { }

        public PixelFormat(PixelComponent x)
            : this([x], x.ByteSize) { }

        public PixelFormat(PixelComponent x, PixelComponent y)
            : this([x, y], x.ByteSize + y.ByteSize) { }

        public PixelFormat(PixelComponent x, PixelComponent y, PixelComponent z)
            : this([x, y, z], x.ByteSize + y.ByteSize + z.ByteSize) { }

        public PixelFormat(PixelComponent x, PixelComponent y, PixelComponent z, PixelComponent w)
            : this([x, y, z, w], x.ByteSize + y.ByteSize + z.ByteSize + w.ByteSize) { }

        private PixelFormat(ImmutableArray<PixelComponent> components, int bytesPerPixel)
        {
            this.Components = components;
            this.BytesPerPixel = bytesPerPixel;
        }

        public ImmutableArray<PixelComponent> Components { get; }

        public override int GetHashCode() => PixelComponent.GetHashCodeFrom(Components);

        public bool Equals(PixelFormat other) => PixelComponent.AreStructurallyEqual(this.Components, other.Components);        

        public static bool operator ==(PixelFormat left, PixelFormat right) => left.Equals(right);
        public static bool operator !=(PixelFormat left, PixelFormat right) => !left.Equals(right);

        public int BytesPerPixel { get; }

        public bool HasAlphaComponent => Components.All(item => item.HasAlphaComponent);

        public int IndexOf(string semantic)
        {
            for (int i = 0; i < Components.Length; i++)
            {
                if (Components[i].Semantic.Equals(semantic, StringComparison.Ordinal)) return i;
            }

            return -1;
        }            

        public bool TryGetCommonType(out Type componentType)
        {
            componentType = Components[0].ComponentType;
            var ct = componentType;
            return Components.Skip(1).All(c => c.ComponentType == ct);
        }

        public override string ToString()
        {
            return string.Join(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator, Components.Select(item => item.ToString()));
        }

        public void ThrowIfBytesPerPixelMismatch<TPixel>() where TPixel: unmanaged
        {
            var s = Unsafe.SizeOf<TPixel>();
            if (s != BytesPerPixel) throw new InvalidOperationException($"{typeof(TPixel).Name} Bytes per pixel mismatch; expected {BytesPerPixel} but found {s}");
        }

        public void ThrowIfComponentTypeMismatch<TComponent>() where TComponent: unmanaged, INumber<TComponent>
        {
            if (!TryGetCommonType(out var ctype)) throw new InvalidOperationException("Hybrid formats not supported");
            if (ctype != typeof(TComponent)) throw new InvalidOperationException($"Component type mismatch, expected {ctype.Name} but found {typeof(TComponent).Name}");
        }

        public bool HasSameBytesPerPixelAs<TPixel>()
            where TPixel : unmanaged
        {
            var s = Unsafe.SizeOf<TPixel>();
            return s == BytesPerPixel;
        }

        public bool HasSameCommonComponentTypeAs<TComponent>()
            where TComponent : unmanaged, INumber<TComponent>
        {
            if (!TryGetCommonType(out var ctype)) return false;
            if (ctype != typeof(TComponent)) return false;
            return true;
        }

        public bool TryGetPixelValue<TPixel>(System.Drawing.Color color, out TPixel pixel)
            where TPixel: unmanaged
        {
            if (!HasSameBytesPerPixelAs<TPixel>())
            {
                pixel = default;
                return false;
            }            

            Span<int> src = stackalloc int[1];
            Span<TPixel> dst = stackalloc TPixel[1];
            src[0] = color.ToArgb();

            PixelConverters.Create<int, TPixel>(KnownPixelFormats.Bgra8, this, true).ConvertPixels(src, dst);

            pixel = dst[0];
            return true;
        }        
    }
}
