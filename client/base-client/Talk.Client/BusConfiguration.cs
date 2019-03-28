using System;
using MassTransit;

namespace Talk.Client
{
    public static class BusConfiguration
    {
        public static IBusControl ConfigureMassTransit()
            =>
                Bus.Factory.CreateUsingRabbitMq(
                    cfg =>
                        cfg.Host(new Uri("rabbitmq://localhost"), h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        }));
    }
}