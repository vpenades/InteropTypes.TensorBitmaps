using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Pixel888
    {
        public byte X;
        public byte Y;
        public byte Z;

        public Pixel888(byte x, byte y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }        
    }
}
