using System.Threading.Tasks;
using HelloWorld.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using SignalR.Orleans.Core;

namespace HelloWorld.Grains
{
    /// <summary>
    /// Orleans grain implementation class HelloGrain.
    /// </summary>
    public class HelloGrain : Orleans.Grain, IHello
    {
        private const string BroadcastMessage = "BroadcastMessage";
        private readonly ILogger<HelloGrain> _logger;
        private HubContext<IChatHub> _hubContext;

        public HelloGrain(ILogger<HelloGrain> logger)
        {
            this._logger = logger;
        }

        public override Task OnActivateAsync()
        {
            _logger.LogInformation($"{nameof(OnActivateAsync)} called");
            _hubContext = GrainFactory.GetHub<IChatHub>();
            return Task.CompletedTask;
        }

        async Task<string> IHello.SayHello(string greeting)
        {
            var groupId = "group1"; // this.GetPrimaryKeyString();
            const string name = "SayHello";

            _logger.LogInformation($"SayHello message received: greeting = '{greeting}'");
            _logger.LogInformation($"Sending message to group: {groupId}. MethodName:{BroadcastMessage} Name:{name}, Message:{greeting}");

            await _hubContext.Group(groupId).Send(BroadcastMessage, name, greeting);

            return await Task.FromResult($"You said: '{greeting}', I say: Hello!");
        }
    }
}
