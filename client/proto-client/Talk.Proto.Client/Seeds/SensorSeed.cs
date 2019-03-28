using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Talk.Proto.Client.Seeds
{
    public static class SensorSeed
    {
        public static async Task<IEnumerable<Sensor>> Seed<T>(
            IEnumerable<VehicleSeed.Vehicle> vehicles,
            int sensorserVehicle,
            Func<Sensor, T> commandFactory,
            Func<T, Task> publisher)
        {
            var sensors =
                vehicles.SelectMany(
                    v => Enumerable.Range(1, sensorserVehicle)
                        .Select(s => new Sensor
                        {
                            VehicleId = v.VehicleId,
                            SensorId = $"{v.VehicleId}-{s:00}"
                        })
                ).ToList();
            await Task.WhenAll(
                sensors.Select(v => publisher(commandFactory(v)))
            );
            return sensors;
        }

        public class Sensor
        {
            public string SensorId { get; set; }
            public string VehicleId { get; set; }
        }
    }
}