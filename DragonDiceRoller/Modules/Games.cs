using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using CustomTools;

namespace DragonDiceRoller.Modules
{    
    [Group("games")]
    public class Games : ModuleBase<SocketCommandContext>
    {        
        [Command, Alias("?", "help")]
        [Summary("Returns list of games available.")]
        public async Task DefaultAsync()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.Gold);

            embedBuilder.AddField("__**Games Help**__", 
                                  "* /games is a directory of mini-games available to play.\n" +
                                  "* '/games list' will display the list of games.\n" +
                                  "* All games have 3 basic commands: 'rules', 'play', and 'quit'\n" +
                                  "-Ex. '/games drink play' would start a drinking match.");

            await ReplyAsync("", false, embedBuilder.Build());
        }

        [Command("list")]
        [Summary("Returns list of games available.")]
        public async Task ListAsync()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.Orange);

            embedBuilder.AddField("18 Mules", "And none of them are named Larry.");
            embedBuilder.AddField("Drink", "Drink your opponent under the table, vomit, or both.");
            embedBuilder.AddField("Liar's Dice", "I ain't callin' you a truther!");
            embedBuilder.AddField("Pazaak", "Pure Pazaak");

            embedBuilder.WithTitle("__**Games**__");

            await ReplyAsync("", false, embedBuilder.Build());
        }

        [Group("drink")]
        public class DrinkModule : ModuleBase<SocketCommandContext>
        {
            [Command, Alias("?", "help", "rules")]
            [Summary("Explains the rules of the game")]
            public async Task RulesAsync()
            {
                EmbedBuilder embedBuilder = new EmbedBuilder()
                            .WithColor(Color.LightOrange)
                            .WithTitle("**Rules**");

                await ReplyAsync("", false, embedBuilder.Build());
            }

            [Command("play"), Alias("start")]
            [Summary("Starts game")]
            public async Task PlayAsync()
            {
                EmbedBuilder embedBuilder = new EmbedBuilder()
                            .WithColor(Color.LightOrange)
                            .WithTitle("**START**");

                await ReplyAsync("", false, embedBuilder.Build());
            }

            [Command("menu"), Alias("list")]
            [Summary("Returns alcoholic beverages on tap.")]
            public async Task MenuAsync(string sInput = "")
            {
                //list all drinks
                if (sInput == "")
                { 
                    foreach (Drink item in Program._lstDrinks)
                    {
                        if (item.Name != "")
                        {
                            EmbedBuilder embedBuilder = new EmbedBuilder()
                                .WithColor(Color.LightOrange)
                                .WithTitle("**" + item.Name + "**");

                            embedBuilder.AddInlineField("Name", item.Name);
                            embedBuilder.AddInlineField("Difficulty", item.Difficulty);
                            embedBuilder.AddInlineField("Description", item.Description);

                            await ReplyAsync("", false, embedBuilder.Build());
                        }
                    }
                }

                //display search format
                else if (sInput == "?" || sInput.ToLower().Trim() == "help")
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder()
                                .WithColor(Color.Gold)
                                .WithTitle("**Drink Menu Help**");

                    embedBuilder.AddField("**Drink Menu Help**",
                                          "* /drink menu returns the name of all drinks that match based on the field (F) and value (V) input in the F-V format.\n" +
                                          "Valid fields include: name, difficulty." +
                                          "* Partial words may be used, but mulitple results may appear if found.\n" +
                                          "- Ex. '/drink menu name-Ab' would list info for 'Absence' and 'Abyssal Peach'.");

                    await ReplyAsync("", false, embedBuilder.Build());
                }

                //search for drink
                else if (sInput.Contains("-"))
                {

                }

                //unable to decipher command
                else
                {

                }
            }
        }
    }
}


