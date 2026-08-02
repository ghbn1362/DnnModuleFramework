using System.IO;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Modules.Foundation.Services.Implementations;
using NUnit.Framework;

namespace DotNetNuke.Modules.Foundation.Tests
{
    public class ManifestServiceTests
    {
        [Test]
        public async Task ReadManifestFromStreamAsync_ParsesScriptsAndStylesAndTokens()
        {
            var xml = @"<manifest>
                            <scripts>
                                <script enabled='true'>/scripts/app.js</script>
                            </scripts>
                            <stylesheets>
                                <stylesheet enabled='true'>/styles/site.css</stylesheet>
                            </stylesheets>
                            <tokens>
                                <token>MY_TOKEN</token>
                            </tokens>
                        </manifest>";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var svc = new ManifestService();

            var doc = await svc.ReadManifestFromStreamAsync(ms);

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.Scripts.Count > 0);
            Assert.IsTrue(doc.StyleSheets.Count > 0);
            Assert.IsTrue(doc.Tokens.Count > 0);
        }
    }
}
