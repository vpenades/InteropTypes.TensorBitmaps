using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace InteropTypes.TensorBitmaps
{
    static class LanczosResizer
    {
        private const int A = 3; // Lanczos-3 (Window size)        

        public static ManagedBitmap<Vector4> Resize(IReadOnlyBitmap<Vector4> src, int dstW, int dstH)
        {
            // Pass 1: Scale Horizontally

            var temp = new ManagedBitmap<Vector4>(dstW, src.Height, src.Format);

            float scaleX = (float)src.Width / dstW;

            for (int y = 0; y < src.Height; y++)
            {
                var srcRow = src.GetRowPixelsSpan(y);
                var tempRow = temp.GetRowPixelsSpan(y);

                for (int x = 0; x < dstW; x++)
                {
                    float srcX = (x + 0.5f) * scaleX - 0.5f;
                    int startX = (int)MathF.Ceiling(srcX - A);
                    int endX = (int)MathF.Floor(srcX + A);

                    Vector4 colorSum = Vector4.Zero;
                    float weightSum = 0f;

                    for (int sx = startX; sx <= endX; sx++)
                    {
                        int clampedX = Math.Clamp(sx, 0, src.Width - 1);
                        float weight = LanczosKernel(srcX - sx);

                        // SIMD Multiplies R, G, B, A all at once
                        colorSum += srcRow[clampedX] * weight;
                        weightSum += weight;
                    }

                    tempRow[x] = weightSum > 0 ? colorSum / weightSum : Vector4.Zero;
                }
            }

            // Pass 2: Scale Vertically
            var dst = new ManagedBitmap<Vector4>(dstW , dstH, src.Format);
            float scaleY = (float)src.Height / dstH;

            for (int y = 0; y < dstH; y++)
            {
                var dstRow = dst.GetRowPixelsSpan(y);

                float srcY = (y + 0.5f) * scaleY - 0.5f;
                int startY = (int)MathF.Ceiling(srcY - A);
                int endY = (int)MathF.Floor(srcY + A);

                // Pre-calculate weights for this row's vertical window pass
                int windowSize = endY - startY + 1;
                float[] weights = new float[windowSize];
                float weightSum = 0f;

                for (int i = 0; i < windowSize; i++)
                {
                    weights[i] = LanczosKernel(srcY - (startY + i));
                    weightSum += weights[i];
                }

                for (int x = 0; x < dstW; x++)
                {
                    var colorSum = Vector4.Zero;

                    for (int i = 0; i < windowSize; i++)
                    {
                        int clampedY = Math.Clamp(startY + i, 0, src.Height - 1);
                        var tempRow = temp.GetRowPixelsSpan(clampedY);
                        colorSum += tempRow[x] * weights[i];
                    }

                    var p = weightSum > 0 ? colorSum / weightSum : Vector4.Zero;

                    System.Diagnostics.Debug.Assert(!float.IsNaN(p.X));
                    System.Diagnostics.Debug.Assert(!float.IsNaN(p.Y));
                    System.Diagnostics.Debug.Assert(!float.IsNaN(p.Z));
                    System.Diagnostics.Debug.Assert(!float.IsNaN(p.W));

                    dstRow[x] = Vector4.Clamp(p, Vector4.Zero, Vector4.One);
                }
            }

            return dst;
        }

        // Core Sinc-based Lanczos kernel evaluation
        private static float LanczosKernel(float x)
        {
            if (x == 0) return 1.0f;
            if (x <= -A || x >= A) return 0.0f;

            float piX = x * MathF.PI;
            return (MathF.Sin(piX) * MathF.Sin(piX / A)) / (piX * (piX / A));
        }
    }
}
