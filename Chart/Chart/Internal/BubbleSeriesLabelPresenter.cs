using System.Collections.Generic;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class BubbleSeriesLabelPresenter : PointSeriesLabelPresenter
    {
        public BubbleSeriesLabelPresenter(SeriesPresenter seriesPresenter)
          : base(seriesPresenter)
        {
        }

        internal override void AdjustDataPointLabelVisibilityRating(LabelVisibilityManager.DataPointRange range, Dictionary<XYDataPoint, double> dataPointRanks)
        {
            BubbleDataPoint bubbleDataPoint1 = (BubbleDataPoint)null;
            double num = double.MinValue;
            foreach (XYDataPoint xyDataPoint in range.DataPoints)
            {
                BubbleDataPoint bubbleDataPoint2 = xyDataPoint as BubbleDataPoint;
                if (bubbleDataPoint2 != null && bubbleDataPoint2.SizeValueInScaleUnitsWithoutAnimation > num)
                {
                    num = bubbleDataPoint2.SizeValueInScaleUnitsWithoutAnimation;
                    bubbleDataPoint1 = bubbleDataPoint2;
                }
            }
            if (bubbleDataPoint1 == null)
                return;
            if (dataPointRanks.ContainsKey((XYDataPoint)bubbleDataPoint1))
            {
                Dictionary<XYDataPoint, double> dictionary;
                XYDataPoint index;
                (dictionary = dataPointRanks)[index = (XYDataPoint)bubbleDataPoint1] = dictionary[index] + 150.0;
            }
            else
                dataPointRanks.Add((XYDataPoint)bubbleDataPoint1, 150.0);
        }
    }
}
