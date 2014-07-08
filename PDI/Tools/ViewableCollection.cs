using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

namespace PDI.Tools
{
    public class ViewableCollection<T> : ObservableCollection<T>
    {
        public ViewableCollection()
        {

        }

        public ViewableCollection(IEnumerable<T> source)
        {
            Fill(source);
        }

        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
        private bool suspendNotification = false;

        public void Fill(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items", "items is null.");

            lock (this)
            {
                Clear();
                foreach (T item in items)
                    Add(item);
            }
        }

        public void BulkFill(IEnumerable<T> items)
        {
            try
            {
                SuspendNotification();
                Fill(items);
            }
            finally
            {
                ResumNotification();
            }
        }

        public void SuspendNotification()
        {
            suspendNotification = true;
        }

        public void ResumNotification()
        {
            if (suspendNotification)
            {
                suspendNotification = false;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (suspendNotification)
                return;

            if (SynchronizationContext.Current == _synchronizationContext)
            {
                // Execute the CollectionChanged event on the current thread
                RaiseCollectionChanged(e);
            }
            else
            {
                // Post the CollectionChanged event on the creator thread
                _synchronizationContext.Post(RaiseCollectionChanged, e);
            }
        }

        private void RaiseCollectionChanged(object param)
        {
            // We are in the creator thread, call the base implementation directly
            if (!suspendNotification)
                base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

    }
}
