using System.Windows;
using System.Windows.Media;

namespace SequentialSelector.Views.Utils
{
    // https://github.com/atomatiq/OptionsBar/blob/master/OptionsBar/Views/Utils/VisualUtils.cs
    public static class VisualUtils
    {
        [CanBeNull]
        public static T FindVisualParent<T>(FrameworkElement element, string name) where T : FrameworkElement
        {
            FrameworkElement parentElement = (FrameworkElement)VisualTreeHelper.GetParent(element);
            while (parentElement != null)
            {
                if (parentElement is T parent)
                    if (parentElement.Name == name)
                        return parent;

                parentElement = (FrameworkElement)VisualTreeHelper.GetParent(parentElement);
            }

            return null;
        }

        public static T FindVisualChild<T>(FrameworkElement element, string name) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                FrameworkElement childElement = (FrameworkElement)VisualTreeHelper.GetChild(element, i);
                if (childElement is T child)
                    if (childElement.Name == name)
                        return child;

                T descendent = FindVisualChild<T>(childElement, name);
                if (descendent != null) return descendent;
            }

            return null;
        }
    }
}