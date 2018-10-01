// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Package;
using SnipInsight.Util;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SnipInsight.Views
{
    /// <summary>
    /// Interaction logic for LibraryPanel.xaml
    /// </summary>
    public partial class LibraryPanel : UserControl
    {
        public LibraryPanel()
        {
            InitializeComponent();
        }

        #region PackagesSource

        public static readonly DependencyProperty PackagesSourceProperty =
            DependencyProperty.Register("PackagesSource", typeof(ObservableCollection<SnipInsightLink>), typeof(LibraryPanel), new PropertyMetadata(null, OnPackagesSourceChangedStatic));

        public ObservableCollection<SnipInsightLink> PackagesSource
        {
            get { return GetValue(PackagesSourceProperty) as ObservableCollection<SnipInsightLink>; }
            set { SetValue(PackagesSourceProperty, value); }
        }

        protected virtual void OnPackagesSourceChanged(ObservableCollection<SnipInsightLink> packagesSource)
        {
            BuildPackagesCollectionView(packagesSource);
        }

        private static void OnPackagesSourceChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as LibraryPanel;

            if (self != null)
            {
                self.OnPackagesSourceChanged(e.NewValue as ObservableCollection<SnipInsightLink>);
            }
        }

        #endregion

        #region Packages View

        private ListCollectionView _packagesView;

        private void BuildPackagesCollectionView(ObservableCollection<SnipInsightLink> packagesSource)
        {
            if (packagesSource == null)
            {
                LibraryListView.ItemsSource = null;
                return;
            }

            LibraryListView.ItemsSource = packagesSource;

            ListCollectionView view = CollectionViewSource.GetDefaultView(LibraryListView.ItemsSource) as ListCollectionView;

            // Sorting
            view.CustomSort = new SnipInsightLinkTimeSorter();

            // Filtering
            ApplyFilters(view, FilterSinceDate);

            // Grouping
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription("TimeGroupingLabel"));

            _packagesView = view;

            //LibraryListView.ItemsSource = _packagesView;
        }

        #endregion

        #region Filters

        public static readonly DependencyProperty FilterSinceDateProperty =
            DependencyProperty.Register("FilterSinceDate", typeof(Nullable<DateTime>), typeof(LibraryPanel), new PropertyMetadata(null, OnFilterSinceDateChangedStatic));

        public Nullable<DateTime> FilterSinceDate
        {
            get { return (Nullable<DateTime>)GetValue(FilterSinceDateProperty); }
            set { SetValue(FilterSinceDateProperty, value); }
        }

        protected virtual void OnFilterSinceDateChanged(Nullable<DateTime> sinceDate)
        {
            ApplyFilters(_packagesView, sinceDate);
        }

        private static void OnFilterSinceDateChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as LibraryPanel;

            if (self != null)
            {
                self.OnFilterSinceDateChanged((Nullable<DateTime>)e.NewValue);
            }
        }

        private void ApplyFilters(ListCollectionView view, Nullable<DateTime> dateTime)
        {
            if (view != null)
            {
                if (dateTime.HasValue)
                {
                    DateTime filterValue = dateTime.Value;

                    view.Filter = (x) => (x as SnipInsightLink).LastWriteTime >= filterValue;
                }
                else
                {
                    view.Filter = null;
                }
            }
        }

        #endregion

        #region Delete

        private bool _isDeleting;

        private async void DeleteButton_OnLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var lb = sender as FrameworkElement;

                if (lb == null)
                {
                    return;
                }

                var model = lb.DataContext as SnipInsightLink;

                if (model == null || model.DeletionPending)
                {
                    return;
                }

                await HandleDelete(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail("There was an exception when calling DeleteButton_OnLeftMouseDown. Ex Message = ", ex.ToString());
                Diagnostics.LogException(ex);
            }
        }

        private async Task HandleDelete(SnipInsightLink item)
        {
            if (item != null)
            {
                await HandleDelete(new SnipInsightLink[] { item });
            }
        }

        private async Task HandleDelete(IEnumerable<SnipInsightLink> items)
        {
            try
            {
                if (_isDeleting)
                {
                    // Prevent re-entry
                    return;
                }

                _isDeleting = true;
                await AppManager.TheBoss.DeleteAsync(items, true, true);
            }
            finally
            {
                _isDeleting = false;
            }
        }

        #endregion

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                OpenSelectedPackage();
                e.Handled = true;
            }
        }

        private void OpenSelectedPackage()
        {
            var item = LibraryListView.SelectedItem as SnipInsightLink;

            if (item != null)
            {
                AppManager.TheBoss.ViewModel.SelectedPackage = item;
                AppManager.TheBoss.ViewModel.RestoreImageUrl = string.Empty;
            }

            AppManager.TheBoss.ViewModel.ToggleEditorCommand.Execute(null);
        }

        private void LibraryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppManager.TheBoss.ViewModel.SelectedLibraryItemsList = LibraryListView.SelectedItems;
        }

        private void LibraryListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                OpenSelectedPackage();
                e.Handled = true;
            }
        }

        internal void SetInitialFocus()
        {
            LibraryListView.Focus();
        }
    }
}
