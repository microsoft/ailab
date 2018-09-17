// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Util;
using SnipInsight.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SnipInsight.Views
{
	// Interaction logic for Acetate Layer
	public partial class AcetateLayer
    {
        Storyboard _animateAnts;

        public AcetateLayer()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            InkCanvas.StrokeCollected += InkCanvasOnStrokeCollected;
            InkCanvas.StrokeErased += InkCanvasOnStrokeErased;
        }

        private void InkCanvasOnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs inkCanvasStrokeCollectedEventArgs)
        {
            SnipInsightViewModel model = DataContext as SnipInsightViewModel;
            if (model != null)
            {
                model.HasInk = true;
            }
        }

        private void InkCanvasOnStrokeErased(object sender, RoutedEventArgs routedEventArgs)
        {
            SnipInsightViewModel model = DataContext as SnipInsightViewModel;
            if (model != null)
            {
                model.HasInk = HasInk();
            }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            _animateAnts = (Storyboard)this.TryFindResource("animateAnts");
            if (_animateAnts != null)
            {
                _animateAnts.Begin();
                _animateAnts.Pause();
            }

            SnipInsightViewModel model = DataContext as SnipInsightViewModel;
            if (model != null)
            {
                model.PropertyChanged += ViewModelOnPropertyChanged;
            }
        }

        void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                SnipInsightViewModel model = DataContext as SnipInsightViewModel;
                if (model == null)
                {
                    return;
                }

                switch (e.PropertyName)
                {
                    case "Mode":
                        {
                            if (_animateAnts != null)
                            {
                                switch (model.Mode)
                                {
                                    case Mode.Recording:
                                        _animateAnts.Resume();
                                        break;
                                    default:
                                        _animateAnts.Pause();
                                        break;
                                }
                            }
                        }
                        break;
                    case "InkModeRequested":
                        InkCanvas.EditingMode = model.InkModeRequested;
                        break;
                }
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
        }

        internal bool HasInk()
        {
            return InkCanvas.HasInk();
        }
    }
}
