using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    public static partial class SkiaSharpForTensorBitmapsExtensions
    {
        public static bool TryGetSKEncodedImageFormat(System.IO.Stream stream, out SKEncodedImageFormat format)
        {
            format = SKEncodedImageFormat.Png;

            if (stream is not FileStream fs) return false;

            bool hasExt(string ext) => fs.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase);

            if (hasExt(".jpg") || hasExt(".jpeg")) { format = SKEncodedImageFormat.Jpeg; return true; }
            if (hasExt(".png")) { format = SKEncodedImageFormat.Png; return true; }
            if (hasExt(".gif")) { format = SKEncodedImageFormat.Gif; return true; }
            if (hasExt(".avif")) { format = SKEncodedImageFormat.Avif; return true; }
            if (hasExt(".webp")) { format = SKEncodedImageFormat.Webp; return true; }
            
            return false;
        }        
    }
}
