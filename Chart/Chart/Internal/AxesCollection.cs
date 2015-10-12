using Semantic.Reporting.Windows.Chart.Internal.Properties;
using Semantic.Reporting.Windows.Common.Internal;
using System;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class AxesCollection : UniqueObservableCollection<Axis>
    {
        private ChartArea _seriesHost;

        internal AxesCollection(ChartArea seriesHost)
        {
            this._seriesHost = seriesHost;
        }

        protected override void RemoveItem(int index)
        {
            if (!this._seriesHost.CanRemoveAxis(this[index]))
                throw new InvalidOperationException(Resources.ChartAreaAxesCollection_RemoveItem_AxisCannotBeRemovedWhenOneOrMoreSeriesAreListeningToIt);
            base.RemoveItem(index);
        }
    }
}
