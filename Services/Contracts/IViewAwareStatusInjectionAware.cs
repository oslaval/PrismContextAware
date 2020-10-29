namespace PrismContextAware.Services.Contracts
{
    public interface IViewAwareStatusInjectionAware
    {
        void InitialiseViewAwareService(IViewAwareStatus view);
    }
}
