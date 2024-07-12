using Microsoft.Extensions.DependencyInjection;

namespace SabaBot;

internal class ZenjectServiceScopeFactory(ZenjectServiceProvider serviceProvider) : IServiceScopeFactory {
    public IServiceScope CreateScope() {
        return new ZenjectServiceScope(serviceProvider);
    }
}