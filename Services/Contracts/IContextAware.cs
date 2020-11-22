namespace PrismContextAware.Services.Contracts
{
    /// <summary>
    /// Interface used for services that want to have the context injected
    /// </summary>
    public interface IContextAware
    {
        void InjectContext(object context);

        object? Context { get; }
    }
}