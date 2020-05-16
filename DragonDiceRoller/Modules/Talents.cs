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
    public class Talents : ModuleBase<SocketCommandContext>
    {        
        [Command("talent")]
        [Summary("Returns all info on the specified talent(s)/specialization(s) (names must be in quotations).")]
        public async Task TalentAsync(params string[] sInputLookup)
        {
            //no input provided
            if (sInputLookup.Length < 1)
            {
                string sContent = "Please provide in quotes the name of a talent(s) or specialization you would like you lookup.\n" +
                                  "If you are not sure which one to look for, try using part of the talent's name or the /spellbook command to look some up.";

                await PrintEmbedMessage("No Input", sContent, Color.LightGrey);
            }

            //guide on how to use
            else if (sInputLookup[0] == "?" || sInputLookup[0].ToLower().Trim() == "help")
            {
                string sContent = "On the quoted talent or specialization's name(s), the following info will be returned: " +
                                   typeof(Talent).GetProperties().Select(s => s.Name.ToLower()).ToList().PrintWithDelimiter(',') + "." +
                                  "\nPartial word may be used, but mulitple results may appear if found." +
                                  "\n\n-Ex. '/talent \"Animal Training\" ' returns info for Animal Training." +
                                  "\n-Ex. '/talent \"A\" ' returns info for talents beginning with the letter 'A'." +
                                  "\n-Ex. '/talent \"Ar\" ' returns info for Armor Training, Arcane Warrior, and Archery." +
                                  "\n-Ex. '/talent \"Arch\" \"Anim\" ' returns info for Archery and Animal Training.";

                await PrintEmbedMessage("Help with command: talent", sContent, Color.Gold);
            }

            else
            {
                //loop through each lookup word in the input array
                foreach (string sQueryWord in sInputLookup)
                {
                    IEnumerable<Talent> queryResults = 
                        from entry in Program._lstTalents
                        select entry;

                    if (sQueryWord.Length == 1)
                    {
                        queryResults = queryResults.Where(e => e.Name.ToLower().StartsWith(sQueryWord.ToLower()));
                    }
                    else
                    {
                        queryResults = queryResults.Where(e => e.Name.ToLower().Contains(sQueryWord.ToLower()));
                    }
                   
                    //check to see if spellbook found the lookup word as a key from the query
                    if (queryResults.Count() > 0)
                    {
                        //loop through spellbook to extract each pair that contains the lookup word as a key
                        foreach (Talent entry in queryResults)
                        {
                            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.Green);

                            embedBuilder.AddInlineField("Classes", entry.Classes);
                            embedBuilder.AddInlineField("Prerequisite", entry.Prerequisite);
                            embedBuilder.AddInlineField("Description", entry.Description);
                            embedBuilder.AddInlineField("Novice", entry.Novice);
                            embedBuilder.AddInlineField("Journeyman", entry.Journeyman);
                            embedBuilder.AddInlineField("Master", entry.Master);

                            embedBuilder.WithTitle("__**" + entry.Name + "**__");

                            await ReplyAsync("", false, embedBuilder.Build());
                        }
                    }

                    //could not find any keys that contain lookup word
                    else
                    {
                        await PrintEmbedMessage("404", "No talent or specializations found matching '" + sQueryWord + "'.", Color.Green);
                    }                    
                }
            }            
        }

        private async Task PrintEmbedMessage(string sEmbedTitle, string sEmbedContent, Color embedColor = default(Color), 
                                             string sMsgTitle = "", string sMsgDescription = "", string sHeader = "")
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.AddField(sEmbedTitle, sEmbedContent);

            if (!embedColor.Equals(Color.Default))
            {
                embedBuilder.WithColor(embedColor);
            }

            if (sMsgDescription != "")
            {
                embedBuilder.WithDescription(sMsgDescription);
            }

            if (sMsgTitle != "")
            {
                embedBuilder.WithTitle(sMsgTitle);
            }
            
            await ReplyAsync(sHeader, false, embedBuilder.Build());
        }
    }
}


