using Core.Interfaces.Repositories;
using System.Threading.Tasks;

namespace Core.Interfaces.UnitOfWorks
{
    public interface IUserUnitOfWork
    {
        IUserProfileRepository UserProfile { get; }
        Task<bool> Save();
    }
}
