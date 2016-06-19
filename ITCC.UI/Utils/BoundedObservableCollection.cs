using System.Collections.ObjectModel;

namespace ITCC.UI.Utils
{
    public class BoundedObservableCollection<TItem> : ObservableCollection<TItem>
    {
        public BoundedObservableCollection(int capacity)
        {
            Capacity = capacity;
        }

        public int Capacity { get; }

        protected override void InsertItem(int index, TItem item)
        {
            if (Count == Capacity)
            {
                RemoveAt(0);
                base.InsertItem(index - 1, item);
            }
            else
            {
                base.InsertItem(index, item);
            }
        }
    }
}