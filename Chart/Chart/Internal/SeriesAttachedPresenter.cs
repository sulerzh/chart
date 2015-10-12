using System;
using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal abstract class SeriesAttachedPresenter : DependencyObject
    {
        protected SeriesPresenter SeriesPresenter { get; private set; }

        public SeriesAttachedPresenter(SeriesPresenter seriesPresenter)
        {
            this.SeriesPresenter = seriesPresenter;
            this.SeriesPresenter.ViewCreated += (EventHandler<SeriesPresenterEventArgs>)((sender, args) => this.OnCreateView(args.DataPoint));
            this.SeriesPresenter.ViewRemoved += (EventHandler<SeriesPresenterEventArgs>)((sender, args) => this.OnRemoveView(args.DataPoint));
            this.SeriesPresenter.ViewUpdated += (EventHandler<SeriesPresenterEventArgs>)((sender, args) => this.OnUpdateView(args.DataPoint));
            this.SeriesPresenter.Removed += (EventHandler<SeriesPresenterEventArgs>)((sender, args) => this.OnSeriesRemoved());
        }

        internal abstract void OnCreateView(DataPoint dataPoint);

        internal abstract void OnRemoveView(DataPoint dataPoint);

        internal abstract void OnUpdateView(DataPoint dataPoint);

        internal abstract void OnSeriesRemoved();
    }
}
