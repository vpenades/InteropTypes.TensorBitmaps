using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps;
using InteropTypes.TensorBitmaps.Operands;
using InteropTypes.TensorBitmaps.Operators;

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
            // https://github.com/onnx/models/tree/main/validated/vision/object_detection_segmentation/faster-rcnn/model

            modelPath ??= System.IO.Path.Combine(AppContext.BaseDirectory, "FasterRCNN-12.onnx");


            _Session = new InferenceSession(modelPath);

            // model's mean enconded into the format:
            var r = new PixelComponent<float>("Red", -102.9801f, -102.9801f + 255);
            var g = new PixelComponent<float>("Green", -115.9465f, -115.9465f + 255);
            var b = new PixelComponent<float>("Blue", -122.7717f, -122.7717f + 255);
            _InputFormat = new PixelFormat(b, g, r); // this model is BGR
        }

        public void Dispose()
        {
            _Session.Dispose();
        }

        private readonly InferenceSession _Session;

        private readonly PixelFormat _InputFormat;

        public float MinConfidence { get; set; } = 0.7f;        

        public IReadOnlyList<Prediction> Predict<TBitmap, TPixel>(TBitmap image)
            where TBitmap: IReadOnlyBitmapOperand<TBitmap, TPixel>, allows ref struct
            where TPixel : unmanaged
        {
            // Preprocess image

            var paddedHeight = (int)(Math.Ceiling(image.Height / 32f) * 32f);
            var paddedWidth = (int)(Math.Ceiling(image.Width / 32f) * 32f);            

            var input = new DenseTensor<float>([3, paddedHeight, paddedWidth]);

            // copy image from the input to the tensor

            var xform = CopyImageGeneric<TBitmap, TPixel>(image, input);

            // Setup inputs and outputs
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("image", input)
            };

            // run inference

            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _Session.Run(inputs);

            // Postprocess to get predictions

            var predictions = DecodePrediction(results, xform);

            return predictions;
        }

        

        private List<Prediction> DecodePrediction(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results, Matrix3x2 transform)
        {
            var resultsArray = results.ToArray();
            float[] boxes = resultsArray[0].AsEnumerable<float>().ToArray();
            long[] labels = resultsArray[1].AsEnumerable<long>().ToArray();
            float[] confidences = resultsArray[2].AsEnumerable<float>().ToArray();
            var predictions = new List<Prediction>();

            for (int i = 0; i < boxes.Length - 4; i += 4)
            {
                var index = i / 4;
                if (confidences[index] < MinConfidence) continue;

                var a = new Vector2(boxes[i + 0], boxes[i + 1]);
                var b = new Vector2(boxes[i + 2], boxes[i + 3]);
                a = Vector2.Transform(a, transform);
                b = Vector2.Transform(b, transform);
                b -= a;

                predictions.Add(new Prediction
                {
                    Box = new System.Drawing.RectangleF(a.X, a.Y, b.X, b.Y),
                    Label = LabelMap.Labels[labels[index]],
                    Confidence = confidences[index]
                });
            }

            return predictions;
        }        

        private Matrix3x2 CopyImageGeneric<TBitmap, TPixel>(TBitmap image, DenseTensor<float> input)
            where TBitmap : IReadOnlyBitmapOperand<TBitmap, TPixel>, allows ref struct
            where TPixel : unmanaged
        {
            // convert Onnx.Tensor to Numerics.TensorSpan
            var lengths = input.Dimensions.ToArray().Select(d => (nint)d).ToArray();
            var inputSpan = new System.Numerics.Tensors.TensorSpan<float>(input.Buffer.Span, lengths);

            // convert Numerics.TensorSpan to TensorSpanPlanes3
            var planes = TensorSpanPlanes3<float>.Create(inputSpan, _InputFormat);

            // resize, convert, fit, and copy pixels
            return planes
                .GetContext<TPixel>()
                .FillScaled(image, 0);
        }

    }

    [System.Diagnostics.DebuggerDisplay("{Label} {Confidence}")]
    public class Prediction
    {
        public System.Drawing.RectangleF Box { get; set; }
        public string? Label { get; set; }
        public float Confidence { get; set; }
    }

    class LabelMap
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
