using System.Windows.Input;

namespace SnipInsight.Forms.Features.Insights
{
    public interface IHideable
    {
        bool IsVisible { get; }

        ICommand ToggleVisibilityCommand { get; }
    }
}
