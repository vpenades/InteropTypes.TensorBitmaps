using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;

namespace InteropTypes.TensorBitmaps.Operators
{
    /// <summary>
    /// When we want to fit a bitmap into another while preserving the aspect ratio,
    /// this helper class calculates the crop rectangles for both src and dst so
    /// we only need to "stretch" one cropped bitmap over the other cropped bitmap.
    /// </summary>
    internal readonly struct ScaledIntersectionCrop
    {
        public static ScaledIntersectionCrop CreateFrom(System.Drawing.Size src, System.Drawing.Size dst, float overflowAmount)
        {
            GetCrossCrops(src, dst, overflowAmount, out var srcRect, out var dstRect);

            return new ScaledIntersectionCrop { SourceCrop = srcRect, TargetCrop = dstRect };
        }

        /// <summary>
        /// Calculate the cropping of src and dst taking in account the overflow amount to preserve the aspect ratio.
        /// </summary>
        /// <param name="src">The size of src bitmap</param>
        /// <param name="dst">The size of dst bitmap</param>
        /// <param name="overflowAmount">how much src can overflow dst</param>
        /// <param name="srcr">the crop rectangle to apply to src</param>
        /// <param name="dstr">the crop rectangle to apply to dst</param>
        private static void GetCrossCrops(System.Drawing.Size src, System.Drawing.Size dst, float overflowAmount, out System.Drawing.Rectangle srcr, out System.Drawing.Rectangle dstr)
        {
            // calculate aspect ratios:
            var srck = (float)src.Width / (float)src.Height;
            var dstk = (float)dst.Width / (float)dst.Height;

            // lerp aspect ratios using allowed overflow amount
            var k = srck * (1 - overflowAmount) + dstk * overflowAmount;

            // shrink both src and dst to ensure they have k aspect ratio:            
            srcr = _GetCenterCrop(src.Width, src.Height, k);
            dstr = _GetCenterCrop(dst.Width, dst.Height, k);

            #if DEBUG
            srck = (float)srcr.Width / (float)srcr.Height;
            dstk = (float)dstr.Width / (float)dstr.Height;
            var aspectRatio = Math.Abs(1f - srck / dstk);
            System.Diagnostics.Debug.Assert(aspectRatio < 0.1f, $"At this point the aspect ratio of both crops must be close enough, but found {aspectRatio}");
            #endif
        }

        private static System.Drawing.Rectangle _GetCenterCrop(int width, int height, float aspect)
        {
            var k = (float)width / (float)height;

            if (k > aspect) // clip horizontally
            {
                var ww = height * aspect;
                var r = new System.Drawing.RectangleF((width - ww) / 2, 0, ww, height);
                return System.Drawing.Rectangle.Truncate(r);
            }

            if (k < aspect) // clip vertically
            {
                var hh = width / aspect;
                var r = new System.Drawing.RectangleF(0, (height - hh) / 2, width, hh);
                return System.Drawing.Rectangle.Truncate(r);
            }

            return new System.Drawing.Rectangle(0, 0, width, height);
        }

        public System.Drawing.Rectangle SourceCrop { get; init; }
        public System.Drawing.Rectangle TargetCrop { get; init; }

        /// <summary>
        /// After stretching the bitmaps, the returned stretch transform can be used to calculate the final transform
        /// </summary>
        /// <param name="stretchTransform">the resulting transform from the stretch operation</param>
        /// <returns>the final transform</returns>
        public System.Numerics.Matrix3x2 GetTransform(System.Numerics.Matrix3x2 stretchTransform)
        {
            var transform = Matrix3x2.CreateTranslation(-TargetCrop.X, -TargetCrop.Y) * stretchTransform;

            transform *= Matrix3x2.CreateTranslation(SourceCrop.X, SourceCrop.Y);

            return transform;
        }


    }
}
