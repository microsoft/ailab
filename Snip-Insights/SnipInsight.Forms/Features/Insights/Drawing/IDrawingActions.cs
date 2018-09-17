namespace SnipInsight.Forms.Features.Insights.Drawing
{
    public interface IDrawingActions
    {
        void Undo();

        void Redo();

        void Save(string filePath);

        void Reset();
    }
}