using System.ComponentModel;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public interface INotifyValueChanged : INotifyPropertyChanged
    {
        event ValueChangedEventHandler ValueChanged;
    }
}
