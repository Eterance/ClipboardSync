using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace ClipboardSync.Common.Collections
{
    /// <summary>
    /// Thread-safe collection. You can safely bind it to a WPF control using the property <see cref="AsObservable"/>.
    /// </summary>
    public sealed class ConcurrentObservableCollection<T> : IList<T>, IReadOnlyList<T>, IList//, INotifyCollectionChanged
    {
        /// <summary>
        /// On WPF, ObservableCollection cannot modify out of the dispatch thread
        /// If UIDispatcherInvoker assigned, meaning all ObservableCollection modification will using this DispatcherInvoke
        /// </summary>
        public Action<Action>? UIDispatcherInvoker { get; private set; } = null;


        private readonly object _lock = new();

        private ImmutableList<T> _items = ImmutableList<T>.Empty;
        private ObservableCollection<T>? _observableCollection;

        public ConcurrentObservableCollection()
        {
        }

        public ConcurrentObservableCollection(Action<Action> uiDispatcherInvoker)
        {
            UIDispatcherInvoker = uiDispatcherInvoker;
        }

        public ConcurrentObservableCollection(List<T> list, Action<Action>? uiDispatcherInvoker = null)
        {
            _items.AddRange(list);
            UIDispatcherInvoker = uiDispatcherInvoker;
        }

        /// <summary>
        /// In WPF platform, change binding list outside Ui thread is illegal.
        /// </summary>
        /// <param name="act"></param>
        private void UseUIDispatcherInvoke(Action act)
        {
            // https://stackoverflow.com/questions/18331723/this-type-of-collectionview-does-not-support-changes-to-its-sourcecollection-fro
            if (UIDispatcherInvoker != null)
            {
                UIDispatcherInvoker(act);
            }
            else
            {
                act();
            }
        }

        public ObservableCollection<T> AsObservable
        {
            get
            {
                if (_observableCollection == null)
                {
                    lock (_lock)
                    {
                        if (_observableCollection == null)
                        {
                            UseUIDispatcherInvoke(() =>
                            {
                                _observableCollection = new();
                                _observableCollection.CollectionChanged += _observableCollection_CollectionChanged;
                            });
                        }
                    }
                }

                return _observableCollection;
            }
        }

        private void _observableCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.IsReadOnly => false;

        public int Count => _items.Count;

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        int ICollection.Count => Count;

        object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

        bool ICollection.IsSynchronized => ((ICollection)_items).IsSynchronized;

        object? IList.this[int index]
        {
            get => this[index];
            set
            {
                AssertType(value, nameof(value));
                this[index] = (T)value!;
            }
        }

        public T this[int index]
        {
            get => _items[index];
            set
            {
                lock (_lock)
                {
                    _items = _items.SetItem(index, value);
                    if (_observableCollection != null)
                    {
                        UseUIDispatcherInvoke(() => _observableCollection.Insert(index, value));
                    }
                }
            }
        }

        public void Add(T item)
        {
            lock (_lock)
            {
                _items = _items.Add(item);
                UseUIDispatcherInvoke(() => _observableCollection?.Add(item));
            }
        }

        public void AddRange(params T[] items)
        {
            AddRange((IEnumerable<T>)items);
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (_lock)
            {
                var count = _items.Count;
                _items = _items.AddRange(items);
                if (_observableCollection != null)
                {
                    for (var i = count; i < _items.Count; i++)
                    {
                        UseUIDispatcherInvoke(() => _observableCollection?.Add(_items[i]));
                    }
                }
            }
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            lock (_lock)
            {
                var count = _items.Count;
                _items = _items.InsertRange(index, items);
                var addedItemsCount = _items.Count - count;
                if (_observableCollection != null)
                {
                    for (var i = index; i < index + addedItemsCount; i++)
                    {
                        UseUIDispatcherInvoke(() => _observableCollection?.Insert(i, _items[i]));
                    }
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _items = _items.Clear();
                UseUIDispatcherInvoke(() => _observableCollection?.Clear());
            }
        }

        public void Insert(int index, T item)
        {
            lock (_lock)
            {
                _items = _items.Insert(index, item);
                UseUIDispatcherInvoke(() => _observableCollection?.Insert(index, item));
            }
        }

        public bool Remove(T item)
        {
            lock (_lock)
            {
                var newList = _items.Remove(item);
                if (_items != newList)
                {
                    _items = newList;
                    UseUIDispatcherInvoke(() => _observableCollection?.Remove(item));
                    return true;
                }

                return false;
            }
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                _items = _items.RemoveAt(index);
                UseUIDispatcherInvoke(() => _observableCollection?.RemoveAt(index));
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /*public void Sort()
        {
            Sort(comparer: null);
        }

        public void Sort(IComparer<T>? comparer)
        {
            lock (_lock)
            {
                _items = _items.Sort(comparer);
                _observableCollection?.EnqueueReset(_items);
            }
        }

        public void StableSort()
        {
            StableSort(comparer: null);
        }

        public void StableSort(IComparer<T>? comparer)
        {
            lock (_lock)
            {
                _items = ImmutableList.CreateRange(_items.OrderBy(item => item, comparer));
                _observableCollection?.EnqueueReset(_items);
            }
        }*/

        int IList.Add(object? value)
        {
            AssertType(value, nameof(value));
            var item = (T)value!;
            lock (_lock)
            {
                var index = _items.Count;
                _items = _items.Add(item);
                UseUIDispatcherInvoke(() => _observableCollection?.Add(item));
                return index;
            }
        }

        bool IList.Contains(object? value)
        {
            AssertType(value, nameof(value));
            return Contains((T)value!);
        }

        void IList.Clear()
        {
            Clear();
        }

        int IList.IndexOf(object? value)
        {
            AssertType(value, nameof(value));
            return IndexOf((T)value!);
        }

        void IList.Insert(int index, object? value)
        {
            AssertType(value, nameof(value));
            Insert(index, (T)value!);
        }

        void IList.Remove(object? value)
        {
            AssertType(value, nameof(value));
            Remove((T)value!);
        }

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_items).CopyTo(array, index);
        }

        private static void AssertType(object? value, string argumentName)
        {
            if (value is null || value is T)
                return;

            throw new ArgumentException($"value must be of type '{typeof(T).FullName}'", argumentName);
        }
    }
}
