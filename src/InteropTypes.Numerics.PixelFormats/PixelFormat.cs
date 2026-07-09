using System;
using System.Collections.Generic;
using System.Linq;
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
    [System.Diagnostics.DebuggerDisplay("{ToDebuggerDisplayString(),nq}")]
    public sealed record PixelFormat
    {
        private string ToDebuggerDisplayString()
        {
            var sb = new StringBuilder();

            foreach(var c in Components)
            {
                sb.Append($"{c.Semantic}:{c.ComponentType.Name} ");
            }

            return sb.ToString();
        }

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

        public override string ToString()
        {
            return string.Join(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator, Components);
        }

        public bool TryGetCommonType(out Type componentType)
        {
            componentType = Components[0].ComponentType;
            var ct = componentType;
            return Components.Skip(1).All(c => c.ComponentType == ct);
        }
    }
}
