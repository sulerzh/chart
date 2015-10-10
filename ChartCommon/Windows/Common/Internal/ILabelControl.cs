﻿using System.Windows;
using System.Windows.Media;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public interface ILabelControl
    {
        Geometry CalloutGeometry { get; set; }

        void UpdateCalloutGeometry(Point start, Point end);

        Size GetDesiredSize();
    }
}
