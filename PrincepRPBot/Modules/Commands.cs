using System.Threading.Tasks;
using Discord.Commands;

namespace PrincepRPBot.Modules {
    public class Commands : ModuleBase<SocketCommandContext> {
        
        [Command("ping")]
        public async Task Ping() {
            await ReplyAsync("Pong!");
        }

        [Command("penis")]
        public async Task Penis() {
            await ReplyAsync("Fotze");
        }
        
    }
}