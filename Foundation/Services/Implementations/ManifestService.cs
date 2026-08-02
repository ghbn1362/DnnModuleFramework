using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetNuke.Modules.Foundation.Manifest;
using DotNetNuke.Modules.Foundation.Services.Contracts;

namespace DotNetNuke.Modules.Foundation.Services.Implementations
{
    public class ManifestService : IManifestService
    {
        public async Task<ManifestDocument> ReadManifestAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            using var stream = File.OpenRead(path);
            return await ReadManifestFromStreamAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ManifestDocument> ReadManifestFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // XmlManifestReader currently provides sync API; run parsing on thread-pool to avoid blocking caller.
            // Later we can refactor XmlManifestReader to fully async.
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var reader = new XmlManifestReader(stream, isDashboard: false);
                var doc = reader.Load();
                return doc;
            }, cancellationToken).ConfigureAwait(false);
        }

        public bool IsValid(ManifestDocument document)
        {
            if (document == null) return false;
            // Basic validation: at least one element type exists or extend with schema/validation logic.
            return (document.Scripts?.Any() ?? false)
                || (document.StyleSheets?.Any() ?? false)
                || (document.Tokens?.Any() ?? false);
        }
    }
}
