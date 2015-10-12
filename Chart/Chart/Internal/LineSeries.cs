using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    [StyleTypedProperty(Property = "DataPointStyle", StyleTargetType = typeof(LineDataPoint))]
    public class LineSeries : XYSeries
    {
        internal override SeriesPresenter CreateSeriesPresenter()
        {
            return (SeriesPresenter)new LineSeriesPresenter((XYSeries)this);
        }

        internal override DataPoint CreateDataPoint()
        {
            return (DataPoint)new LineDataPoint();
        }
    }
}
