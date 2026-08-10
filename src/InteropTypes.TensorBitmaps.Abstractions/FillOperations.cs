using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.TensorBitmaps.Operators;

namespace InteropTypes.TensorBitmaps
{   

    public static partial class FillOperations
    {
        public static BitmapOperationFactory<int> Copy { get; } = new _FillCopy();        

        sealed class _FillCopy : BitmapOperationFactory<int>
        {
            public override IBitmapOperation<TSrcPixel, TDstPixel, int> GetInstance<TSrcPixel, TDstPixel>()
            {
                return _DirectCopyOperator<TSrcPixel, TDstPixel>.Instance;
            }
        }    
    }

    
}
