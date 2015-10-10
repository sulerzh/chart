namespace Semantic.Reporting.Windows.Common.Internal
{
    public interface IUpdatable
    {
        IUpdatable Parent { get; }

        void Update();
    }
}
