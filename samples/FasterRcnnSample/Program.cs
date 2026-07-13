using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps;

namespace FasterRcnnSample
{
    // https://github.com/microsoft/onnxruntime/tree/main/csharp/sample/Microsoft.ML.OnnxRuntime.FasterRcnnSample

    class Program
    {
        public static void Main(string[] args)
        {
            // Read image
            var image = new System.IO.FileInfo("shannon.jpg")
                .LoadTensorBitmapWithSkiaSharp<byte, int>(KnownPixelFormats.Rgba8)
                .AsReadOnlyTensorSpanBitmap();

            // create session

            using var sesion = new DetectionSession(null);

            // run session

            var predictions = sesion.Predict(image);

            // draw predictions

            
        }
    }

    
}
