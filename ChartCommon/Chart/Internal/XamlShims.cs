using System;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public static class XamlShims
    {
        public static string NewFrameworkElementName()
        {
            return "_" + Guid.NewGuid().ToString("N");
        }
    }
}
