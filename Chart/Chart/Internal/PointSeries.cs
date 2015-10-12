using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    [StyleTypedProperty(Property = "DataPointStyle", StyleTargetType = typeof(PointDataPoint))]
    public class PointSeries : XYSeries
    {
        internal override SeriesPresenter CreateSeriesPresenter()
        {
            return (SeriesPresenter)new PointSeriesPresenter((XYSeries)this);
        }

        internal override DataPoint CreateDataPoint()
        {
            return (DataPoint)new PointDataPoint();
        }
    }
}
