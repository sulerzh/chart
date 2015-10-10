
using System;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class PositiveTimeValueAggregator : TimeValueAggregator
    {
        public override bool CanPlot(TimeSpan timespan)
        {
            return timespan >= TimeSpan.Zero;
        }
    }
}
