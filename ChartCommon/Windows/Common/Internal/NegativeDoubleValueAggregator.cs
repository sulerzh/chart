﻿namespace Semantic.Reporting.Windows.Common.Internal
{
    public class NegativeDoubleValueAggregator : DoubleValueAggregator
    {
        protected override bool CanPlot(double x)
        {
            if (base.CanPlot(x))
                return x <= 0.0;
            return false;
        }
    }
}
