using HexArch.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using Microsoft.Extensions.DependencyInjection;

namespace HexArch.EventDispatching
{
    public sealed class ServiceProviderEventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider serviceProvider;

        public ServiceProviderEventDispatcher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task Dispatch<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent
        {
            foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
            {
                await handler.Handle(domainEvent, cancellationToken);
            }
        }
    }
}
