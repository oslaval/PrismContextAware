namespace PrismContextAware.Services.Contracts
{
    public interface IWindowAwareStatusInjectionAware
    {
        void InitialiseWindowAwareService(IWindowAwareStatus window);
    }
}
