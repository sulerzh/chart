using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public interface IRequireMarginOnEdges
    {
        Thickness RequiredMargin { get; }

        void ResetRequiredMargin(bool resetScrollingInfo);

        void ScheduleUpdate();
    }
}
