using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Gtk;
using SnipInsights.Forms.GTK;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Controls;
using Xamarin.Forms.Platform.GTK.Extensions;
using Xamarin.Forms.Platform.GTK.Renderers;
using Xamarin.Forms.PlatformConfiguration.GTKSpecific;

[assembly: ExportRenderer(typeof(Xamarin.Forms.TabbedPage), typeof(TabbedPageRendererFix))]

namespace SnipInsights.Forms.GTK
{
    public class TabbedPageRendererFix : AbstractPageRenderer<NotebookWrapper, Xamarin.Forms.TabbedPage>
    {
        private const int DefaultIconWidth = 24;
        private const int DefaultIconHeight = 24;

        protected override void OnElementChanged(Xamarin.Forms.Platform.GTK.VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                this.Page.ChildAdded -= this.OnPageAdded;
                this.Page.ChildRemoved -= this.OnPageRemoved;
                this.Page.PagesChanged -= this.OnPagesChanged;
            }

            if (e.NewElement != null)
            {
                var newPage = e.NewElement as Xamarin.Forms.TabbedPage;

                if (newPage == null)
                {
                    throw new ArgumentException("Element must be a TabbedPage");
                }

                if (this.Widget == null)
                {
                    // Custom control using a tabbed notebook container.
                    this.Widget = new NotebookWrapper();
                    this.Control.Content = this.Widget;
                }

                this.Init();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(Xamarin.Forms.TabbedPage.CurrentPage) || e.PropertyName == nameof(Xamarin.Forms.TabbedPage.SelectedItem))
            {
                this.UpdateCurrentPage();
                this.UpdateBarTextColor();
                this.UpdateBarBackgroundColor();
            }
            else if (e.PropertyName == Xamarin.Forms.TabbedPage.BarTextColorProperty.PropertyName)
            {
                this.UpdateBarTextColor();
            }
            else if (e.PropertyName == Xamarin.Forms.TabbedPage.BarBackgroundColorProperty.PropertyName)
            {
                this.UpdateBarBackgroundColor();
            }
            else if (e.PropertyName ==
                  Xamarin.Forms.PlatformConfiguration.GTKSpecific.TabbedPage.TabPositionProperty.PropertyName)
            {
                this.UpdateTabPos();
            }
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            this.Control?.Content?.SetSize(allocation.Width, allocation.Height);
        }

        protected override void UpdateBackgroundImage()
        {
            this.Widget?.SetBackgroundImage(this.Page.BackgroundImage);
        }

        protected override void Dispose(bool disposing)
        {
            this.Page.PagesChanged -= this.OnPagesChanged;
            this.Page.ChildAdded -= this.OnPageAdded;
            this.Page.ChildRemoved -= this.OnPageRemoved;

            if (this.Widget != null)
            {
                this.Widget.NoteBook.SwitchPage -= this.OnNotebookPageSwitched;
            }

            base.Dispose(disposing);
        }

        private void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Widget.NoteBook.SwitchPage -= this.OnNotebookPageSwitched;

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.ResetPages();
            }

            this.UpdateChildrenOrderIndex();
            this.UpdateCurrentPage();

            this.Widget.NoteBook.SwitchPage += this.OnNotebookPageSwitched;
        }

        private void OnPageAdded(object sender, ElementEventArgs e)
        {
            this.InsertPage(e.Element as Xamarin.Forms.Page, this.Page.Children.IndexOf(e.Element));
        }

        private void OnPageRemoved(object sender, ElementEventArgs e)
        {
            this.RemovePage(e.Element as Xamarin.Forms.Page);
        }

        private void InsertPage(Xamarin.Forms.Page page, int index)
        {
            var pageRenderer = Platform.GetRenderer(page);

            if (pageRenderer == null)
            {
                pageRenderer = Platform.CreateRenderer(page);
                Platform.SetRenderer(page, pageRenderer);
            }

            this.Widget.InsertPage(
                pageRenderer.Container,
                page.Title,
                page.Icon?.ToPixbuf(new Size(DefaultIconWidth, DefaultIconHeight)),
                index);

            this.Widget.ShowAll();

            page.PropertyChanged += this.OnPagePropertyChanged;
        }

        private void RemovePage(Xamarin.Forms.Page page)
        {
            page.PropertyChanged -= this.OnPagePropertyChanged;

            var pageRenderer = Platform.GetRenderer(page);

            if (pageRenderer != null)
            {
                this.Widget.RemovePage(pageRenderer.Container);
            }

            Platform.SetRenderer(page, null);
        }

        private void ResetPages()
        {
            foreach (var page in this.Page.Children)
            {
                this.RemovePage(page);
            }

            this.Widget.RemoveAllPages();

            var i = 0;
            foreach (var page in this.Page.Children)
            {
                this.InsertPage(page, i++);
            }
        }

        private void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Xamarin.Forms.Page.TitleProperty.PropertyName)
            {
                var page = (Xamarin.Forms.Page)sender;
                var index = Xamarin.Forms.TabbedPage.GetIndex(page);

                this.Widget.SetTabLabelText(index, page.Title);
            }
            else if (e.PropertyName == Xamarin.Forms.Page.IconProperty.PropertyName)
            {
                var page = (Xamarin.Forms.Page)sender;
                var index = Xamarin.Forms.TabbedPage.GetIndex(page);
                var icon = page.Icon;

                this.Widget.SetTabIcon(index, icon.ToPixbuf());
            }
            else if (e.PropertyName == Xamarin.Forms.TabbedPage.BarBackgroundColorProperty.PropertyName)
            {
                this.UpdateBarBackgroundColor();
            }
            else if (e.PropertyName == Xamarin.Forms.TabbedPage.BarTextColorProperty.PropertyName)
            {
                this.UpdateBarTextColor();
            }
        }

        private void UpdateCurrentPage()
        {
            Xamarin.Forms.Page page = this.Page.CurrentPage;

            if (page == null)
            {
                return;
            }

            int selectedIndex = 0;
            if (this.Page.SelectedItem != null)
            {
                for (var i = 0; i < this.Page.Children.Count - 1; i++)
                {
                    if (this.Page.Children[i].BindingContext?.Equals(this.Page.SelectedItem) == true)
                    {
                        break;
                    }

                    selectedIndex++;
                }
            }

            this.Widget.NoteBook.CurrentPage = selectedIndex;
            this.Widget.NoteBook.ShowAll();
        }

        private void UpdateChildrenOrderIndex()
        {
            for (var i = 0; i < this.Page.Children.Count; i++)
            {
                var page = this.PageController.InternalChildren[i];

                Xamarin.Forms.TabbedPage.SetIndex(page as Xamarin.Forms.Page, i);
            }
        }

        private void UpdateBarBackgroundColor()
        {
            if (this.Element == null || this.Page.BarBackgroundColor.IsDefault)
            {
                return;
            }

            var barBackgroundColor = this.Page.BarBackgroundColor.ToGtkColor();

            for (var i = 0; i < this.Page.Children.Count; i++)
            {
                this.Widget.SetTabBackgroundColor(i, barBackgroundColor);
            }
        }

        private void UpdateBarTextColor()
        {
            if (this.Element == null || this.Page.BarTextColor.IsDefault)
            {
                return;
            }

            var barTextColor = this.Page.BarTextColor.ToGtkColor();

            for (var i = 0; i < this.Page.Children.Count; i++)
            {
                this.Widget.SetTabTextColor(i, barTextColor);
            }
        }

        private void UpdateTabPos() // Platform-Specific Functionality
        {
            var tabposition = this.Page.OnThisPlatform().GetTabPosition();

            switch (tabposition)
            {
                case TabPosition.Top:
                    this.Widget.NoteBook.TabPos = PositionType.Top;
                    break;
                case TabPosition.Bottom:
                    this.Widget.NoteBook.TabPos = PositionType.Bottom;
                    break;
            }
        }

        private void OnNotebookPageSwitched(object o, SwitchPageArgs args)
        {
            var currentPageIndex = (int)args.PageNum;
            VisualElement currentSelectedChild = this.Page.Children.Count > currentPageIndex
                ? this.Page.Children[currentPageIndex]
                : null;

            if (currentSelectedChild != null)
            {
                this.ElementController.SetValueFromRenderer(Xamarin.Forms.TabbedPage.SelectedItemProperty, currentSelectedChild.BindingContext);

                var pageRenderer = Platform.GetRenderer(currentSelectedChild);
                pageRenderer?.Container.ShowAll();
            }
        }

        private void Init()
        {
            this.OnPagesChanged(this.Page.Children, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            this.Page.ChildAdded += this.OnPageAdded;
            this.Page.ChildRemoved += this.OnPageRemoved;
            this.Page.PagesChanged += this.OnPagesChanged;

            this.UpdateCurrentPage();
            this.UpdateBarBackgroundColor();
            this.UpdateBarTextColor();
            this.UpdateTabPos();
            this.UpdateBackgroundImage();
        }
    }
}