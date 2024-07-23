using Discord.Interactions;
using JetBrains.Annotations;
using Zenject;

namespace SabaBot;

public abstract class InjectableInteractionModuleBase : InteractionModuleBase {
    [UsedImplicitly]
    public IServiceProvider ServiceProvider {
        set {
            var container = (DiContainer)value.GetService(typeof(DiContainer))!;
            container.Inject(this);
        }
    }
}