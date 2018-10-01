// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using SnipInsight.Util;

namespace SnipInsight.Views
{
	/// <summary>
	/// Interaction logic for EditorWindowTourPanel.xaml
	/// </summary>
	public partial class EditorWindowTourPanel : UserControl
	{
		public event EventHandler Completed;

		public EditorWindowTourPanel()
		{
			InitializeComponent();
		}

		public void Start()
		{
			try
			{
				var s = (Storyboard)TryFindResource("ShowStoryboard") as Storyboard;

				if (s != null)
				{
					s.Begin();
				}

				Tip1.FadeIn();
				Tip1.AfterFadeIn += (sender, evt) => { Button1.Focus(); };
			}
			catch (Exception ex)
			{
				Diagnostics.LogException(ex);
				OnError();
			}
		}

		public void Stop()
		{
			BeginEndAnimation();
		}

		private void Button1_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Tip1.FadeOut();
				Tip2.AfterFadeIn += (s, evt) => { Button2.Focus(); };
				Tip2.FadeIn();
			}
			catch (Exception ex)
			{
				Diagnostics.LogException(ex);
				OnError();
			}
		}

		private void Button2_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Tip2.FadeOut();
				Tip3.AfterFadeIn += (s, evt) => { Button3.Focus(); };
				Tip3.FadeIn();
			}
			catch (Exception ex)
			{
				Diagnostics.LogException(ex);
				OnError();
			}
		}

		private void Button3_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Tip3.FadeOut();
				BeginEndAnimation();
			}
			catch (Exception ex)
			{
				Diagnostics.LogException(ex);
				OnError();
			}
		}

		public void BeginEndAnimation()
		{
			var s = (Storyboard)TryFindResource("HideStoryboard") as Storyboard;

			if (s != null)
			{
				s.Completed += EndAnimationCompleted;
				s.Begin();
			}
			else
			{
				EndAnimationCompleted(null, null);
			}
		}

		private void EndAnimationCompleted(object sender, EventArgs e)
		{
			Finish();
		}

		private void Finish()
		{
			// Become invisible
			Visibility = Visibility.Collapsed;

			RaiseCompleted();

			RemoveSelfFromTheVisualTree();
		}

		private void RaiseCompleted()
		{
			if (Completed != null)
			{
				Completed(this, EventArgs.Empty);
			}
		}

		private void RemoveSelfFromTheVisualTree()
		{
			// Remove from the parent Panel (if possible)
			if (Parent != null && Parent is Panel)
			{
				((Panel)Parent).Children.Remove(this);
			}
		}

		private void OnError()
		{
			// In case of error, it's essential that we exit properly (even if it's not graceful)!!!
			Finish();
		}
	}
}
