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
    public interface IReadOnlyTensorBitmap : IReadOnlyBitmap
    {
        IReadOnlyTensor Tensor { get; }
        IReadOnlyTensorBitmap GetCropped(System.Drawing.Rectangle rectangle);        
    }


    /// <summary>
    /// A bitmap interface to a bitmap backed by a <see cref="ITensor"/>
    /// </summary>    
    public interface ITensorBitmap : IBitmap, IReadOnlyTensorBitmap
    {
        new ITensor Tensor { get; }
        new ITensorBitmap GetCropped(System.Drawing.Rectangle rectangle);
    }


}
