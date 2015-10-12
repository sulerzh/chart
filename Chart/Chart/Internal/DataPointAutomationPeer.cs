using Semantic.Reporting.Windows.Chart.Internal.Properties;
using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public sealed class DataPointAutomationPeer : FrameworkElementAutomationPeer, ISelectionItemProvider
    {
        private DataPoint DataPoint
        {
            get
            {
                return (DataPoint)this.Owner;
            }
        }

        private Series Series
        {
            get
            {
                return this.DataPoint.Series;
            }
        }

        bool ISelectionItemProvider.IsSelected
        {
            get
            {
                return this.DataPoint.IsSelected;
            }
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get
            {
                if (this.Series != null)
                    return this.ProviderFromPeer(UIElementAutomationPeer.CreatePeerForElement((UIElement)this.DataPoint.Series));
                return (IRawElementProviderSimple)null;
            }
        }

        public DataPointAutomationPeer(DataPoint owner)
          : base((FrameworkElement)owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.DataItem;
        }

        protected override string GetClassNameCore()
        {
            return Resources.Automation_DataPointClassName;
        }

        protected override string GetAutomationIdCore()
        {
            string str = base.GetAutomationIdCore();
            if (string.IsNullOrEmpty(str))
            {
                str = this.GetName();
                if (this.Series != null)
                {
                    int num = this.Series.DataPoints.IndexOf(this.DataPoint);
                    if (num != -1)
                        str = UIElementAutomationPeer.CreatePeerForElement((UIElement)this.Series).GetAutomationId() + "_DataPoint" + num.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                }
            }
            return str;
        }

        protected override string GetNameCore()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.DataPoint.IsSelected)
                stringBuilder.Append(Resources.DataPointHighlightedScreenReaderContent);
            stringBuilder.Append(Resources.DataPointDetailsScreenReaderContent);
            string str1 = base.GetNameCore();
            if (string.IsNullOrEmpty(str1) && this.DataPoint.ToolTipContent != null)
                str1 = this.DataPoint.ToolTipContent.AutomationName;
            if (string.IsNullOrEmpty(str1))
                str1 = this.DataPoint.ActualLabelContent as string;
            if (string.IsNullOrEmpty(str1) && this.DataPoint is XYDataPoint)
            {
                XYDataPoint xyDataPoint = (XYDataPoint)this.DataPoint;
                if (xyDataPoint.XValue != null)
                    str1 = ((XYDataPoint)this.DataPoint).XValue.ToString();
                if (xyDataPoint.YValue != null)
                {
                    if (!string.IsNullOrEmpty(str1))
                        str1 += "; ";
                    StringFormatConverter stringFormatConverter = new StringFormatConverter();
                    string str2 = ValueHelper.PrepareFormatString(xyDataPoint.StringFormat);
                    str1 += (string)stringFormatConverter.Convert(xyDataPoint.YValue, (Type)null, (object)str2, (CultureInfo)null);
                }
            }
            if (string.IsNullOrEmpty(str1))
                str1 = this.DataPoint.Name;
            if (string.IsNullOrEmpty(str1))
                str1 = this.DataPoint.GetType().Name;
            stringBuilder.Append(str1);
            return stringBuilder.ToString();
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.SelectionItem)
                return (object)this;
            return (object)null;
        }

        protected override bool IsOffscreenCore()
        {
            return this.DataPoint.ViewState != DataPointViewState.Normal;
        }

        protected override Rect GetBoundingRectangleCore()
        {
            if (this.Series == null || this.Series.ChartArea == null || !this.Series.ChartArea.IsTemplateApplied)
                return base.GetBoundingRectangleCore();
            FrameworkElement dataPointView = SeriesVisualStatePresenter.GetDataPointView(this.DataPoint);
            if (dataPointView != null)
                return new FrameworkElementAutomationPeer(dataPointView).GetBoundingRectangle();
            AutomationPeer peerForElement = UIElementAutomationPeer.CreatePeerForElement((UIElement)this.Series);
            if (!(this.Series is LineSeries) || this.DataPoint.View == null || peerForElement == null)
                return base.GetBoundingRectangleCore();
            Point anchorPoint = this.DataPoint.View.AnchorPoint;
            Rect rectangle = RectExtensions.Expand(new Rect(anchorPoint.X, anchorPoint.Y, 0.0, 0.0), 2.0, 2.0);
            Rect boundingRectangle = peerForElement.GetBoundingRectangle();
            Size renderSize = this.Series.SeriesPresenter.RootPanel.RenderSize;
            Size size = RectExtensions.GetSize(boundingRectangle);
            return RectExtensions.Translate(RectExtensions.Transform(rectangle, (Transform)new ScaleTransform()
            {
                ScaleX = (size.Width / renderSize.Width),
                ScaleY = (size.Height / renderSize.Height)
            }), boundingRectangle.X, boundingRectangle.Y);
        }

        void ISelectionItemProvider.AddToSelection()
        {
            if (this.DataPoint.IsSelected || this.Series == null || this.Series.ChartArea == null)
                return;
            this.Series.ChartArea.FireDataPointSelectionChanging(this.DataPoint, true);
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            if (!this.DataPoint.IsSelected || this.Series == null || this.Series.ChartArea == null)
                return;
            this.Series.ChartArea.FireDataPointSelectionChanging(this.DataPoint, true);
        }

        void ISelectionItemProvider.Select()
        {
            if (this.Series == null || this.Series.ChartArea == null)
                return;
            this.Series.ChartArea.FireDataPointSelectionChanging(this.DataPoint, false);
        }
    }
}
