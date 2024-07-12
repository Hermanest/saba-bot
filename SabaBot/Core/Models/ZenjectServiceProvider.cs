using Zenject;

namespace SabaBot;

internal class ZenjectServiceProvider(DiContainer container) : IServiceProvider {
    public object? GetService(Type serviceType) {
        return container.Resolve(serviceType);
    }
}