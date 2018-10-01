// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SnipInsight.Views
{
    public static class ControlTreeIterator
    {
        //private static readonly MethodInfo UserControlContentPropertyGetMethod = typeof(UserControl).GetProperty("Content", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true);

        public static T GetAncestorOfType<T>(FrameworkElement control)
            where T : FrameworkElement
        {
            FrameworkElement parent = control.Parent as FrameworkElement;

            while (parent != null)
            {
                if (parent is T)
                    return (T)parent;

                parent = parent.Parent as FrameworkElement;
            }

            return null;
        }

        public static UIElementCollection GetChildren(Panel control)
        {
            return control.Children;
        }

        public static object GetContent(ContentControl control)
        {
            return control.Content;
        }

        public static object GetContent(ContentPresenter control)
        {
            return control.Content;
        }

        public static object GetContent(Popup control)
        {
            return control.Child;
        }

        public static object GetContent(UserControl control)
        {
            //return UserControlContentPropertyGetMethod.Invoke(control, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);

            //  TODO: Can't find a way to get to the children of a UserControl...
            return null;
        }

        public static IEnumerable<object> IterateChildren(object control)
        {
            if (!(control == null))
            {
                if (control is Panel)
                    return IterateChildren((Panel)control);
                else if (control is Canvas)
                    return IterateChildren((Canvas)control);
                else if (control is ContentControl)
                    return IterateChildren((ContentControl)control);
                else if (control is ContentPresenter)
                    return IterateChildren((ContentPresenter)control);
                else if (control is Border)
                    return IterateChildren(((Border)control).Child);
                else if (control is Popup)
                {
                    IEnumerable<object> children = IterateChildren((Popup)control);
                    return children;
                }
            }

            return null;
        }

        public static IEnumerable<object> IterateChildren(ContentControl control)
        {
            object content = GetContent(control);

            if (content != null)
                yield return content;
        }

        public static IEnumerable<object> IterateChildren(ContentPresenter control)
        {
            object content = GetContent(control);

            if (content != null)
                yield return content;
        }

        public static IEnumerable<object> IterateChildren(Panel control)
        {
            foreach (UIElement element in GetChildren(control))
            {
                yield return element;
            }
        }

        public static IEnumerable<object> IterateChildren(Popup control)
        {
            object content = GetContent(control);

            if (content != null)
                yield return content;
        }

        public static IEnumerable<object> IterateChildren(UserControl control)
        {
            object content = GetContent(control);

            if (content != null)
                yield return content;
        }

        public static IEnumerable<T> IterateChildrenOfType<T>(object control)
        {
            foreach (object item in IterateChildren(control))
            {
                if (item is T)
                    yield return (T)item;
            }
        }

        public static IEnumerable<object> IterateSelfAndDescendants(object self)
        {
            return IterateSelfAndDescendants(self, false);
        }

        public static IEnumerable<object> IterateSelfAndDescendants(object self, bool depthFirst)
        {
            if (self != null)
            {
                if (!depthFirst)
                    yield return self;

                foreach (object item in IterateSelfAndDescendants(IterateChildren(self), depthFirst))
                    yield return item;

                if (depthFirst)
                    yield return self;
            }
        }

        public static IEnumerable<object> IterateSelfAndDescendants(IEnumerable<object> items, bool depthFirst)
        {
            if (items != null)
            {
                foreach (object item in items)
                {
                    if (!depthFirst)
                        yield return item;

                    foreach (object child in IterateSelfAndDescendants(IterateChildren(item), depthFirst))
                        yield return child;

                    if (depthFirst)
                        yield return item;
                }
            }
        }

        public static IEnumerable<T> IterateSelfAndDescendantsOfType<T>(object self)
        {
            return IterateSelfAndDescendantsOfType<T>(self, false);
        }

        public static IEnumerable<T> IterateSelfAndDescendantsOfType<T>(object self, bool depthFirst)
        {
            foreach (object item in IterateSelfAndDescendants(self, depthFirst))
            {
                if (item is T)
                    yield return (T)item;
            }
        }

        public static bool SetFocusToFirstChild(object self)
        {
            foreach (object item in IterateSelfAndDescendants(self))
            {
                if (item is Control && ((Control)item).IsTabStop == true)
                {
                    ((Control)item).Focus();

                    return true;
                }
            }

            return false;
        }

        public static void Visit<T>(IEnumerable<T> items, Action<T> action)
        {
            foreach (T item in items)
            {
                action(item);
            }
        }

        public static void VisitSelfAndDescendants(object self, Action<object> action)
        {
            Visit(IterateSelfAndDescendants(self), action);
        }

        public static void VisitSelfAndDescendants(object self, bool depthFirst, Action<object> action)
        {
            Visit(IterateSelfAndDescendants(self, depthFirst), action);
        }

        public static void VisitSelfAndDescendants(IEnumerable<object> items, bool depthFirst, Action<object> action)
        {
            Visit(IterateSelfAndDescendants(items, depthFirst), action);
        }

        public static void VisitSelfAndDescendantsOfType<T>(object self, Action<T> action)
        {
            Visit(IterateSelfAndDescendantsOfType<T>(self), action);
        }

        public static void VisitSelfAndDescendantsOfType<T>(object self, bool depthFirst, Action<T> action)
        {
            Visit(IterateSelfAndDescendantsOfType<T>(self, depthFirst), action);
        }
    }
}
