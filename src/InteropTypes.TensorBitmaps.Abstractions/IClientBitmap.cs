using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RECT = System.Drawing.Rectangle;
using SIZE = System.Drawing.Size;

namespace InteropTypes.TensorBitmaps
{
    public interface IClientReadOnlyBitmap<TPixel> : IReadOnlyBitmap<TPixel>, IDisposable
        where TPixel : unmanaged
    {
        /// <summary>
        /// Tries to get a cropped region of this bitmap
        /// </summary>        
        public bool TryGetCropped(RECT rect, out IClientReadOnlyBitmap<TPixel> croppedBitmap)
        {
            croppedBitmap = default;
            return false;
        }

        public bool TryCreateStretched(RECT? srcCrop, SIZE dstSize, out IClientReadOnlyBitmap<TPixel> stretchedBitmap)
        {
            stretchedBitmap = default;
            return false;
        }

        public static bool TryCreateStretched(IReadOnlyBitmap<TPixel> src, SIZE dstSize, out IClientReadOnlyBitmap<TPixel> stretchedBitmap)
        {
            switch (src)
            {
                case IClientReadOnlyBitmap<TPixel> client:
                    return client.TryCreateStretched(null, dstSize, out stretchedBitmap);

                case _ReadOnlyBitmapCropped<TPixel> rocropped:
                    return rocropped.TryCreateStretchedClient(dstSize, out stretchedBitmap);

                case _BitmapCropped<TPixel> rocropped:
                    if (rocropped.TryCreateStretchedClient(dstSize, out var stretched))
                    {
                        stretchedBitmap = stretched;
                        return true;
                    }
                    break;
            }

            stretchedBitmap = default;
            return false;
        }
    }

    public interface IClientBitmap<TPixel>
        : IClientReadOnlyBitmap<TPixel>
        , IBitmap<TPixel>
        where TPixel : unmanaged
    {
        
        bool IClientReadOnlyBitmap<TPixel>.TryGetCropped(RECT rect, out IClientReadOnlyBitmap<TPixel> croppedBitmap)
        {
            if (TryGetCropped(rect, out var cropped)) { croppedBitmap = cropped; return true; }
            croppedBitmap = default;
            return false;
        }

        bool IClientReadOnlyBitmap<TPixel>.TryCreateStretched(RECT? srcCrop, SIZE dstSize, out IClientReadOnlyBitmap<TPixel> stretchedBitmap)
        {
            if (TryCreateStretched(srcCrop, dstSize, out var stretched)) { stretchedBitmap = stretched; return true; }
            stretchedBitmap = default;
            return false;
        }

        /// <summary>
        /// Tries to get a cropped region of this bitmap
        /// </summary>
        public bool TryGetCropped(RECT rect, out IClientBitmap<TPixel> croppedBitmap)
        {
            croppedBitmap = default;
            return false;
        }

        public bool TryCreateStretched(RECT? srcCrop, SIZE dstSize, out IClientBitmap<TPixel> stretchedBitmap)
        {
            stretchedBitmap = default;
            return false;
        }
    }
}
