using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps.Diagnostics
{
    internal class ComparisonStatistics
    {
        #region lifecycle

        public static ComparisonStatistics Compare<TLeftBitmap, TLeftPixel, TRightBitmap, TRightPixel>(TLeftBitmap left, TRightBitmap right)
            where TLeftBitmap : Operands.IReadOnlyBitmapOperand<TLeftBitmap, TLeftPixel>
            where TLeftPixel : unmanaged
            where TRightBitmap : Operands.IReadOnlyBitmapOperand<TRightBitmap, TRightPixel>
            where TRightPixel : unmanaged
        {
            var stats = new ComparisonStatistics();

            if (left.Width != right.Width || left.Height != right.Height)
            {
                stats.SizeMismatch = true;
                return stats;
            }

            var leftConverter = IPixelConverter<TLeftPixel, Vector4>.Create(left.Format, KnownPixelFormats.RgbaF32, true);
            var rightConverter = IPixelConverter<TRightPixel, Vector4>.Create(right.Format, KnownPixelFormats.RgbaF32, true);

            Span<Vector4> leftRow = stackalloc Vector4[right.Width];
            Span<Vector4> rightRow = stackalloc Vector4[right.Width];

            for (int y = 0; y < left.Height; y++)
            {
                leftConverter.ConvertPixels(left.GetRowPixelsSpan(y), leftRow);
                rightConverter.ConvertPixels(right.GetRowPixelsSpan(y), rightRow);

                for (int x = 0; x < left.Width; x++)
                {
                    var l = leftRow[x];
                    var r = rightRow[x];
                    stats.Aggregate(l, r);
                }
            }

            stats.Finish();

            return stats;
        }

        public ComparisonStatistics()
        {
            compMin.AsSpan().Fill(double.PositiveInfinity);
            compMax.AsSpan().Fill(double.NegativeInfinity);
        }

        #endregion

        #region data

        private readonly double[] compSumAbs = new double[4];
        private readonly double[] compSumAbsSq = new double[4];
        private readonly double[] compMin = new double[4];
        private readonly double[] compMax = new double[4];
        private double totalAbs;
        private double totalSq;

        #endregion

        #region API

        protected void Aggregate(Vector4 L, Vector4 R)
        {
            PixelCount++;

            float dR = L.X - R.X;
            float dG = L.Y - R.Y;
            float dB = L.Z - R.Z;
            float dA = L.W - R.W;

            double absR = Math.Abs(dR);
            double absG = Math.Abs(dG);
            double absB = Math.Abs(dB);
            double absA = Math.Abs(dA);

            double sqR = dR * (double)dR;
            double sqG = dG * (double)dG;
            double sqB = dB * (double)dB;
            double sqA = dA * (double)dA;

            // accumulate per-component
            compSumAbs[0] += absR;
            compSumAbs[1] += absG;
            compSumAbs[2] += absB;
            compSumAbs[3] += absA;

            compSumAbsSq[0] += absR * absR;
            compSumAbsSq[1] += absG * absG;
            compSumAbsSq[2] += absB * absB;
            compSumAbsSq[3] += absA * absA;

            compMin[0] = Math.Min(compMin[0], absR);
            compMin[1] = Math.Min(compMin[1], absG);
            compMin[2] = Math.Min(compMin[2], absB);
            compMin[3] = Math.Min(compMin[3], absA);

            compMax[0] = Math.Max(compMax[0], absR);
            compMax[1] = Math.Max(compMax[1], absG);
            compMax[2] = Math.Max(compMax[2], absB);
            compMax[3] = Math.Max(compMax[3], absA);

            // global accumulators (squared diffs and absolute diffs)
            totalAbs += absR + absG + absB + absA;
            totalSq += sqR + sqG + sqB + sqA;
        }

        private void Finish()
        {
            var n = PixelCount;

            // global metrics
            double globalMeanAbs = totalAbs / (n * 4.0);
            double globalRms = Math.Sqrt(totalSq / (n * 4.0)); // in 0..1
            
            GlobalMeanAbsolute = globalMeanAbs;
            GlobalRms = globalRms;            


            // finalize per-component stats
            var compStats = new ComponentStat[4];
            for (int c = 0; c < 4; c++)
            {
                double meanAbs = compSumAbs[c] / n;
                // stddev of the absolute diffs (population)
                double meanSqAbs = compSumAbsSq[c] / n;
                double variance = meanSqAbs - (meanAbs * meanAbs);
                if (variance < 0 && variance > -1e-12) variance = 0;
                double stddev = Math.Sqrt(Math.Max(0.0, variance));

                // RMS for this component across pixels: sqrt(mean(sq(diff)))
                // note: sq(diff) is same whether signed or not
                double rms = Math.Sqrt(meanSqAbs);

                compStats[c] = new ComponentStat
                {
                    Name = c.ToString(),
                    MeanAbsolute = meanAbs,
                    StdDev = stddev,
                    Rms = rms,
                    Min = compMin[c] == double.PositiveInfinity ? 0.0 : compMin[c],
                    Max = compMax[c] == double.NegativeInfinity ? 0.0 : compMax[c]
                };
            }

            ComponentStats = compStats;
            // PixelStats = pixelStats;
        }

        #endregion

        #region result

        public bool SizeMismatch { get; private set; }

        public int PixelCount { get; private set; }

        /// <summary>
        /// RMS across all components and pixels (0 = same, 1 = totally different)
        /// </summary>
        public double GlobalRms { get; private set; }

        /// <summary>
        /// Mean absolute difference across all components and pixels
        /// </summary>
        public double GlobalMeanAbsolute { get; private set; }

        /// <summary>
        /// Per-component stats: order = 0:R, 1:G, 2:B, 3:A
        /// </summary>
        public ComponentStat[] ComponentStats { get; private set; } = new ComponentStat[4];

        public class ComponentStat
        {
            public string Name { get; set; } = "";
            public double MeanAbsolute { get; set; }
            public double StdDev { get; set; }

            /// <summary>
            /// sqrt(mean(square(diff))) for this component across pixels
            /// </summary>
            public double Rms { get; set; }   
            public double Min { get; set; }
            public double Max { get; set; }
        }

        #endregion
    }
}
