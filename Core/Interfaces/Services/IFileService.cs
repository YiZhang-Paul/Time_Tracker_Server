using System.IO;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IFileService
    {
        Task<string> GetAvatarUrl(long userId);
        Task<bool> UploadAvatar(long userId, Stream avatar);
    }
}
