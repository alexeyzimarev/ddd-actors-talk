using System.Threading.Tasks;
using Proto;
using Proto.Cluster;
using Proto.Remote;
using Talk.Proto.Messages.Events;

namespace Talk.Actors.Queries.Infrastructure.ProtoActor
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
                Serilog.Log.Warning($"Warning, can't get a remote actor {sc}");
                (pid, sc) = await GetActor();
            }

            return pid;

            Task<(PID, ResponseStatusCode)> GetActor()
                => Cluster.GetAsync(actorName, prefix);
        }

        public static async Task SendToActor<T>(string prefix, string id, T command)
        {
            var pid = await GetActor(prefix, id);
            await RootContext.Empty.RequestAsync<AckEvent>(pid, command);
        }
    }
}