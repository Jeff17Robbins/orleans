using System.Net;
using System.Threading.Tasks;
using HelloWorld.Grains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace OrleansSiloHost
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            return new HostBuilder()
                .UseOrleans((context, siloBuilder) =>
                {
                    siloBuilder
                        .UseSignalR(builder =>
                        {
                            builder
                                .Configure((innerSiloBuilder, config) =>
                                {
                                    innerSiloBuilder
                                        //.UseLocalhostClustering(serviceId: "ChatSampleApp", clusterId: "dev")
                                        .AddMemoryGrainStorage("PubSubStore")
                                        //.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(UserNotificationGrain).Assembly).WithReferences());
                                        .UseLocalhostClustering()
                                        .Configure<ClusterOptions>(options =>
                                        {
                                            options.ClusterId = "dev";
                                            options.ServiceId = "HelloWorldApp";
                                        })
                                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                                        .AddMemoryGrainStorage(name: "ArchiveStorage");
                                });
                        });
                })



                .ConfigureServices(services =>
                {
                    services.Configure<ConsoleLifetimeOptions>(options =>
                    {
                        options.SuppressStatusMessages = true;
                    });
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                })
                .RunConsoleAsync();
        }
    }
}
