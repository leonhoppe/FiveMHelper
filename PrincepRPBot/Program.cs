using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace PrincepRPBot {
    class Program {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync() {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += async message => Console.WriteLine(message);

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, "OTQyODU0ODUzNDQzNjA0NTEx.YgqkAg.qDCfmSyewDMbXcyjXrONPNzEbaA");
            await _client.StartAsync();

            var source = new TaskCompletionSource<object>();
            _client.Ready += () => {
                source.TrySetResult(null);
                return Task.CompletedTask;
            };
            await source.Task;
            
            await _client.SetGameAsync("PrincepRP");
            await UpdateOnlineStatus();
        }

        private async Task UpdateOnlineStatus() {
            SocketGuild server = _client.Guilds.First();
            SocketTextChannel updateChannel = server.GetTextChannel(962831793147486248);
            
            while (true) {
                string channelName;

                if (IsServerOnline())
                    channelName = "Online";
                else
                    channelName = "Offline";

                await updateChannel.ModifyAsync(props => props.Name = channelName);
                
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }
        
        private async Task RegisterCommandsAsync() {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg) {
            var message = arg as SocketUserMessage;
            if (message is null || message.Author.IsBot) return;
            var context = new SocketCommandContext(_client, message);
            
            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }

        private bool IsServerOnline() {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://api.princep.de/info.json");
            request.Timeout = 15000;
            request.Method = "HEAD";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}