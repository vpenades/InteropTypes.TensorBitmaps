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
    public record TensorPixelFormat
    {
        public static TensorPixelFormat Rgb24 = new TensorPixelFormat(TensorPixelComponent.RedByte, TensorPixelComponent.GreenByte, TensorPixelComponent.BlueByte);
        public static TensorPixelFormat Rgbx32 = new TensorPixelFormat(TensorPixelComponent.RedByte, TensorPixelComponent.GreenByte, TensorPixelComponent.BlueByte, TensorPixelComponent.OpaqueAlphaByte);
            
        public static TensorPixelFormat Rgba32 = new TensorPixelFormat(TensorPixelComponent.RedByte, TensorPixelComponent.GreenByte, TensorPixelComponent.BlueByte, TensorPixelComponent.AlphaByte);
        public static TensorPixelFormat Rgbp32 = new TensorPixelFormat(TensorPixelComponent.RedByte, TensorPixelComponent.GreenByte, TensorPixelComponent.BlueByte, TensorPixelComponent.PremulByte);

        public static TensorPixelFormat Bgr24 = new TensorPixelFormat(TensorPixelComponent.BlueByte, TensorPixelComponent.GreenByte, TensorPixelComponent.RedByte);
        public static TensorPixelFormat Bgrx32 = new TensorPixelFormat(TensorPixelComponent.BlueByte, TensorPixelComponent.GreenByte, TensorPixelComponent.RedByte, TensorPixelComponent.OpaqueAlphaByte);
        public static TensorPixelFormat Bgra32 = new TensorPixelFormat(TensorPixelComponent.BlueByte, TensorPixelComponent.GreenByte, TensorPixelComponent.RedByte, TensorPixelComponent.AlphaByte);
        public static TensorPixelFormat Bgrp32 = new TensorPixelFormat(TensorPixelComponent.BlueByte, TensorPixelComponent.GreenByte, TensorPixelComponent.RedByte, TensorPixelComponent.PremulByte);

        public static TensorPixelFormat Argb32 = new TensorPixelFormat(TensorPixelComponent.AlphaByte, TensorPixelComponent.RedByte, TensorPixelComponent.GreenByte, TensorPixelComponent.BlueByte);
        public static TensorPixelFormat Abgr32 = new TensorPixelFormat(TensorPixelComponent.AlphaByte, TensorPixelComponent.BlueByte, TensorPixelComponent.GreenByte, TensorPixelComponent.RedByte);

        public static TensorPixelFormat Rgb96f = new TensorPixelFormat(TensorPixelComponent.RedSingle, TensorPixelComponent.GreenSingle, TensorPixelComponent.BlueSingle);
        public static TensorPixelFormat Bgr96f = new TensorPixelFormat(TensorPixelComponent.BlueSingle, TensorPixelComponent.GreenSingle, TensorPixelComponent.RedSingle);
        public static TensorPixelFormat Rgba128f = new TensorPixelFormat(TensorPixelComponent.RedSingle, TensorPixelComponent.GreenSingle, TensorPixelComponent.BlueSingle, TensorPixelComponent.AlphaSingle);
        public static TensorPixelFormat Rgbp128f = new TensorPixelFormat(TensorPixelComponent.RedSingle, TensorPixelComponent.GreenSingle, TensorPixelComponent.BlueSingle, TensorPixelComponent.PremulSingle);

        public static TensorPixelFormat Rg32 = new TensorPixelFormat(TensorPixelComponent.RedShort, TensorPixelComponent.GreenShort);

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
