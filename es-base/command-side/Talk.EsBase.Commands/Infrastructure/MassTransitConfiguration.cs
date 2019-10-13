using System;
using GreenPipes;
using MassTransit;

namespace Talk.EsBase.Commands.Infrastructure
{
    public static class MassTransitConfiguration
    {
        public static IBusControl ConfigureBus(
            string hostUri,
            string userName,
            string password,
            params (string queue, Action<IReceiveEndpointConfigurator> config)[] endpoints
        )
            => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.UseMessageRetry(r => r.Immediate(10));

                var host = cfg.Host(new Uri(hostUri), h =>
                {
                    h.Username(userName);
                    h.Password(password);
                });
                foreach (var (queue, config) in endpoints)
                    cfg.ReceiveEndpoint(host, queue,
                        e =>
                        {
                            config(e);
                            e.PrefetchCount = 60;
                            e.UseConcurrencyLimit(20);
                        });
            });
    }
}