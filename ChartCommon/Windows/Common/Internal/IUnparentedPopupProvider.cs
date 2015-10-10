using System.Windows.Controls.Primitives;

namespace Semantic.Reporting.Windows.Common.Internal
{
    internal interface IUnparentedPopupProvider
    {
        Popup UnparentedPopup { get; }
    }
}
