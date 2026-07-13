using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.Numerics.BitmapOperators;
using InteropTypes.TensorBitmaps;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace FasterRcnnSample
{
    public class DetectionSession : IDisposable
    {
        public DetectionSession(string? modelPath)
        {
            // https://onnxruntime.ai/models
            // https://huggingface.co/onnxmodelzoo/FasterRCNN-12

            modelPath ??= System.IO.Path.Combine(AppContext.BaseDirectory, "FasterRCNN-12.onnx");


            _Session = new InferenceSession(modelPath);

            // model's mean enconded into the format:
            var r = new PixelComponent<float>("Red", 102.9801f, 102.9801f + 255);
            var g = new PixelComponent<float>("Green", 115.9465f, 115.9465f + 255);
            var b = new PixelComponent<float>("Blue", 122.7717f, 122.7717f + 255);
            _InputFormat = new PixelFormat(b, g, r); // this model is BGR
        }

        public void Dispose()
        {
            _Session.Dispose();
        }

        private readonly InferenceSession _Session;

        private readonly PixelFormat _InputFormat;

        public float MinConfidence { get; set; } = 0.7f;

        public IReadOnlyList<Prediction> Predict<TPixel, TBitmap>(TBitmap image)
            where TBitmap: IReadOnlyBitmapOperand<TBitmap,TPixel>
            where TPixel : unmanaged
        {
            // Preprocess image

            var paddedHeight = (int)(Math.Ceiling(image.Height / 32f) * 32f);
            var paddedWidth = (int)(Math.Ceiling(image.Width / 32f) * 32f);
            DenseTensor<float> input = new DenseTensor<float>([3, paddedHeight, paddedWidth]);

            CopyImageGeneric<TPixel,TBitmap>(image, input);

            // Setup inputs and outputs
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("image", input)
            };

            // run inference

            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _Session.Run(inputs);

            // Postprocess to get predictions
            List<Prediction> predictions = DecodePrediction(results);

            return predictions;
        }

        public IReadOnlyList<Prediction> Predict<TElement, TPixel>(ReadOnlyTensorSpanBitmap<TElement, TPixel> image)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            // Preprocess image

            var paddedHeight = (int)(Math.Ceiling(image.Height / 32f) * 32f);
            var paddedWidth = (int)(Math.Ceiling(image.Width / 32f) * 32f);
            DenseTensor<float> input = new DenseTensor<float>([3, paddedHeight, paddedWidth]);

            CopyImage(image, input);

            // Setup inputs and outputs
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("image", input)
            };

            // run inference

            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _Session.Run(inputs);

            // Postprocess to get predictions
            List<Prediction> predictions = DecodePrediction(results);

            return predictions;
        }

        private List<Prediction> DecodePrediction(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
        {
            var resultsArray = results.ToArray();
            float[] boxes = resultsArray[0].AsEnumerable<float>().ToArray();
            long[] labels = resultsArray[1].AsEnumerable<long>().ToArray();
            float[] confidences = resultsArray[2].AsEnumerable<float>().ToArray();
            var predictions = new List<Prediction>();

            for (int i = 0; i < boxes.Length - 4; i += 4)
            {
                var index = i / 4;
                if (confidences[index] >= MinConfidence)
                {
                    predictions.Add(new Prediction
                    {
                        Box = new System.Drawing.RectangleF(boxes[i], boxes[i + 1], boxes[i + 2] - boxes[i], boxes[i + 3] - boxes[i + 1]),
                        Label = LabelMap.Labels[labels[index]],
                        Confidence = confidences[index]
                    });
                }
            }

            return predictions;
        }

        private Matrix3x2 CopyImage<TElement, TPixel>(ReadOnlyTensorSpanBitmap<TElement, TPixel> image, DenseTensor<float> input)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            // convert Onnx.Tensor to Numerics.TensorSpan

            nint[] lengths = input.Dimensions.ToArray().Select(d => (nint)d).ToArray();
            var inputSpan = new System.Numerics.Tensors.TensorSpan<float>(input.Buffer.Span, lengths);

            // convert Numerics.TensorSpan to TensorSpanPlanes3
            var planes = TensorSpanPlanes3<float>.Create(inputSpan, _InputFormat);

            // resize, convert, fit, and copy pixels
            return planes.CopyPixelsFrom(PixelsTransform.ScaleToFit(0), image);
        }

        private Matrix3x2 CopyImageGeneric<TPixel, TBitmap>(TBitmap image, DenseTensor<float> input)
            where TBitmap : IReadOnlyBitmapOperand<TBitmap, TPixel>
            where TPixel : unmanaged
        {
            // convert Onnx.Tensor to Numerics.TensorSpan

            nint[] lengths = input.Dimensions.ToArray().Select(d => (nint)d).ToArray();
            var inputSpan = new System.Numerics.Tensors.TensorSpan<float>(input.Buffer.Span, lengths);

            // convert Numerics.TensorSpan to TensorSpanPlanes3
            var planes = TensorSpanPlanes3<float>.Create(inputSpan, _InputFormat);

            // resize, convert, fit, and copy pixels
            // return planes.CopyPixelsFrom(PixelsTransform.ScaleToFit(0), image);

            
            var resizer = IBinaryOperation<TPixel, float, Matrix3x2>.GetScaleToFit(0);
            resizer.Execute(image, planes.PlaneX, IPixelConverter<TPixel, float>.Create(image.Format, planes.PlaneX.Format,true));
            resizer.Execute(image, planes.PlaneY, IPixelConverter<TPixel, float>.Create(image.Format, planes.PlaneY.Format, true));
            return resizer.Execute(image, planes.PlaneZ, IPixelConverter<TPixel, float>.Create(image.Format, planes.PlaneZ.Format, true));
        }

    }

    public class Prediction
    {
        public System.Drawing.RectangleF Box { get; set; }
        public string? Label { get; set; }
        public float Confidence { get; set; }
    }

    public class LabelMap
    {
        public static readonly string[] Labels = new[] {"__background",
                                                        "person",
                                                        "bicycle",
                                                        "car",
                                                        "motorcycle",
                                                        "airplane",
                                                        "bus",
                                                        "train",
                                                        "truck",
                                                        "boat",
                                                        "traffic light",
                                                        "fire hydrant",
                                                        "stop sign",
                                                        "parking meter",
                                                        "bench",
                                                        "bird",
                                                        "cat",
                                                        "dog",
                                                        "horse",
                                                        "sheep",
                                                        "cow",
                                                        "elephant",
                                                        "bear",
                                                        "zebra",
                                                        "giraffe",
                                                        "backpack",
                                                        "umbrella",
                                                        "handbag",
                                                        "tie",
                                                        "suitcase",
                                                        "frisbee",
                                                        "skis",
                                                        "snowboard",
                                                        "sports ball",
                                                        "kite",
                                                        "baseball bat",
                                                        "baseball glove",
                                                        "skateboard",
                                                        "surfboard",
                                                        "tennis racket",
                                                        "bottle",
                                                        "wine glass",
                                                        "cup",
                                                        "fork",
                                                        "knife",
                                                        "spoon",
                                                        "bowl",
                                                        "banana",
                                                        "apple",
                                                        "sandwich",
                                                        "orange",
                                                        "broccoli",
                                                        "carrot",
                                                        "hot dog",
                                                        "pizza",
                                                        "donut",
                                                        "cake",
                                                        "chair",
                                                        "couch",
                                                        "potted plant",
                                                        "bed",
                                                        "dining table",
                                                        "toilet",
                                                        "tv",
                                                        "laptop",
                                                        "mouse",
                                                        "remote",
                                                        "keyboard",
                                                        "cell phone",
                                                        "microwave",
                                                        "oven",
                                                        "toaster",
                                                        "sink",
                                                        "refrigerator",
                                                        "book",
                                                        "clock",
                                                        "vase",
                                                        "scissors",
                                                        "teddy bear",
                                                        "hair drier",
                                                        "toothbrush"};
    }
}
