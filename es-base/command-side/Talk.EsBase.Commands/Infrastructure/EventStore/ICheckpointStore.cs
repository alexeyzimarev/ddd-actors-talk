using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Talk.EsBase.Commands.Infrastructure.EventStore
{
    public interface ICheckpointStore
    {
        Task<Position?> GetCheckpoint();
        Task StoreCheckpoint(Position? checkpoint);
    }
}