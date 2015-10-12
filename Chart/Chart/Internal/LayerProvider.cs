﻿using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public class LayerProvider
    {
        private Dictionary<object, Panel> _layers = new Dictionary<object, Panel>();
        private Panel _parentPanel;

        public Panel ParentPanel
        {
            get
            {
                return this._parentPanel;
            }
            set
            {
                if (this._parentPanel == value)
                    return;
                if (this._parentPanel != null)
                    EnumerableFunctions.ForEachWithIndex<Panel>((IEnumerable<Panel>)this._layers.Values, (Action<Panel, int>)((item, index) => this._parentPanel.Children.Remove((UIElement)item)));
                this._parentPanel = value;
                if (this._parentPanel == null)
                    return;
                this.PopulateLayers();
            }
        }

        private void PopulateLayers()
        {
            if (this.ParentPanel == null)
                return;
            EnumerableFunctions.ForEachWithIndex<Panel>((IEnumerable<Panel>)this._layers.Values, (Action<Panel, int>)((item, index) => this.AddChildPanel((UIElement)item)));
        }

        public Panel GetLayer(Enum type)
        {
            return this.GetLayer((object)type, Convert.ToInt32((object)type, (IFormatProvider)CultureInfo.InvariantCulture), (Func<Panel>)(() => (Panel)new Canvas()));
        }

        public Panel GetLayer(object layerKey, Enum type)
        {
            return this.GetLayer(layerKey, Convert.ToInt32((object)type, (IFormatProvider)CultureInfo.InvariantCulture), (Func<Panel>)(() => (Panel)new Canvas()));
        }

        public Panel GetLayer(object layerKey, int zIndex, Func<Panel> createPanel)
        {
            Panel layer;
            if (!this._layers.TryGetValue(layerKey, out layer))
                layer = this.CreateLayer(layerKey, createPanel);
            if (zIndex != 0)
                Panel.SetZIndex((UIElement)layer, zIndex);
            return layer;
        }

        private Panel CreateLayer(object layerKey, Func<Panel> createPanel)
        {
            this.RemoveLayer(layerKey);
            Panel panel = createPanel();
            this._layers.Add(layerKey, panel);
            if (this.ParentPanel != null)
                this.AddChildPanel((UIElement)panel);
            return panel;
        }

        public void RemoveLayer(object layerKey)
        {
            Panel panel;
            if (!this._layers.TryGetValue(layerKey, out panel))
                return;
            if (this.ParentPanel != null)
                this.ParentPanel.Children.Remove((UIElement)panel);
            this._layers.Remove(layerKey);
        }

        public void RemoveLayer(Func<object, bool> test)
        {
            object key = Enumerable.FirstOrDefault<object>((IEnumerable<object>)this._layers.Keys, test);
            Panel panel;
            if (key == null || !this._layers.TryGetValue(key, out panel))
                return;
            if (this.ParentPanel != null)
                this.ParentPanel.Children.Remove((UIElement)panel);
            this._layers.Remove(key);
        }

        public void RemoveAllLayers()
        {
            if (this._parentPanel != null)
                EnumerableFunctions.ForEachWithIndex<Panel>((IEnumerable<Panel>)this._layers.Values, (Action<Panel, int>)((item, index) => this.ParentPanel.Children.Remove((UIElement)item)));
            this._layers.Clear();
        }

        protected virtual void AddChildPanel(UIElement item)
        {
            this.ParentPanel.Children.Add(item);
        }

        public void SetLayerVisibility(object layerKey, Visibility visibility)
        {
            Panel panel;
            if (!this._layers.TryGetValue(layerKey, out panel))
                return;
            panel.Visibility = visibility;
        }
    }
}
