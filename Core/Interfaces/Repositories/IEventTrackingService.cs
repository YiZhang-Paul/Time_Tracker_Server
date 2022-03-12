using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventTrackingService
    {
        Task<bool> StartIdlingSession();
    }
}
