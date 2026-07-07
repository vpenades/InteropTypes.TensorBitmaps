using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// A bitmap interface to a bitmap backed by a <see cref="IReadOnlyTensor"/>
    /// </summary>    
    public interface IReadOnlyTensorBitmap
    {
        IReadOnlyTensor Tensor { get; }
        TensorPixelFormat Format { get; }
        int BytesPerPixel { get; }
        int Width { get; }
        int Height { get; }

        ReadOnlySpan<byte> GetRowSpan(int y);
    }

    /// <summary>
    /// A bitmap interface to a bitmap backed by a <see cref="ITensor"/>
    /// </summary>    
    public interface ITensorBitmap : IReadOnlyTensorBitmap
    {
        new ITensor Tensor { get; }
        new Span<byte> GetRowSpan(int y);
    }

    
}
