using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Talk.EsBase.Queries.Infrastructure.RavenDb;
using static Talk.Messages.Vehicle.Events;
using static Talk.EsBase.Queries.Modules.Projections.ReadModels;

namespace Talk.EsBase.Queries.Modules.Projections
{
    public static class VehicleItemProjection
    {
        public static Func<Task> GetHandler(
            IAsyncDocumentSession session,
            object @event)
        {
            return @event switch
                {
                    VehicleRegistered e =>
                        () =>
                            session.StoreAsync(
                                new VehicleItem
                                {
                                    Id = GetDbId(e.VehicleId),
                                    Registration = e.Registration,
                                    MakeModel = e.MakeModel,
                                    MaxSpeed = e.MaxSpeed,
                                    MaxTemperature = e.MaxTemperature,
                                    State = e.State,
                                    Sensors = new List<VehicleItem.VehicleSensor>()
                                }
                            ),
                    VehicleMaxSpeedAdjusted e =>
                        () => Update(e.VehicleId, x => x.MaxSpeed = e.MaxSpeed),
                    VehicleMaxTemperatureAdjusted e =>
                        () => Update(e.VehicleId, x => x.MaxTemperature = e.MaxTemperature),
                    VehicleSpeeingDetected e =>
                        () => Update(e.VehicleId, x => x.State = "Speeding"),
                    VehicleOverheated e =>
                        () => Update(e.VehicleId, x => x.State = "Overheated"),
                    Messages.Sensor.Events.SensorTelemetryReceived e =>
                        () => UpdateOneSensor(
                            e.VehicleId,
                            e.SensorId,
                            sensor =>
                                {
                                    sensor.Speed = e.Speed;
                                    sensor.Temperature = e.Temperature;
                                }
                            ),
                    _ => (Func<Task>) null
                };

            Task Update(
                string id,
                Action<VehicleItem> update)
                => session.UpdateItem(
                    GetDbId(id),
                    update
                );

            Task UpdateOneSensor(
                string id,
                string sensorId,
                Action<VehicleItem.VehicleSensor> update)
                => Update(
                    id,
                    vehicleItem =>
                    {
                        var sensor = vehicleItem.Sensors
                            .FirstOrDefault(x => x.SensorId == sensorId);
                        if (sensor != null) update(sensor);
                    }
                );

            string GetDbId(string vehicleId)
                => VehicleItem.GetDatabaseId(vehicleId);
        }
    }
}