using System;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public interface INotifyTreeChanged
    {
        event EventHandler<TreeChangedEventArgs> TreeChanged;
    }
}
