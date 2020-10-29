namespace PrismContextAware.Services.Contracts
{
    public interface IWindowAwareStatusInjectionAware
    {
        void InitialiseViewAwareService(IWindowAwareStatus window);
    }
}
