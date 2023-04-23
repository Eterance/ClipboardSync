using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace ClipboardSync.Client.Windows
{
    // https://www.jianshu.com/p/e86724da61af
    public static class FindChildControl
    {
        /// <summary>
        /// 通过某种类型查找子控件列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parentObj">某个父控件</param>
        /// <param name="typename">需要查找子控件类型</param>
        /// <returns>查找到的子控件列表</returns>
        public static List<T> GetChildObjects<T>(DependencyObject parentObj, Type typename) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(parentObj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(parentObj, i);

                if (child is T && (((T)child).GetType() == typename))
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child, typename));
            }
            return childList;
        }

        /// <summary>
        /// 通过名称来查找子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parentObj">需要查找的父控件</param>
        /// <param name="name">需要查找的控件名称</param>
        /// <returns></returns>
        public static T GetChildObject<T>(DependencyObject parentObj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            T grandChild = null;

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(parentObj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(parentObj, i);

                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)child;
                }
                else
                {
                    grandChild = GetChildObject<T>(child, name);
                    if (grandChild != null)
                        return grandChild;
                }
            }
            return null;
        }

        /// <summary>
        /// 按名称查找子控件列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parentObj">开始查找的父控件</param>
        /// <param name="name">需要查找控件名称</param>
        /// <returns></returns>
        public static List<T> GetChildObjects<T>(DependencyObject parentObj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(parentObj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(parentObj, i);

                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child, name));
            }
            return childList;
        }
    }
}
