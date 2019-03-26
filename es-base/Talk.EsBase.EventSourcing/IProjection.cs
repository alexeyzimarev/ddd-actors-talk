using System.Threading.Tasks;

namespace Talk.EsBase.EventSourcing
{
    public interface IProjection
    {
        Task Project(object @event);
    }
}