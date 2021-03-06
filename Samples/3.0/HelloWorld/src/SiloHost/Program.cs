using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HelloWorld.Grains;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace SiloHost
{
    public class Program
    {
        static string webRoot = "../../../wwwroot";

        public static Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .UseOrleans(siloBuilder =>
                {
                    siloBuilder
                        .UseSignalR(builder =>
                        {
                            builder
                                .Configure((innerSiloBuilder, config) =>
                                {
                                    innerSiloBuilder
                                        .AddMemoryGrainStorage(name: "ArchiveStorage")
                                        .AddMemoryGrainStorage("PubSubStore")
                                        .UseLocalhostClustering()
                                        .Configure<ClusterOptions>(options =>
                                        {
                                            options.ClusterId = "dev";
                                            options.ServiceId = "HelloWorldApp";
                                        })
                                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences());

                                });
                        });
                })
                // Install-Package Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv -Version 3.1.0
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .UseUrls("http://localhost:5000")
                        .UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
                        .UseWebRoot(webRoot)
                        .UseStartup<WebStartupHandler>();
                })
                .ConfigureServices(services =>
                {
                    services.Configure<ConsoleLifetimeOptions>(options =>
                    {
                        options.SuppressStatusMessages = true;
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    //logging.AddDebug(); // uncomment if you want to see everything in VS's Output window
                    logging.AddConsole(c =>
                    {
                        // https://www.c-sharpcorner.com/blogs/date-and-time-format-in-c-sharp-programming1
                        c.TimestampFormat = "[HH:mm:ss.fff] ";
                        c.IncludeScopes = false;
                    });
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddFilter((provider, category, logLevel) =>
                    {
                        if (provider == "Microsoft.Extensions.Logging.Debug.DebugLoggerProvider")
                            return true;

                        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.0
                        if (category.StartsWith("SiloHost") || category.StartsWith("HelloWorld")) // || category.StartsWith("Orleans.Runtime.GrainDirectory.LocalGrainDirectory"))
                            return true;

                        if (logLevel > LogLevel.Information)
                            return true;

                        return false;

                    });
                });

            var built = hostBuilder.UseConsoleLifetime().Build();   //.RunConsoleAsync();

            return built.RunAsync();
        }
    }
}
