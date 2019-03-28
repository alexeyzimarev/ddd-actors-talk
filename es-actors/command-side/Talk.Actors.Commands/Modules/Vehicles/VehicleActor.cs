using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Proto;
using Proto.Persistence;
using Talk.Domain.Vehicle;
using Talk.Proto.Messages;

namespace Talk.Actors.Commands.Modules.Vehicles
{
    public class VehicleActor : IActor
    {
        VehicleState State { get; set; }

        public VehicleActor(IEventStore provider)
        {
            State = new VehicleState();

            When<Started>(ctx =>
            {
                _persistence = Persistence.WithEventSourcing(provider, ctx.Self.Id, ApplyEvent);
                return _persistence.RecoverStateAsync();
            });

            When<RegisterVehicle>(
                cmd =>
                    Vehicle.Register(
                        cmd.VehicleId,
                        cmd.CustomerId,
                        cmd.MakeModel,
                        cmd.Registration,
                        cmd.MaxSpeed,
                        cmd.MaxTemperature)
            );
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (!_handlers.TryGetValue(context.Message.GetType(), out var handler))
                return;

            await handler(context);

            if (context.Sender != null)
                context.Respond(new Ack());
        }

        void ApplyEvent(Event @event)
        {
            State = State.When(State, @event.Data);
        }

        async Task Apply(VehicleState.Result result)
        {
            foreach (var @event in result.Events)
                await _persistence.PersistEventAsync(@event);
        }

        void When<TCommand>(Func<IContext, Task> handler)
            where TCommand : class
            => _handlers.Add(typeof(TCommand), handler);

        void When<TCommand>(Func<TCommand, Task> handler)
            where TCommand : class
            => _handlers.Add(
                typeof(TCommand),
                ctx => handler(ctx.Message as TCommand));

        void When<TCommand>(Func<TCommand, VehicleState.Result> handler)
            where TCommand : class
            => _handlers.Add(
                typeof(TCommand),
                ctx => Apply(handler(ctx.Message as TCommand))
            );

        readonly Dictionary<Type, Func<IContext, Task>> _handlers =
            new Dictionary<Type, Func<IContext, Task>>();

        Persistence _persistence;
    }
}