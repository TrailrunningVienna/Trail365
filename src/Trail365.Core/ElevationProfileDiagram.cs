using System;
using System.Drawing;
using Trail365.Internal;

namespace Trail365
{
    [Serializable]
    public class ElevationProfileDiagram
    {
        public static readonly ElevationProfileDiagram BasicChallenge = new ElevationProfileDiagram { DistanceBase = 10000, AltitudeDelta = 2500 };
        public static readonly ElevationProfileDiagram IntermediateChallenge = new ElevationProfileDiagram { DistanceBase = 20000, AltitudeDelta = 500 };
        public static readonly ElevationProfileDiagram AdvancedChallenge = new ElevationProfileDiagram { DistanceBase = 30000, AltitudeDelta = 1000 };
        public static readonly ElevationProfileDiagram ProficiencyChallenge = new ElevationProfileDiagram { DistanceBase = 60000, AltitudeDelta = 2000 };

        public static readonly ElevationProfileDiagram Default = new ElevationProfileDiagram();

        /// <summary>
        /// if not null then THIS is the length (x-axis) that is used for the diagram
        /// 0 is used as StartValue
        /// </summary>
        public float? DistanceBase { get; set; }

        public float? AltitudeMinValue { get; set; }

        public float? AltitudeMaxValue { get; set; }

        public float? AltitudeDelta { get; set; }

        public Tuple<float, float> GetFactors(Size canvasSize, float? distance, float? height)
        {
            if (canvasSize.IsEmpty) throw new ArgumentNullException(nameof(canvasSize));
            //int destWidthBmp = canvasSize.Width;
            //int destHeightBmp = canvasSize.Height;
            float destWidthBmpAsFloat = Convert.ToSingle(canvasSize.Width);
            float destHeightBmpAsFloat = Convert.ToSingle(canvasSize.Height);

            float? d = null;
            if (distance.HasValue)
            {
                d = distance.Value;
            }
            if (this.DistanceBase.HasValue)
            {
                d = this.DistanceBase.Value;
            }
            Guard.Assert(d.HasValue);
            float enforcedDistance = d.Value;

            float? h = null;

            if (height.HasValue)
            {
                h = height.Value;
            }

            if (this.AltitudeDelta.HasValue)
            {
                h = this.AltitudeDelta.Value;
            }

            if (this.AltitudeMinValue.HasValue && this.AltitudeMaxValue.HasValue)
            {
                Guard.Assert(this.AltitudeDelta.HasValue == false);
                h = this.AltitudeMaxValue.Value - this.AltitudeMinValue.Value;
                Guard.Assert(h.Value >= 0);
            }
            Guard.Assert(h.HasValue);

            float enforcedHeight = h.Value;
            float xFactor = destWidthBmpAsFloat / enforcedDistance;
            float yFactor = destHeightBmpAsFloat / enforcedHeight;

            return new Tuple<float, float>(xFactor, yFactor);
        }
    }
}
