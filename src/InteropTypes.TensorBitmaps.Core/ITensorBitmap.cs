using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// A bitmap interface to a bitmap backed by a <see cref="IReadOnlyTensor"/>
    /// </summary>    
    public interface IReadOnlyBitmap
    {
        PixelFormat Format { get; }
        int BytesPerPixel { get; }
        int Width { get; }
        int Height { get; }

        ReadOnlySpan<byte> GetRowSpan(int y);
    }

    
    public interface IBitmap : IReadOnlyBitmap
    {
        new Span<byte> GetRowSpan(int y);
    }


    /// <summary>
    /// A bitmap interface to a bitmap backed by a <see cref="IReadOnlyTensor"/>
    /// </summary>    
    public interface IReadOnlyTensorBitmap : IReadOnlyBitmap
    {
        IReadOnlyTensor Tensor { get; }

        IReadOnlyTensorBitmap GetCropped(System.Drawing.Rectangle rectangle);

        public void CopyPixelsTo<TOtherElement, TOtherPixel>(TensorSpanBitmap<TOtherElement, TOtherPixel> dstBitmap, bool initPixels = true)
            where TOtherElement : unmanaged, INumber<TOtherElement>
            where TOtherPixel : unmanaged;
    }


    /// <summary>
    /// A bitmap interface to a bitmap backed by a <see cref="ITensor"/>
    /// </summary>    
    public interface ITensorBitmap : IReadOnlyTensorBitmap
    {
        new ITensor Tensor { get; }

        new ITensorBitmap GetCropped(System.Drawing.Rectangle rectangle);
    }


}
