using Semantic.Reporting.Windows.Common.Internal;
using System;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public class ScaleViewChangedArgs : EventArgs
    {
        public Range<IComparable> OldRange { get; set; }

        public Range<IComparable> NewRange { get; set; }
    }
}
