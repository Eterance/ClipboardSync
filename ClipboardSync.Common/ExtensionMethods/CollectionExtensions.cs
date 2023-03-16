using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ClipboardSync.Commom.ExtensionMethods
{
    public enum DeleteDirection
    {
        Head,
        Tail
    }

    public static class CollectionExtensions
    {
        /// <summary>
        /// 往集合中插入元素，并保持集合中元素的数量不超过指定的最大值。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <param name="capacity">容量</param>
        /// <param name="direction">从集合的哪个方向删除元素</param>
        public static void InsertWithCapacityLimit<T>(
            this Collection<T> collection,
            int index,
            T item,
            int capacity,
            DeleteDirection direction = DeleteDirection.Tail)
        {
            collection.ApplyCapacityLimit(capacity-1, direction);
            collection.Insert(index, item);
        }

        /// <summary>
        /// 对集合应用容量限制，删除超出容量限制的元素。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="capacity">容量</param>
        /// <param name="direction">从集合的哪个方向删除元素</param>
        public static void ApplyCapacityLimit<T>(this Collection<T> collection, int capacity, DeleteDirection direction = DeleteDirection.Tail)
        {
            while (collection.Count > capacity)
            {
                if (direction == DeleteDirection.Tail)
                {
                    collection.RemoveAt(collection.Count - 1);
                }
                else
                {
                    collection.RemoveAt(0);
                }
            }
        }
    }
}
