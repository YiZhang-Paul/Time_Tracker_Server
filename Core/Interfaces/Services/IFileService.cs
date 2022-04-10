using System.IO;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IFileService
    {
        Task<bool> UploadAvatar(long userId, Stream avatar);
    }
}
