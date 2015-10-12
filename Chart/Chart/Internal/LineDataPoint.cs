using System;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public class LineDataPoint : PointDataPoint
    {
        public LineDataPoint()
        {
        }

        public LineDataPoint(IComparable xValue, IComparable yValue)
          : base(xValue, yValue)
        {
        }
    }
}
