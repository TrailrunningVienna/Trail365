namespace Trail365.Data
{
    public interface IDependencyTracker
    {
        string OperationType(bool cached);

        string OperationTarget { get; }
    }
}
