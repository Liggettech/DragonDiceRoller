using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DragonDiceRoller.Modules
{
    public class Ping : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Summary("Pong? Pong!")]
        [Remarks("Because why not?")]
        public async Task PingAsync(string sInput = "")
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();

            //guide on how to use
            if (sInput == "?" || sInput == "help")
            {
                embedBuilder.AddField("助けて！", "私は箱の中にくっついている！");                
            }

            else
            {
                embedBuilder.WithTitle("Pong!")
                    .WithColor(Color.LighterGrey);
            }

            await ReplyAsync("", false, embedBuilder.Build());
        }
    }
}
