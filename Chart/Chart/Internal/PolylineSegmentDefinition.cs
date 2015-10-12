﻿using System.Windows.Media;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class PolylineSegmentDefinition
    {
        public PointCollection Points { get; set; }

        public IAppearanceProvider Appearance { get; set; }

        public PolylineSegmentDefinition()
        {
            this.Points = new PointCollection();
        }
    }
}
