using Semantic.Reporting.Windows.Common.Internal;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal interface IAppearanceProvider : INotifyValueChanged, INotifyPropertyChanged
    {
        Brush Fill { get; }

        Brush Stroke { get; }

        double StrokeThickness { get; }

        StrokeDashType StrokeDashType { get; }

        double Opacity { get; }

        Effect Effect { get; }
    }
}
