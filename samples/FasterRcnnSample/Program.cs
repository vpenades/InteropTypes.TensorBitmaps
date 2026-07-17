using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps;
using InteropTypes.TensorBitmaps.Diagnostics;
using InteropTypes.TensorBitmaps.Operands;
using InteropTypes.TensorBitmaps.Operators;

namespace FasterRcnnSample
{
    // https://github.com/microsoft/onnxruntime/tree/main/csharp/sample/Microsoft.ML.OnnxRuntime.FasterRcnnSample

    class Program
    {
        public static void Main(string[] args)
        {
            // Read image
            var image = SkiaSharpBitmapOperand<uint>.Load(new System.IO.FileInfo("frcnn_demo.jpg"));

            // create session
            using var sesion = new DetectionSession(null);

            // run session
            var predictions = sesion.Predict<SkiaSharpBitmapOperand<uint>, uint>(image);            

            // draw predictions
            var dc = new DiagnosticsDrawing<SkiaSharpBitmapOperand<uint>, uint>(image);

            foreach(var prediction in predictions)
            {
                dc.DrawRectangle(prediction.Box, System.Drawing.Color.Red);
            }

            image.Save(new System.IO.FileInfo("frcnn_demo.result.jpg"));
        }
    }

    
}
