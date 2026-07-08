using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// Represents a pixel format
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToDebuggerDisplayString(),nq}")]
    public sealed record TensorPixelFormat
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

        public TensorPixelFormat(IReadOnlyList<TensorPixelComponent> components)
        {
            Components = components;

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public TensorPixelFormat(TensorPixelComponent x)
        {
            Components = [x];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public TensorPixelFormat(TensorPixelComponent x, TensorPixelComponent y)
        {
            Components = [x, y];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public TensorPixelFormat(TensorPixelComponent x, TensorPixelComponent y, TensorPixelComponent z)
        {
            Components = [x, y, z];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public TensorPixelFormat(TensorPixelComponent x, TensorPixelComponent y, TensorPixelComponent z, TensorPixelComponent w)
        {
            Components = [x, y, z, w];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public IReadOnlyList<TensorPixelComponent> Components { get; }

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
    }
}
