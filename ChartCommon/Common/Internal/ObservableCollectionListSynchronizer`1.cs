using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class ObservableCollectionListSynchronizer<TTarget> where TTarget : class
    {
        private WeakEventListener<ObservableCollectionListSynchronizer<TTarget>, object, NotifyCollectionChangedEventArgs> _weakEventListener;
        private IEnumerable _sourceCollection;

        public Func<object, TTarget> CreateItem { get; set; }

        public Action<TTarget> RemoveItem { get; set; }

        public Action<TTarget, object> ReplaceItem { get; set; }

        public Action ResetItems { get; set; }

        public Action StartUpdating { get; set; }

        public Action EndUpdating { get; set; }

        public IEnumerable SourceCollection
        {
            get
            {
                return this._sourceCollection;
            }
            set
            {
                if (this._sourceCollection == value)
                    return;
                if (this.StartUpdating != null)
                    this.StartUpdating();
                INotifyCollectionChanged newObservableCollection = value as INotifyCollectionChanged;
                this._sourceCollection = value;
                if (this._weakEventListener != null)
                {
                    this._weakEventListener.Detach();
                    this._weakEventListener = (WeakEventListener<ObservableCollectionListSynchronizer<TTarget>, object, NotifyCollectionChangedEventArgs>)null;
                }
                if (this.TargetList != null)
                {
                    EnumerableFunctions.ForEach<TTarget>((IEnumerable<TTarget>)this.TargetList, (Action<TTarget>)(item => this.OnTargetItemRemoved(item)));
                    this.TargetList.Clear();
                }
                this.Populate();
                if (newObservableCollection != null)
                {
                    this._weakEventListener = new WeakEventListener<ObservableCollectionListSynchronizer<TTarget>, object, NotifyCollectionChangedEventArgs>(this);
                    this._weakEventListener.OnEventAction = (Action<ObservableCollectionListSynchronizer<TTarget>, object, NotifyCollectionChangedEventArgs>)((instance, source, eventArgs) => instance.OnCollectionChanged(source, eventArgs));
                    this._weakEventListener.OnDetachAction = (Action<WeakEventListener<ObservableCollectionListSynchronizer<TTarget>, object, NotifyCollectionChangedEventArgs>>)(weakEventListener => newObservableCollection.CollectionChanged -= new NotifyCollectionChangedEventHandler(weakEventListener.OnEvent));
                    newObservableCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(this._weakEventListener.OnEvent);
                }
                if (this.EndUpdating == null)
                    return;
                this.EndUpdating();
            }
        }

        public IList<TTarget> TargetList { get; set; }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.TargetList == null)
                return;
            this.OnStartUpdating();
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (TTarget targetItem in (IEnumerable<TTarget>)this.TargetList)
                    this.OnTargetItemRemoved(targetItem);
                this.TargetList.Clear();
                foreach (object sourceItem in this.SourceCollection)
                    this.TargetList.Add(this.CreateTargetItem(sourceItem));
                this.OnResetItems();
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                for (int index = 0; index < e.NewItems.Count; ++index)
                {
                    if (this.ReplaceItem != null)
                    {
                        this.ReplaceItem(this.TargetList[e.NewStartingIndex + index], e.NewItems[index]);
                    }
                    else
                    {
                        this.OnTargetItemRemoved(this.TargetList[e.NewStartingIndex + index]);
                        this.TargetList[e.NewStartingIndex + index] = this.CreateTargetItem(e.NewItems[index]);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                for (int index = 0; index < e.OldItems.Count; ++index)
                {
                    this.OnTargetItemRemoved(this.TargetList[e.OldStartingIndex]);
                    this.TargetList.RemoveAt(e.OldStartingIndex);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                for (int index = 0; index < e.NewItems.Count; ++index)
                    this.TargetList.Insert(e.NewStartingIndex + index, this.CreateTargetItem(e.NewItems[index]));
            }
            this.OnEndUpdating();
        }

        private void Populate()
        {
            if (this.TargetList == null)
                return;
            if (this.SourceCollection != null)
            {
                foreach (object sourceItem in this.SourceCollection)
                    this.TargetList.Add(this.CreateTargetItem(sourceItem));
            }
            else
                this.TargetList.Clear();
        }

        private void OnTargetItemRemoved(TTarget targetItem)
        {
            if (this.RemoveItem == null)
                return;
            this.RemoveItem(targetItem);
        }

        private TTarget CreateTargetItem(object sourceItem)
        {
            if (this.CreateItem != null)
                return this.CreateItem(sourceItem);
            return default(TTarget);
        }

        private void OnStartUpdating()
        {
            if (this.StartUpdating == null)
                return;
            this.StartUpdating();
        }

        private void OnEndUpdating()
        {
            if (this.EndUpdating == null)
                return;
            this.EndUpdating();
        }

        private void OnResetItems()
        {
            if (this.ResetItems == null)
                return;
            this.ResetItems();
        }
    }
}
