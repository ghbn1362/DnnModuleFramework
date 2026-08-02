using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNetNuke.Modules.Foundation.Manifest;

namespace DotNetNuke.Modules.Foundation.Services.Contracts
{
    public interface IManifestService
    {
        Task<ManifestDocument> ReadManifestAsync(string path, CancellationToken cancellationToken = default);
        Task<ManifestDocument> ReadManifestFromStreamAsync(Stream stream, CancellationToken cancellationToken = default);
        bool IsValid(ManifestDocument document);
    }
}
