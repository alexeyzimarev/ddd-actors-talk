using System.Threading.Tasks;

namespace Talk.EventSourcing
{
    public delegate Task EventHandler(object @event);
}