using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using SnipInsight.Forms.Features.Insights;
using Xamarin.Forms;

namespace SnipInsight.Forms.Common
{
    public class HideableViewModel : BaseViewModel, IHideable
    {
        private bool isVisible = false;

        public HideableViewModel()
        {
            this.ToggleVisibilityCommand = new Command(this.OnToggleVisibility);
        }

        public bool IsVisible
        {
            get => this.isVisible;
            set => this.SetProperty(ref this.isVisible, value);
        }

        public ICommand ToggleVisibilityCommand { get; set; }

        private void OnToggleVisibility(object obj)
        {
            this.IsVisible = !this.IsVisible;
        }
    }
}