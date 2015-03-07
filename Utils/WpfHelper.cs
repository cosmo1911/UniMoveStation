using System.Windows;

namespace UniMoveStation.Utils
{
    public class WpfHelper
    {
        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/bb613579.aspx
        /// </summary>
        /// <typeparam name="TChildItem"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TChildItem FindVisualChild<TChildItem>(DependencyObject obj) where TChildItem : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is TChildItem)
                    return (TChildItem)child;
                TChildItem childOfChild = FindVisualChild<TChildItem>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
