using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DragonDiceRoller.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help"), Alias("?", "commands")]
        [Summary("Provides a list of available commands and what they do.")]
        public async Task HelpAsync()
        {
            IEnumerable<CommandInfo> commands = Program._commands.Commands.OrderBy(c => c.Name);
            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.Gold);

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string sCommandSummary = command.Summary ?? "No description available.";

                string sCommandAlias = "";

                if (command.Aliases.Count > 1)
                {

                    sCommandAlias = " AKA (";

                    for (int i = 1; i < command.Aliases.Count; i++)
                    {
                        sCommandAlias += command.Aliases[i];

                        if (i < command.Aliases.Count - 1)
                        {
                            sCommandAlias += ", ";
                        }
                    }

                    sCommandAlias += ")";
                }

                embedBuilder.AddField("*" + command.Name.ToString() + sCommandAlias, sCommandSummary);
            }

            await ReplyAsync("The following is a list of all commands currently available. For in-depth usage, enter the command followed by a '?'.", 
                false, embedBuilder.Build());
        }
    }
}
