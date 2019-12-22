using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HelloWorld.Interfaces;
using Orleans;
using Orleans.Runtime;

namespace HelloWorld.Grains
{
    public class HelloArchiveGrain : Grain, IHelloArchive
    {
        private readonly IPersistentState<GreetingArchive> _archive;

        public HelloArchiveGrain([PersistentState("archive", "ArchiveStorage")] IPersistentState<GreetingArchive> archive)
        {
            this._archive = archive;
        }

        public async Task<string> SayHello(string greeting)
        {
            this._archive.State.Greetings.Add(greeting);

            await this._archive.WriteStateAsync();

            //var notifier = GrainFactory.GetGrain<IUserNotificationGrain>(Guid.Empty.ToString());
            //await notifier.SendMessageAsync("test", greeting);

            return $"You said: '{greeting}', I say: Hello!";
        }

        public Task<IEnumerable<string>> GetGreetings() => Task.FromResult<IEnumerable<string>>(this._archive.State.Greetings);
    }

    public class GreetingArchive
    {
        public List<string> Greetings { get; } = new List<string>();
    }
}
