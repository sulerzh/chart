using System.Windows.Media;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public interface ISelectableView
    {
        Geometry GetSelectionGeometry();
    }
}
