using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Test
{
    public class ControllerTestUtility
    {
        public async Task<HttpClient> SetupTestHttpClient(Action<IServiceCollection> serviceRegister = null)
        {
            var builder = Program.CreateHostBuilder(new string[0]).ConfigureWebHost(_ => _.UseTestServer());

            if (serviceRegister != null)
            {
                builder = builder.ConfigureServices((_, services) => serviceRegister(services));
            }

            var host = await builder.StartAsync().ConfigureAwait(false);

            return host.GetTestClient();
        }
    }
}
