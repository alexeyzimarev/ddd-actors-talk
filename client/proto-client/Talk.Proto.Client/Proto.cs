using System;
using System.Threading.Tasks;
using Proto;
using Proto.Cluster;
using Proto.Remote;

namespace Talk.Proto.Client
{
    public static class ProtoCluster
    {
        public static async Task<PID> GetActor(string prefix, string id)
        {
            var actorName = $"{prefix}-{id}";
            var (pid, sc) = await GetActor();
            while (sc != ResponseStatusCode.OK)
            {
                await Task.Delay(100);
                await Console.Out.WriteLineAsync($"Warning, can't get a remote actor {sc}");
                (pid, sc) = await GetActor();
            }

            return pid;

            Task<(PID, ResponseStatusCode)> GetActor() => Cluster.GetAsync(actorName, prefix);
        }
    }
}