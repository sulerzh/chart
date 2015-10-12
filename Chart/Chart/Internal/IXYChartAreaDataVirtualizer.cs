using System.Collections.Generic;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public interface IXYChartAreaDataVirtualizer
    {
        void InitializeSeries(XYSeries series);

        void UninitializeSeries(XYSeries series);

        void InitializeAxisScale(Axis axis, Scale scale);

        void UninitializeAxisScale(Axis axis, Scale scale);

        void UpdateSeriesForCurrentView(IEnumerable<XYSeries> series);
    }
}
