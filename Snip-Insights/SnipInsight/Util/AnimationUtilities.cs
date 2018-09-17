// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SnipInsight.Util
{
    public static class AnimationUtilities
    {
        #region Property Paths

        public static PropertyPath CanvasLeftPropertyPath = new PropertyPath(Canvas.LeftProperty);
        public static PropertyPath CanvasTopPropertyPath = new PropertyPath(Canvas.TopProperty);
        public static PropertyPath GradientStopColorPropertyPath = new PropertyPath(GradientStop.ColorProperty);
        public static PropertyPath HeightPropertyPath = new PropertyPath(FrameworkElement.HeightProperty);
        public static PropertyPath WidthPropertyPath = new PropertyPath(FrameworkElement.WidthProperty);
        public static PropertyPath OpacityPropertyPath = new PropertyPath(UIElement.OpacityProperty);
        public static PropertyPath SolidColorBrushColorPropertyPath = new PropertyPath(SolidColorBrush.ColorProperty);
        public static PropertyPath ScaleXPropertyPath = new PropertyPath(ScaleTransform.ScaleXProperty);
        public static PropertyPath ScaleYPropertyPath = new PropertyPath(ScaleTransform.ScaleYProperty);
        public static PropertyPath TranslateTransformXPropertyPath = new PropertyPath(TranslateTransform.XProperty);
        public static PropertyPath TranslateTransformYPropertyPath = new PropertyPath(TranslateTransform.YProperty);
        public static PropertyPath VisibilityPropertyPath = new PropertyPath(UIElement.VisibilityProperty);

        #endregion

        #region Color Animation

        public static ColorAnimation CreateColorAnimation(Nullable<Color> from, Nullable<Color> to)
        {
            ColorAnimation animation = new ColorAnimation();

            animation.From = from;
            animation.To = to;

            return animation;
        }

        public static ColorAnimation CreateColorAnimation(int durationInMilliseconds, Nullable<Color> from, Nullable<Color> to)
        {
            ColorAnimation animation = CreateColorAnimation(from, to);

            animation.Duration = CreateDuration(durationInMilliseconds);

            return animation;
        }

        public static ColorAnimation CreateColorAnimation(PropertyPath path, int durationInMilliseconds, Nullable<Color> from, Nullable<Color> to)
        {
            ColorAnimation animation = CreateColorAnimation(durationInMilliseconds, from, to);

            SetTargetProperty(animation, path);

            return animation;
        }

        public static ColorAnimation CreateColorAnimation(DependencyObject element, PropertyPath path, int durationInMilliseconds, Nullable<Color> from, Nullable<Color> to)
        {
            ColorAnimation animation = CreateColorAnimation(durationInMilliseconds, from, to);

            SetTargetProperty(animation, element, path);

            return animation;
        }

        #endregion

        #region Double Animation

        public static DoubleAnimation CreateDoubleAnimation(Nullable<double> from, Nullable<double> to)
        {
            DoubleAnimation animation = new DoubleAnimation();

            animation.From = from;
            animation.To = to;

            return animation;
        }

        public static DoubleAnimation CreateDoubleAnimation(int durationInMilliseconds, Nullable<double> from, Nullable<double> to)
        {
            DoubleAnimation animation = CreateDoubleAnimation(from, to);

            animation.Duration = CreateDuration(durationInMilliseconds);

            return animation;
        }

        public static DoubleAnimation CreateDoubleAnimation(PropertyPath path, int durationInMilliseconds, Nullable<double> from, Nullable<double> to)
        {
            DoubleAnimation animation = CreateDoubleAnimation(durationInMilliseconds, from, to);

            SetTargetProperty(animation, path);

            return animation;
        }

        public static DoubleAnimation CreateDoubleAnimation(DependencyObject element, PropertyPath path, int durationInMilliseconds, Nullable<double> from, Nullable<double> to)
        {
            DoubleAnimation animation = CreateDoubleAnimation(durationInMilliseconds, from, to);

            SetTargetProperty(animation, element, path);

            return animation;
        }

        #endregion

        #region Opacity Animation

        public static DoubleAnimation CreateOpacityAnimation(int durationInMilliseconds, Nullable<double> from, Nullable<double> to)
        {
            return CreateDoubleAnimation(OpacityPropertyPath, durationInMilliseconds, from, to);
        }

        public static DoubleAnimation CreateOpacityAnimation(DependencyObject element, int durationInMilliseconds, Nullable<double> from, Nullable<double> to)
        {
            return CreateDoubleAnimation(element, OpacityPropertyPath, durationInMilliseconds, from, to);
        }

        #endregion

        #region Fade In/Out Animation

        public static DoubleAnimation CreateFadeInAnimation(int durationInMilliseconds)
        {
            return CreateOpacityAnimation(durationInMilliseconds, null, 1);
        }

        public static DoubleAnimation CreateFadeInAnimation(DependencyObject element, int durationInMilliseconds)
        {
            return CreateOpacityAnimation(element, durationInMilliseconds, null, 1);
        }

        public static Storyboard CreateFadeInStoryboard(DependencyObject element, int durationInMilliseconds)
        {
            return CreateStoryboard(CreateFadeInAnimation(element, durationInMilliseconds));
        }

        public static DoubleAnimation CreateFadeOutAnimation(int durationInMilliseconds)
        {
            return CreateOpacityAnimation(durationInMilliseconds, null, 0);
        }

        public static DoubleAnimation CreateFadeOutAnimation(DependencyObject element, int durationInMilliseconds)
        {
            return CreateOpacityAnimation(element, durationInMilliseconds, null, 0);
        }

        private static Storyboard CreateFadeOutStoryboard(UIElement element, int durationInMilliseconds)
        {
            return CreateFadeOutStoryboard(element, durationInMilliseconds, false, false);
        }

        private static Storyboard CreateFadeOutAndHideStoryboard(UIElement element, int durationInMilliseconds)
        {
            return CreateFadeOutStoryboard(element, durationInMilliseconds, true, false);
        }

        private static Storyboard CreateFadeOutAndRemoveStoryboard(UIElement element, int durationInMilliseconds)
        {
            return CreateFadeOutStoryboard(element, durationInMilliseconds, false, true);
        }

        private static Storyboard CreateFadeOutStoryboard(UIElement element, int durationInMilliseconds, bool hideOnComplete, bool removeOnComplete)
        {
            Storyboard storyboard = CreateStoryboard(CreateFadeOutAnimation(element, durationInMilliseconds));

            if (removeOnComplete)
            {
                AddRemoveOnComplete(storyboard, element);
            }
            else if (hideOnComplete)
            {
                AddHideOnComplete(storyboard, element);
            }

            return storyboard;
        }

        #endregion

        #region Fade In/Out

        public static Storyboard FadeIn(this UIElement element, int durationInMilliseconds)
        {
            Storyboard storyboard = CreateStoryboard(CreateFadeInAnimation(element, durationInMilliseconds));

            element.Visibility = Visibility.Visible;

            storyboard.Begin();

            return storyboard;
        }

        public static Storyboard FadeOut(this UIElement element, int durationInMilliseconds)
        {
            return FadeOut(element, durationInMilliseconds, false, false);
        }

        public static Storyboard FadeOutAndHide(this UIElement element, int durationInMilliseconds)
        {
            return FadeOut(element, durationInMilliseconds, true, false);
        }

        public static Storyboard FadeOutAndRemove(this UIElement element, int durationInMilliseconds)
        {
            return FadeOut(element, durationInMilliseconds, false, true);
        }

        private static Storyboard FadeOut(UIElement element, int durationInMilliseconds, bool hideOnComplete, bool removeOnComplete)
        {
            Storyboard storyboard = CreateFadeOutStoryboard(element, durationInMilliseconds, hideOnComplete, removeOnComplete);

            storyboard.Begin();

            return storyboard;
        }

        #endregion

        #region Storyboard

        public static Storyboard CreateStoryboard(AnimationTimeline timeline)
        {
            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(timeline);

            return storyboard;
        }

        #endregion

        #region Storyboard Targets

        public static void SetTargetProperty(AnimationTimeline timeline, PropertyPath path)
        {
            Storyboard.SetTargetProperty(timeline, path);
        }

        public static void SetTarget(AnimationTimeline timeline, DependencyObject element)
        {
            Storyboard.SetTarget(timeline, element);
        }

        public static void SetTargetProperty(AnimationTimeline timeline, DependencyObject element, PropertyPath path)
        {
            SetTarget(timeline, element);
            SetTargetProperty(timeline, path);
        }

        #endregion

        #region Hide and Remove

        public static void AddHideOnComplete(Storyboard storyboard, UIElement element)
        {
            storyboard.Completed += (s, e) =>
            {
                Hide(element);
            };
        }

        public static void AddRemoveOnComplete(Storyboard storyboard, UIElement element)
        {
            storyboard.Completed += (s, e) =>
                {
                    Hide(element);
                    RemoveFromParent(element);
                };
        }

        private static void Hide(UIElement element)
        {
            element.Visibility = Visibility.Collapsed;
        }

        private static void RemoveFromParent(UIElement element)
        {
            FrameworkElement frameworkElement = element as FrameworkElement;

            if (frameworkElement != null)
            {
                if (frameworkElement.Parent != null && frameworkElement.Parent is Panel)
                {
                    ((Panel)frameworkElement.Parent).Children.Remove(element);
                }
            }
        }

        #endregion

        #region Duration

        public static Duration CreateDuration(int milliseconds)
        {
            return new Duration(TimeSpan.FromMilliseconds(milliseconds));
        }

        public static KeyTime CreateKeyTime(int milliseconds)
        {
            return KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(milliseconds));
        }

        #endregion
    }
}
