using System;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class SeriesPresenterEventArgs : EventArgs
    {
        public Series Series { get; private set; }

        public DataPoint DataPoint { get; private set; }

        public SeriesPresenterEventArgs(Series series, DataPoint dataPoint)
        {
            this.Series = series;
            this.DataPoint = dataPoint;
        }
    }
}
