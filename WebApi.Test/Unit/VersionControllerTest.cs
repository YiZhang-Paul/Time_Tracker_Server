using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebApi.Test.Unit
{
    [TestFixture]
    public class VersionControllerTest
    {
        private const string ApiBase = "api/v1/version";
        private HttpClient HttpClient { get; set; }

        [SetUp]
        public async Task Setup()
        {
            HttpClient = await new ControllerTestUtility().SetupTestHttpClient().ConfigureAwait(false);
        }

        [Test]
        public async Task GetVersionShouldReturnSemanticVersionString()
        {
            var response = await HttpClient.GetAsync(ApiBase).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(Regex.IsMatch(result, @"^\d+\.\d+\.\d+$"));
        }
    }
}
