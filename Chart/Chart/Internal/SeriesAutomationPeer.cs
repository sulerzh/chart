using Semantic.Reporting.Windows.Chart.Internal.Properties;
using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public sealed class SeriesAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider
    {
        private Series Series
        {
            get
            {
                return (Series)this.Owner;
            }
        }

        bool ISelectionProvider.CanSelectMultiple
        {
            get
            {
                return true;
            }
        }

        bool ISelectionProvider.IsSelectionRequired
        {
            get
            {
                return false;
            }
        }

        public SeriesAutomationPeer(Series owner)
          : base((FrameworkElement)owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }

        protected override string GetClassNameCore()
        {
            return Resources.Automation_SeriesClassName;
        }

        protected override string GetAutomationIdCore()
        {
            string str = base.GetAutomationIdCore();
            if (string.IsNullOrEmpty(str))
            {
                str = this.GetName();
                if (this.Series.ChartArea != null)
                {
                    int num = EnumerableFunctions.IndexOf((IEnumerable)this.Series.ChartArea.GetSeries(), (object)this.Series);
                    if (num != -1)
                        str = "Series" + num.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                }
            }
            return str;
        }

        protected override string GetNameCore()
        {
            string str = base.GetNameCore();
            if (string.IsNullOrEmpty(str))
                str = string.Format((IFormatProvider)CultureInfo.CurrentCulture, Resources.SeriesScreenReaderLabel, new object[1]
                {
          (object) this.Series.LegendText
                });
            if (string.IsNullOrEmpty(str))
                str = this.GetClassName();
            if (string.IsNullOrEmpty(str))
                str = this.Series.Name;
            if (string.IsNullOrEmpty(str))
                str = this.Series.GetType().Name;
            return str;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Selection)
                return (object)this;
            return (object)null;
        }

        protected override Rect GetBoundingRectangleCore()
        {
            if (this.Series.ChartArea == null || !this.Series.ChartArea.IsTemplateApplied)
                return base.GetBoundingRectangleCore();
            return new FrameworkElementAutomationPeer((FrameworkElement)this.Series.SeriesPresenter.RootPanel).GetBoundingRectangle();
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            List<AutomationPeer> list = new List<AutomationPeer>();
            IEnumerable<AutomationPeer> collection = Enumerable.Select<DataPoint, AutomationPeer>(Enumerable.Where<DataPoint>((IEnumerable<DataPoint>)this.Series.DataPoints, (Func<DataPoint, bool>)(dataPoint => dataPoint.ViewState == DataPointViewState.Normal)), (Func<DataPoint, AutomationPeer>)(dataPoint => UIElementAutomationPeer.CreatePeerForElement((UIElement)dataPoint)));
            list.AddRange(collection);
            return list;
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            return Enumerable.ToArray<IRawElementProviderSimple>(Enumerable.Select<DataPoint, IRawElementProviderSimple>(this.Series.GetSelectedDataPoints(), (Func<DataPoint, IRawElementProviderSimple>)(dataPoint => this.ProviderFromPeer(UIElementAutomationPeer.CreatePeerForElement((UIElement)dataPoint)))));
        }
    }
}
