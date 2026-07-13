using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

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
    public sealed record PixelFormat
    {
        public PixelFormat(IReadOnlyList<PixelComponent> components)
        {
            Components = components;

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public PixelFormat(PixelComponent x)
        {
            Components = [x];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public PixelFormat(PixelComponent x, PixelComponent y)
        {
            Components = [x, y];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public PixelFormat(PixelComponent x, PixelComponent y, PixelComponent z)
        {
            Components = [x, y, z];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public PixelFormat(PixelComponent x, PixelComponent y, PixelComponent z, PixelComponent w)
        {
            Components = [x, y, z, w];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public IReadOnlyList<PixelComponent> Components { get; }

        public int BytesPerPixel { get; }

        public bool HasAlphaComponent => Components.All(item => item.HasAlphaComponent);

        public int IndexOf(string semantic)
        {
            for (int i = 0; i < Components.Count; i++)
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

        public void ThrowIfSizeMismatch<TPixel>() where TPixel: unmanaged
        {
            var s = Unsafe.SizeOf<TPixel>();
            if (s != BytesPerPixel) throw new InvalidOperationException($"{typeof(TPixel).Name} Bytes per pixel mismatch; expected {BytesPerPixel} but found {s}");
        }

        public void ThrowIfComponentTypeMismatch<TComponent>() where TComponent: unmanaged, INumber<TComponent>
        {
            if (!TryGetCommonType(out var ctype)) throw new InvalidOperationException("Hybrid formats not supported");
            if (ctype != typeof(TComponent)) throw new InvalidOperationException($"Component type mismatch, expected {ctype.Name} but found {typeof(TComponent).Name}");
        }

        public bool HasSameBytesPerPixelAs<TPixel>() where TPixel : unmanaged
        {
            var s = Unsafe.SizeOf<TPixel>();
            return s == BytesPerPixel;
        }

        public bool HasSameCommonComponentTypeAs<TComponent>() where TComponent : unmanaged, INumber<TComponent>
        {
            if (!TryGetCommonType(out var ctype)) return false;
            if (ctype != typeof(TComponent)) return false;
            return true;
        }
    }
}
