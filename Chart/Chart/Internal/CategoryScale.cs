using Semantic.Reporting.Windows.Chart.Internal.Properties;
using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public class CategoryScale : Scale<double, double, CategoryScaleUnit>, ICategoryScale
    {
        private const int MaxActualViewSize = 1000;
        private const int SmallCount = 50;
        private int _minLeafIndex;
        private NumericSequence _majorSequence;
        private NumericSequence _minorSequence;
        private List<List<Category>> _realizedCategoryLevels;
        private Dictionary<object, int> _realizedLeafDictionary;
        private CategoryScale.DataSourceType _dataSourceType;
        private int _treeInvalidationCount;
        private ICategoryBinder _binder;
        private Binding _keyBinding;
        private Binding _contentBinding;
        private Binding _childrenBinding;
        private ITreeSource _treeSource;
        private IHierarchyVirtualizationHelper _virtualizationHelper;

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? Maximum
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, CategoryScaleUnit>.MaximumProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, CategoryScaleUnit>.MaximumProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? Minimum
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, CategoryScaleUnit>.MinimumProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, CategoryScaleUnit>.MinimumProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? ViewMaximum
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, CategoryScaleUnit>.ViewMaximumProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, CategoryScaleUnit>.ViewMaximumProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? ViewMinimum
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, CategoryScaleUnit>.ViewMinimumProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, CategoryScaleUnit>.ViewMinimumProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? Interval
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, CategoryScaleUnit>.MajorIntervalProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, CategoryScaleUnit>.MajorIntervalProperty, (object)value);
            }
        }

        public CategoryCollection Categories
        {
            get
            {
                Category category = this._treeSource.GetRoot() as Category;
                if (category == null)
                    return (CategoryCollection)null;
                return category.Children;
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? CrossingPosition
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, CategoryScaleUnit>.CrossingPositionProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, CategoryScaleUnit>.CrossingPositionProperty, (object)value);
            }
        }

        protected override Range<double> DefaultRange
        {
            get
            {
                return new Range<double>(0.0, 10.0);
            }
        }

        public override double ProjectedEndMargin
        {
            get
            {
                return 0.0;
            }
        }

        public override double ProjectedStartMargin
        {
            get
            {
                return 0.0;
            }
        }

        public ICategoryBinder Binder
        {
            get
            {
                return this._binder;
            }
            set
            {
                this._binder = value;
            }
        }

        public Binding KeyBinding
        {
            get
            {
                return this._keyBinding;
            }
            set
            {
                this._keyBinding = value;
            }
        }

        public Binding ContentBinding
        {
            get
            {
                return this._contentBinding;
            }
            set
            {
                this._contentBinding = value;
            }
        }

        public Binding ChildrenBinding
        {
            get
            {
                return this._childrenBinding;
            }
            set
            {
                this._childrenBinding = value;
            }
        }

        private ITreeSource TreeSource
        {
            get
            {
                return this._treeSource;
            }
            set
            {
                if (this._treeSource == value)
                    return;
                INotifyTreeChanged notifyTreeChanged1 = this._treeSource as INotifyTreeChanged;
                if (notifyTreeChanged1 != null)
                    notifyTreeChanged1.TreeChanged -= new EventHandler<TreeChangedEventArgs>(this.TreeChanged);
                this._treeSource = value;
                if (this._treeSource != null)
                    this._dataSourceType = CategoryScale.DataSourceType.TreeSource;
                INotifyTreeChanged notifyTreeChanged2 = this._treeSource as INotifyTreeChanged;
                if (notifyTreeChanged2 != null)
                    notifyTreeChanged2.TreeChanged += new EventHandler<TreeChangedEventArgs>(this.TreeChanged);
                this.Invalidate();
            }
        }

        public IHierarchyVirtualizationHelper VirtualizationHelper
        {
            get
            {
                return this._virtualizationHelper;
            }
            set
            {
                if (this._virtualizationHelper == value)
                    return;
                this._virtualizationHelper = value;
                if (value == null)
                {
                    this.TreeSource = (ITreeSource)new CategoryScale.DefaultTreeSource(this);
                }
                else
                {
                    if (this.ItemSource != null)
                        throw new InvalidOperationException(Properties.Resources.CategoryScale_VirtuizationHelperMustBeSetBeforeItemSource);
                    this.TreeSource = (ITreeSource)new CategoryScale.VirtualizedTreeSource(this, value);
                }
            }
        }

        public IEnumerable ItemSource
        {
            get
            {
                Category category = this._treeSource.GetRoot() as Category;
                if (category == null)
                    return (IEnumerable)null;
                return category.ChildrenSource;
            }
            set
            {
                this.BeginInit();
                try
                {
                    Category category = this._treeSource.GetRoot() as Category;
                    if (category == null)
                        throw new InvalidOperationException(Properties.Resources.CategoryScale_ItemSourceCannotBeSetWithTheCustomTreeSource);
                    category.ChildrenSource = value;
                    if (value != null)
                        this._dataSourceType = CategoryScale.DataSourceType.ItemSource;
                    this.Invalidate();
                }
                finally
                {
                    this.EndInit();
                }
            }
        }

        internal override int ActualMajorCount
        {
            get
            {
                return (int)Math.Floor(Math.Abs(this.ActualViewMaximum - this.ActualViewMinimum));
            }
        }

        internal override object DefaultLabelContent
        {
            get
            {
                return (object)"ABCDEFGH";
            }
        }

        public CategoryScale()
        {
            this.LabelDefinition.Format = (string)null;
            this._binder = (ICategoryBinder)new CategoryScale.DefaultBinder(this);
            this._treeSource = (ITreeSource)new CategoryScale.DefaultTreeSource(this);
            ((CategoryScale.DefaultTreeSource)this._treeSource).TreeChanged += new EventHandler<TreeChangedEventArgs>(this.TreeChanged);
        }

        public override bool CanProject(DataValueType valueType)
        {
            if (valueType != DataValueType.Category)
                return valueType == DataValueType.Auto;
            return true;
        }

        protected override double ConvertToPositionType(object value)
        {
            int num = -1;
            if (this._realizedLeafDictionary.TryGetValue(value, out num))
                return (double)(this._minLeafIndex + num);
            return -1.0;
        }

        public override void Recalculate()
        {
            this.CalculateActualProperties();
            this.RealizeCategories();
            NumericRangeInfo range = new NumericRangeInfo((DoubleR10)this.ActualViewMinimum, (DoubleR10)this.ActualViewMaximum);
            this._majorSequence = NumericSequence.Calculate(range, new double?(this.ActualMajorInterval), new double?(this.ActualMajorIntervalOffset), 1000, int.MinValue, (DoubleR10[])null, true, 1.0);
            this._minorSequence = NumericSequence.Calculate(range, new double?(this.ActualMinorInterval), new double?(this.ActualMinorIntervalOffset + 0.5), 1000, int.MinValue, (DoubleR10[])null, true, 1.0);
            base.Recalculate();
        }

        private void CalculateActualProperties()
        {
            this.ActualMajorInterval = this.Interval.HasValue ? (double)(int)this.Interval.Value : 1.0;
            this.ActualMajorIntervalOffset = 0.0;
            this.ActualMinorInterval = this.Interval.HasValue ? (double)(int)this.Interval.Value : 1.0;
            this.ActualMinorIntervalOffset = 0.0;
            this.ActualMinimum = this.Minimum.HasValue ? (double)(int)this.Minimum.Value : 0.0;
            this.ActualMaximum = this.Maximum.HasValue ? (double)(int)this.Maximum.Value : (double)this._treeSource.GetLeafCount();
            this.CalculateActualView();
        }

        private void CalculateActualView()
        {
            double viewMinimum = this.ViewMinimum ?? this.ActualMinimum;
            double viewMaximum = this.ViewMaximum ?? viewMinimum + 1000.0;
            this.BoxViewRange(ref viewMinimum, ref viewMaximum);
            this.ActualViewMinimum = viewMinimum;
            this.ActualViewMaximum = viewMaximum;
        }

        protected override double GetMaxPossibleZoom()
        {
            return 1.0 / this.ConvertToPercent((object)1);
        }

        private void RealizeCategories()
        {
            if (this._treeSource.GetChildCount(this._treeSource.GetRoot()) > 0)
            {
                this._minLeafIndex = (int)Math.Floor(this.ActualViewMinimum);
                this._realizedCategoryLevels = this.GetCategoriesByLevels(this.GetPath(this._treeSource.GetLeaf(this._minLeafIndex)), (int)Math.Ceiling(this.ActualViewMaximum - this.ActualViewMinimum));
            }
            else
            {
                this._realizedCategoryLevels = new List<List<Category>>();
                this._realizedCategoryLevels.Add(new List<Category>());
            }
            this._realizedLeafDictionary = this.GetRealizedLeafDictionary();
        }

        internal override void RecalculateIfEmpty()
        {
            if (this._majorSequence != null && this._minorSequence != null)
                return;
            this.Recalculate();
        }

        public override double Project(double value)
        {
            this.RecalculateIfEmpty();
            return RangeHelper.Project(new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum), value, Scale.PercentRange);
        }

        public override double ProjectDataValue(object value)
        {
            double num = this.ConvertToPositionType(value);
            if (num < this.ActualMinimum || num > this.ActualMaximum)
                return double.NaN;
            return RangeHelper.Project(new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum), num + 0.5, Scale.PercentRange);
        }

        public override double ConvertToPercent(object value)
        {
            if (value == null)
                return double.NaN;
            this.RecalculateIfEmpty();
            Range<double> fromRange = new Range<double>(this.ActualMinimum, this.ActualMaximum);
            double num = Convert.ToDouble(value, (IFormatProvider)CultureInfo.InvariantCulture);
            if (DoubleHelper.LessWithPrecision(num, fromRange.Minimum) || DoubleHelper.GreaterWithPrecision(num, fromRange.Maximum))
                return double.NaN;
            return RangeHelper.Project(fromRange, num, Scale.PercentRange);
        }

        public override IEnumerable<double> ProjectValues(IEnumerable values)
        {
            this.RecalculateIfEmpty();
            if (values != null)
            {
                Range<double> fromRange = new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum);
                foreach (object obj in values)
                {
                    double x = this.ConvertToPositionType(obj);
                    if (x < 0.0)
                        yield return double.NaN;
                    else
                        yield return RangeHelper.Project(fromRange, x + 0.5, Scale.PercentRange);
                }
            }
        }

        private IEnumerable<ScalePosition> Project(IList<Category> categories, NumericSequence sequence, double interval)
        {
            this.RecalculateIfEmpty();
            Range<double> fromRange = new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum);
            Category category = (Category)null;
            foreach (DoubleR10 doubleR10 in sequence)
            {
                double x = (double)doubleR10;
                int index = (int)Math.Floor(x) - this._minLeafIndex;
                if (index >= 0 && index < categories.Count)
                {
                    category = categories[index];
                    double bucketMinX = (double)(this._minLeafIndex + category.LeafIndexRange.Minimum);
                    double bucketMaxX = (double)(this._minLeafIndex + category.LeafIndexRange.Maximum);
                    yield return new ScalePosition(category.Content, RangeHelper.Project(fromRange, x, Scale.PercentRange), RangeHelper.Project(fromRange, bucketMinX, Scale.PercentRange), RangeHelper.Project(fromRange, bucketMaxX, Scale.PercentRange));
                }
                if (sequence == this._majorSequence && category != null && index == categories.Count)
                {
                    double bucketMinX = Math.Floor(x);
                    double bucketMaxX = bucketMinX;
                    yield return new ScalePosition(category.Content, RangeHelper.Project(fromRange, x, Scale.PercentRange), RangeHelper.Project(fromRange, bucketMinX, Scale.PercentRange), RangeHelper.Project(fromRange, bucketMaxX, Scale.PercentRange));
                }
            }
        }

        private IEnumerable<ScalePosition> Project(IList<Category> categories, ScaleElementDefinition element, CategoryScaleUnit defaultUnit)
        {
            this.RecalculateIfEmpty();
            double? interval1 = Scale<double, double, CategoryScaleUnit>.GetInterval(element);
            double? intervalOffset = Scale<double, double, CategoryScaleUnit>.GetIntervalOffset(element);
            CategoryScaleUnit? intervalUnit = Scale<double, double, CategoryScaleUnit>.GetIntervalUnit(element);
            double interval2 = interval1.HasValue ? interval1.Value : 1.0;
            double num1 = intervalOffset.HasValue ? intervalOffset.Value : 0.0;
            CategoryScaleUnit categoryScaleUnit = intervalUnit.HasValue ? intervalUnit.Value : defaultUnit;
            double num2 = defaultUnit == CategoryScaleUnit.MinorInterval ? 0.5 : 0.0;
            NumericSequence numericSequence = defaultUnit == CategoryScaleUnit.MinorInterval ? this._minorSequence : this._majorSequence;
            NumericSequence sequence = (NumericSequence)null;
            NumericRangeInfo range = new NumericRangeInfo((DoubleR10)this.ActualMinimum, (DoubleR10)this.ActualMaximum);
            switch (categoryScaleUnit)
            {
                case CategoryScaleUnit.Index:
                    sequence = interval2 != this.ActualMajorInterval || num1 != this.ActualMinorIntervalOffset ? NumericSequence.Calculate(range, new double?(interval2), new double?(num1 + num2), 100, int.MinValue, (DoubleR10[])null, true, 1.0) : numericSequence;
                    break;
                case CategoryScaleUnit.MajorInterval:
                    sequence = interval2 != this.ActualMajorInterval || num1 != this.ActualMajorIntervalOffset ? (NumericSequence)RelativeSequence.Calculate(this._majorSequence, new double?(interval2), new double?(num1), 1.0) : this._majorSequence;
                    break;
                case CategoryScaleUnit.MinorInterval:
                    sequence = interval2 != this.ActualMinorInterval || num1 != this.ActualMinorIntervalOffset ? (NumericSequence)RelativeSequence.Calculate(this._minorSequence, new double?(interval2), new double?(num1 + 0.5), 1.0) : this._minorSequence;
                    break;
            }
            return this.Project(categories, sequence, interval2);
        }

        public override IEnumerable<ScaleElementDefinition> ProjectElements()
        {
            IList<Category> leafs = this.GetRealizedLeafList();
            this.MajorTickmarkDefinition.Positions = this.ProjectMajor(leafs, (ScaleElementDefinition)this.MajorTickmarkDefinition);
            this.MinorTickmarkDefinition.Positions = this.ProjectMinor(leafs, (ScaleElementDefinition)this.MinorTickmarkDefinition, 0);
            yield return (ScaleElementDefinition)this.MajorTickmarkDefinition;
            yield return (ScaleElementDefinition)this.MinorTickmarkDefinition;
            if (this._realizedCategoryLevels != null)
            {
                for (int i = this._realizedCategoryLevels.Count - 1; i >= 0; --i)
                {
                    List<Category> level = this._realizedCategoryLevels[i];
                    LabelDefinition label = this.CloneLabelDefinition(this.LabelDefinition);
                    label.Level = i;
                    label.Positions = this.ProjectMinor((IList<Category>)level, (ScaleElementDefinition)this.LabelDefinition, i);
                    yield return (ScaleElementDefinition)label;
                }
            }
        }

        private IEnumerable<ScalePosition> ProjectMinor(IList<Category> categories, ScaleElementDefinition definition, int level)
        {
            this.RecalculateIfEmpty();
            int step = 1;
            int maxCount = this.MaxCount;
            if (categories.Count > maxCount && categories.Count > 50)
            {
                if (level != this._realizedCategoryLevels.Count - 1)
                {
                    yield break;
                }
                else
                {
                    switch (maxCount)
                    {
                        case 0:
                            yield break;
                        case 1:
                            yield break;
                        case 2:
                            step = categories.Count - 1;
                            break;
                        default:
                            step = categories.Count / (maxCount - 1);
                            if (categories.Count % maxCount != 0)
                            {
                                step = (int)Math.Ceiling((double)categories.Count / ((double)maxCount - 2.0));
                                break;
                            }
                            break;
                    }
                }
            }
            Range<double> fromRange = new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum);
            int i = 0;
            while (i < categories.Count)
            {
                Category category = categories[i];
                double bucketMinX = (double)(this._minLeafIndex + category.LeafIndexRange.Minimum);
                double bucketMaxX = (double)(this._minLeafIndex + category.LeafIndexRange.Maximum);
                double x = (bucketMaxX + bucketMinX) / 2.0;
                yield return new ScalePosition(category.Content, RangeHelper.Project(fromRange, x, Scale.PercentRange), RangeHelper.Project(fromRange, bucketMinX, Scale.PercentRange), RangeHelper.Project(fromRange, bucketMaxX, Scale.PercentRange));
                i += step;
            }
        }

        private IEnumerable<ScalePosition> ProjectMajor(IList<Category> categories, ScaleElementDefinition definition)
        {
            this.RecalculateIfEmpty();
            int maxCount = this.MaxCount * 2;
            if (categories.Count <= maxCount || categories.Count <= 50)
            {
                Range<double> fromRange = new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum);
                Category lastCategory = (Category)null;
                foreach (Category category in (IEnumerable<Category>)categories)
                {
                    double bucketMinX = (double)(this._minLeafIndex + category.LeafIndexRange.Minimum);
                    double bucketMaxX = (double)(this._minLeafIndex + category.LeafIndexRange.Maximum);
                    double x = bucketMinX;
                    yield return new ScalePosition(category.Content, RangeHelper.Project(fromRange, x, Scale.PercentRange), RangeHelper.Project(fromRange, bucketMinX, Scale.PercentRange), RangeHelper.Project(fromRange, bucketMaxX, Scale.PercentRange));
                    lastCategory = category;
                }
                if (lastCategory != null)
                {
                    double x = (double)(this._minLeafIndex + lastCategory.LeafIndexRange.Maximum);
                    double projectedX = RangeHelper.Project(fromRange, x, Scale.PercentRange);
                    yield return new ScalePosition((object)string.Empty, projectedX, projectedX, projectedX);
                }
            }
        }

        private LabelDefinition CloneLabelDefinition(LabelDefinition from)
        {
            LabelDefinition labelDefinition = new LabelDefinition();
            labelDefinition.Group = from.Group;
            labelDefinition.Format = from.Format;
            labelDefinition.Visibility = from.Visibility;
            return labelDefinition;
        }

        public override double ProjectClusterSize(IEnumerable values)
        {
            return this.ProjectMajorIntervalSize();
        }

        public override IEnumerable<ScalePosition> ProjectMajorIntervals()
        {
            return this.Project(this.GetRealizedLeafList(), (ScaleElementDefinition)this.MajorTickmarkDefinition, CategoryScaleUnit.MajorInterval);
        }

        public override bool TryChangeInterval(double ratio)
        {
            return false;
        }

        public override bool TryChangeMaxCount(double maxMajorCount)
        {
            if (maxMajorCount < 1.0 || maxMajorCount >= 1000.0)
                return false;
            this.MaxCount = (int)maxMajorCount;
            return true;
        }

        public override void ScrollToValue(double position)
        {
            double num = this.ActualViewMaximum - this.ActualViewMinimum;
            double viewMinimum = position;
            double viewMaximum = position + num;
            this.BoxViewRange(ref viewMinimum, ref viewMaximum);
            double precision = DoubleHelper.GetPrecision(new double[2]
            {
        viewMinimum,
        this.ActualViewMinimum
            });
            if (DoubleHelper.EqualsWithPrecision(viewMinimum, this.ActualViewMinimum, precision))
                return;
            this._majorSequence.MoveToCover((DoubleR10)viewMinimum, (DoubleR10)viewMaximum);
            this._minorSequence.MoveToCover((DoubleR10)viewMinimum, (DoubleR10)viewMaximum);
            this.BeginInit();
            this.ViewMinimum = new double?(this.ActualViewMinimum = viewMinimum);
            this.ViewMaximum = new double?(this.ActualViewMaximum = viewMaximum);
            this.IsScrolling = true;
            this.RealizeCategories();
            this.EndInit();
        }

        public override void ScrollToPercent(double position)
        {
            Range<double> targetRange = new Range<double>(this.ActualMinimum, this.ActualMaximum);
            this.ScrollToValue(RangeHelper.Project(Scale.PercentRange, position, targetRange));
        }

        public override void ZoomToPercent(double viewMinimum, double viewMaximum)
        {
            double minSize = 1.0 / this.ActualZoomRange.Maximum;
            double maxSize = 1.0 / this.ActualZoomRange.Minimum;
            RangeHelper.BoxRangeInsideAnother(ref viewMinimum, ref viewMaximum, 0.0, 1.0, minSize, maxSize, 0.0);
            Range<double> targetRange = new Range<double>(this.ActualMinimum, this.ActualMaximum);
            this.ZoomToValue(RangeHelper.Project(Scale.PercentRange, viewMinimum, targetRange), RangeHelper.Project(Scale.PercentRange, viewMaximum, targetRange));
        }

        internal override void BoxViewRange(ref double viewMinimum, ref double viewMaximum)
        {
            RangeHelper.BoxRangeInsideAnother(ref viewMinimum, ref viewMaximum, this.ActualMinimum, this.ActualMaximum);
        }

        public override void UpdateRange(IEnumerable<Range<IComparable>> ranges)
        {
        }

        internal override void UpdateRangeIfUndefined(IEnumerable<Range<IComparable>> ranges)
        {
        }

        internal override void UpdateValuesIfUndefined(IEnumerable<object> values)
        {
            if (this._dataSourceType != CategoryScale.DataSourceType.Series && (this.Categories.Count != 0 || this._dataSourceType == CategoryScale.DataSourceType.TreeSource))
                return;
            this.BeginInit();
            try
            {
                this.Categories.Clear();
                foreach (object obj in Enumerable.Distinct<object>(values))
                {
                    if (obj != null)
                        this.Categories.Add(new Category()
                        {
                            Key = obj,
                            Content = obj
                        });
                }
                this._dataSourceType = CategoryScale.DataSourceType.Series;
            }
            finally
            {
                this.EndInit();
            }
        }

        private void TreeChanged(object sender, TreeChangedEventArgs e)
        {
            this.InvalidateTree();
        }

        internal void InvalidateTree()
        {
            ++this._treeInvalidationCount;
            if (this.IsScrolling)
                this.ResetView();
            this.InvalidateView();
            this.Invalidate();
        }

        protected override void ResetView()
        {
            this.BeginInit();
            base.ResetView();
            this.ViewMinimum = new double?();
            this.ViewMaximum = new double?();
            this.EndInit();
        }

        private object[] GetPath(object item)
        {
            List<object> list = new List<object>();
            for (; item != null; item = this._treeSource.GetParent(item))
                list.Insert(0, item);
            return list.ToArray();
        }

        private List<List<Category>> GetCategoriesByLevels(object[] path, int maxLeafCount)
        {
            List<List<Category>> levels = new List<List<Category>>();
            if (path.Length < 2)
                return levels;
            for (int index = 0; index < path.Length - 1; ++index)
            {
                List<Category> list = new List<Category>();
                levels.Add(list);
            }
            int leafCount = 0;
            this.CollectChildCategories(path[0], path, ref leafCount, maxLeafCount, levels, 0);
            return levels;
        }

        private void CollectChildCategories(object parent, object[] initialPath, ref int leafCount, int maxLeafCount, List<List<Category>> levels, int levelIndex)
        {
            List<Category> list = levels[levelIndex];
            int num = 0;
            if (list.Count == 0)
                num = this._treeSource.GetChildIndex(parent, initialPath[levelIndex + 1]);
            int childCount = this._treeSource.GetChildCount(parent);
            for (int index = num; index < childCount && leafCount < maxLeafCount; ++index)
            {
                object child = this._treeSource.GetChild(parent, index);
                if (child == null)
                    break;
                Category category = child as Category;
                if (category == null)
                {
                    category = (Category)new ShadowCategory();
                    category.Scale = this;
                    this.Binder.Bind(category, child);
                }
                int minimum = leafCount;
                if (levelIndex == levels.Count - 1)
                    ++leafCount;
                else
                    this.CollectChildCategories(child, initialPath, ref leafCount, maxLeafCount, levels, levelIndex + 1);
                category.LeafIndexRange = new Range<int>(minimum, leafCount);
                list.Add(category);
            }
        }

        private IList<Category> GetRealizedLeafList()
        {
            if (this._realizedCategoryLevels != null && this._realizedCategoryLevels.Count > 0)
                return (IList<Category>)this._realizedCategoryLevels[this._realizedCategoryLevels.Count - 1];
            return (IList<Category>)new List<Category>();
        }

        private Dictionary<object, int> GetRealizedLeafDictionary()
        {
            Dictionary<object, int> dictionary = new Dictionary<object, int>();
            int num = 0;
            foreach (Category category in (IEnumerable<Category>)this.GetRealizedLeafList())
                dictionary.Add(category.Key, num++);
            return dictionary;
        }

        protected override bool NeedsRecalculation()
        {
            bool flag = base.NeedsRecalculation() || this._treeInvalidationCount > 0;
            this._treeInvalidationCount = 0;
            return flag;
        }

        private enum DataSourceType
        {
            None,
            Series,
            ItemSource,
            TreeSource,
        }

        private class DefaultBinder : ICategoryBinder
        {
            private CategoryScale _scale;

            public DefaultBinder(CategoryScale scale)
            {
                this._scale = scale;
            }

            public void Bind(Category category, object businessObject)
            {
                if (this._scale.KeyBinding == null)
                    throw new InvalidOperationException(Properties.Resources.CategoryScale_SetKeyBindingBeforeSettingItemSource);
                category.SetBinding(Category.KeyProperty, (BindingBase)this._scale.KeyBinding);
                if (this._scale.ContentBinding == null)
                    throw new InvalidOperationException(Properties.Resources.CategoryScale_SetContentBindingBeforeSettingItemSource);
                category.SetBinding(Category.ContentProperty, (BindingBase)this._scale.ContentBinding);
                if (this._scale.ChildrenBinding != null)
                    category.SetBinding(Category.ChildrenSourceProperty, (BindingBase)this._scale.ChildrenBinding);
                category.DataContext = businessObject;
            }
        }

        private class DefaultTreeSource : ITreeSource
        {
            private Category _root;
            private List<Category> _leafs;

            public event EventHandler<TreeChangedEventArgs> TreeChanged;

            public DefaultTreeSource(CategoryScale scale)
            {
                this._root = new Category();
                this._root.Scale = scale;
                this._root.TreeChanged += new EventHandler<TreeChangedEventArgs>(this._root_TreeChanged);
            }

            private void _root_TreeChanged(object sender, TreeChangedEventArgs e)
            {
                this._leafs = (List<Category>)null;
                if (this.TreeChanged == null)
                    return;
                this.TreeChanged(sender, e);
            }

            private IList<Category> GetLeafs()
            {
                if (this._leafs == null)
                    this._leafs = Enumerable.ToList<Category>(this.GetLeafs(this._root));
                return (IList<Category>)this._leafs;
            }

            private IEnumerable<Category> GetLeafs(Category category)
            {
                foreach (Category category1 in (Collection<Category>)category.Children)
                {
                    if (category1.IsLeaf)
                    {
                        yield return category1;
                    }
                    else
                    {
                        foreach (Category category2 in this.GetLeafs(category1))
                            yield return category2;
                    }
                }
            }

            public object GetRoot()
            {
                return (object)this._root;
            }

            public bool IsLeaf(object item)
            {
                Category category = (Category)item;
                if (category.Children != null)
                    return category.Children.Count == 0;
                return true;
            }

            public int GetChildCount(object parent)
            {
                Category category = (Category)parent;
                if (category.Children == null)
                    return 0;
                return category.Children.Count;
            }

            public object GetChild(object parent, int index)
            {
                Category category = (Category)parent;
                if (index < 0 || index >= category.Children.Count)
                    return (object)null;
                return (object)category.Children[index];
            }

            public object GetParent(object child)
            {
                return (object)((Category)child).ParentCategory;
            }

            public int GetChildIndex(object parent, object child)
            {
                return EnumerableFunctions.IndexOf((IEnumerable)((Category)parent).Children, child);
            }

            public int GetLeafCount()
            {
                return this.GetLeafs().Count;
            }

            public object GetLeaf(int leafIndex)
            {
                IList<Category> leafs = this.GetLeafs();
                if (leafIndex < 0 || leafIndex >= leafs.Count)
                    return (object)null;
                return (object)leafs[leafIndex];
            }
        }

        private class VirtualizedTreeSource : ITreeSource, INotifyTreeChanged
        {
            private Category _root;
            private IHierarchyVirtualizationHelper _virtualizer;
            private WeakEventListener<CategoryScale.VirtualizedTreeSource, object, EventArgs> _leavesChangedWeakEventListener;
            private CategoryScale _scale;

            public event EventHandler<TreeChangedEventArgs> TreeChanged;

            public VirtualizedTreeSource(CategoryScale scale, IHierarchyVirtualizationHelper virtualizer)
            {
                this._root = (Category)new ShadowCategory();
                this._scale = scale;
                this._virtualizer = virtualizer;
                this._leavesChangedWeakEventListener = new WeakEventListener<CategoryScale.VirtualizedTreeSource, object, EventArgs>(this);
                this._leavesChangedWeakEventListener.OnEventAction = (Action<CategoryScale.VirtualizedTreeSource, object, EventArgs>)((instance, source, eventArgs) => instance.OnVirtualizerLeavesChanged(source, eventArgs));
                this._leavesChangedWeakEventListener.OnDetachAction = (Action<WeakEventListener<CategoryScale.VirtualizedTreeSource, object, EventArgs>>)(weakEventListener => this._virtualizer.LeavesChanged -= new EventHandler(weakEventListener.OnEvent));
                this._virtualizer.LeavesChanged += new EventHandler(this._leavesChangedWeakEventListener.OnEvent);
            }

            private IList GetChildrenList(object item)
            {
                Category category = item as Category;
                if (category == null)
                {
                    category = (Category)new ShadowCategory();
                    this._scale.Binder.Bind(category, item);
                }
                return category.GetChildrenItemsList();
            }

            private void OnVirtualizerLeavesChanged(object sender, EventArgs e)
            {
                if (this.TreeChanged == null)
                    return;
                this.TreeChanged((object)this, new TreeChangedEventArgs(sender, e));
            }

            public object GetRoot()
            {
                return (object)this._root;
            }

            public bool IsLeaf(object item)
            {
                IList childrenList = this.GetChildrenList(item);
                if (childrenList != null)
                    return childrenList.Count == 0;
                return true;
            }

            public int GetChildCount(object parent)
            {
                IList childrenList = this.GetChildrenList(parent);
                if (childrenList != null)
                    return childrenList.Count;
                return 0;
            }

            public object GetChild(object parent, int index)
            {
                IList childrenList = this.GetChildrenList(parent);
                if (index < 0 || index >= childrenList.Count)
                    return (object)null;
                return childrenList[index];
            }

            public object GetParent(object child)
            {
                if (child == this._root)
                    return (object)null;
                return this._virtualizer.GetParent(child) ?? (object)this._root;
            }

            public int GetChildIndex(object parent, object child)
            {
                return this.GetChildrenList(parent).IndexOf(child);
            }

            public int GetLeafCount()
            {
                return this._virtualizer.GetLeafCount();
            }

            public object GetLeaf(int leafIndex)
            {
                return this._virtualizer.GetLeaf(leafIndex);
            }
        }

        internal class CategoryScaleFactory : Scale.ScaleFactory
        {
            public override Scale Create(DataValueType valueType)
            {
                CategoryScale categoryScale = new CategoryScale();
                categoryScale.ValueType = valueType;
                return (Scale)categoryScale;
            }
        }
    }
}
