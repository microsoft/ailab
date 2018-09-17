using System;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights.Celebrities
{
    public partial class CelebritiesView : ContentView
    {
        public CelebritiesView()
        {
            this.InitializeComponent();
        }

        public void ToggleCelebrityInformationVisibility()
        {
            this.celebrityInformationSection.IsVisible = !this.celebrityInformationSection.IsVisible;
        }

        public void ToggleCelebrityNewsVisibility()
        {
            this.celebrityNewsSection.IsVisible = !this.celebrityNewsSection.IsVisible;
        }
    }
}
