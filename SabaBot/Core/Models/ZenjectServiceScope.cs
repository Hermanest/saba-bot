using Microsoft.Extensions.DependencyInjection;

namespace SabaBot;

internal class ZenjectServiceScope(IServiceProvider provider) : IServiceScope {
    public IServiceProvider ServiceProvider => provider;
    
    public void Dispose() { }
}