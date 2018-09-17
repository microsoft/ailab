using System;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SnipInsight.Forms.Features.Localization
{
    [ContentProperty("Text")]
    public class LocalizeExtension : IMarkupExtension
    {
        private const string ResourceId = "SnipInsight.Forms.Features.Localization.Resources.resources";

        private static readonly Lazy<ResourceManager> ResourceManager = new Lazy<ResourceManager>(
            () => new ResourceManager(typeof(Resources)));

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Text == null)
            {
                return string.Empty;
            }

            var translation = ResourceManager.Value.GetString(this.Text);

            if (translation == null)
            {
                translation = $"MISSING TRANSLATION: {this.Text}";
            }

            return translation;
        }
    }
}
