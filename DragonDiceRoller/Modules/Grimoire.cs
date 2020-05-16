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
    public class Grimoire : ModuleBase<SocketCommandContext>
    {
        [Command("spell")]
        [Summary("Returns all info on the specified spell(s) (spell names must be in quotations).")]
        public async Task SpellAsync(params string[] sInputLookup)
        {
            //no input provided
            if (sInputLookup.Length < 1)
            {
                string sContent = "Please provide in quotes the name of a spell(s) you would like you lookup.\n" +
                                  "If you are not sure which one to look for, try using part of the spell's name or the /spellbook command to look some up.";

                await PrintEmbedMessage("No Input", sContent, Color.LightGrey);
            }

            //guide on how to use
            else if (sInputLookup[0] == "?" || sInputLookup[0].ToLower().Trim() == "help")
            {
                string sContent = "On the quoted spell name(s), the following info will be returned: " +
                                  typeof(Spell).GetProperties().Select(s => s.Name.ToLower()).ToList().PrintWithDelimiter(',') + "." +
                                  "\nPartial words may be used, but mulitple results may appear if found." +
                                  "\n\n-Ex. '/spell \"Affliction Hex\" ' returns 'School: Entropy, Type: Attack, ...'" +
                                  "\n-Ex. '/spell \"A\" ' returns info for spells beginning with the letter 'A'." +
                                  "\n-Ex. '/spell \"Anti\" ' returns info for Anti-Magic Burst and Anti-Magic Ward." +
                                  "\n-Ex. '/spell \"Affl\" \"Anim\" ' returns info for Affliction Hex and Animate Dead.";

                await PrintEmbedMessage("Help with command: spell", sContent, Color.Gold);
            }

            else
            {
                //loop through each lookup word in the input array
                foreach (string sQueryWord in sInputLookup)
                {
                    IEnumerable<Spell> queryResults = 
                        from entry in Program._lstGrimoire
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
                        foreach (Spell entry in queryResults)
                        {
                            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.Blue);

                            embedBuilder.AddInlineField("School", entry.School);
                            embedBuilder.AddInlineField("Spell Type", entry.Type);
                            embedBuilder.AddInlineField("Mana Cost", entry.Cost);
                            embedBuilder.AddInlineField("Cast Time", entry.CastTime);
                            embedBuilder.AddInlineField("Target #", entry.TargetNumber);
                            embedBuilder.AddInlineField("Test", entry.Test);
                            embedBuilder.AddInlineField("Prerequisite", entry.Prerequisite);
                            embedBuilder.AddInlineField("Description", entry.Description);
                            embedBuilder.WithTitle("__**" + entry.Name + "**__");

                            await ReplyAsync("", false, embedBuilder.Build());
                        }
                    }

                    //could not find any keys that contain lookup word
                    else
                    {
                        await PrintEmbedMessage("404", "No spells found matching '" + sQueryWord + "'.", Color.Blue);
                    }                    
                }
            }            
        }

        [Command("spellbook"), Alias("grimoire", "spells")]
        [Summary("Provides the name of all spells that match based on the criteria of field (F) and value (V) in the F-V format.")]
        public async Task GrimoireAsync(params string[] sInputFilter)
        {         
            //no input provided, list all spells
            //block to 'GM' role only, otherwise act confused
            if (sInputFilter.Length < 1)
            {
                SocketGuildUser user = Context.User as SocketGuildUser;
                IRole role = (Context.User as IGuildUser).Guild.Roles.FirstOrDefault(r => r.Name == "GM");

                if (user.Roles.Contains(role))
                {
                    foreach (Spell entry in Program._lstGrimoire)
                    {
                        if (entry.Name != "")
                        {
                            EmbedBuilder embedBuilder = new EmbedBuilder()
                                .WithColor(Color.DarkBlue)
                                .WithTitle("**" + entry.Name + "**");

                            embedBuilder.AddInlineField("School", entry.School);
                            embedBuilder.AddInlineField("Spell Type", entry.Type);
                            embedBuilder.AddInlineField("Mana Cost", entry.Cost);
                            embedBuilder.AddInlineField("Cast Time", entry.CastTime);
                            embedBuilder.AddInlineField("Target #", entry.TargetNumber);
                            embedBuilder.AddInlineField("Test", entry.Test);
                            embedBuilder.AddInlineField("Prerequisite", entry.Prerequisite);
                            embedBuilder.AddInlineField("Description", entry.Description);

                            await ReplyAsync("", false, embedBuilder.Build());
                        }
                    }
                }

                else
                {
                    string sContent = "Please provide the field-value combo(s) to search for in the spellbook.\n" +
                                      "If you are not sure how to use this command, try using the /spellbook ? command for help.";

                    await PrintEmbedMessage("No input", sContent, Color.LightGrey);
                }
            }
            //guide on how to use
            else if (sInputFilter[0] == "?" || sInputFilter[0] == "help")
            {
                string sContent = "Returns the name of all spells that match based on the field (F) and value (V) input in the F-V format." +
                                  "\nSpell names with spaces must be in quotes. Cost and target number can use comparison operators if placed left of value number (default is =)." +
                                  "\n\nValid fields include: " + Spell.GetFilterFields().PrintWithDelimiter(',') + "." +
                                  "\n\nMultiple search parameters may be used. Partial words may also be used." +
                                  "\n -Ex. '/spellbook cost-<10 name-\"Anti\" ' will list all spells with a mana cost less than 10 and contain 'Anti' in their name.";

                await PrintEmbedMessage("Help with command: spellbook AKA (grimoire, spells)", sContent, Color.Gold);
            }
            else
            {
                IEnumerable< Spell> queryResults =
                    from entry in Program._lstGrimoire                    
                    select entry;

                //loop through each filter in the input array
                foreach (string sFilterCombo in sInputFilter)
                {
                    string sFilterField = "";
                    string sFilterValue = "";

                    try
                    {
                        string[] sFilterSplit = sFilterCombo.Split('-');
                        sFilterField = sFilterSplit[0].ToLower().Trim();
                        sFilterValue = sFilterSplit[1].ToLower().Trim();
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                        
                    //field input matches list of acceptable values
                    if(Spell.GetFilterFields().Contains(sFilterField) && 
                        sFilterField != "" && sFilterValue != "")
                    {
                        if (sFilterField.Contains("name"))
                        {
                            if (sFilterValue.Length == 1)
                            {
                                queryResults = queryResults.Where(e => e.Name.ToLower().StartsWith(sFilterValue));
                            }
                            else
                            {
                                queryResults = queryResults.Where(e => e.Name.ToLower().Contains(sFilterValue));
                            }
                        }

                        else if (sFilterField.Contains("school"))
                            queryResults = queryResults.Where(e => e.School.ToLower().Contains(sFilterValue));

                        else if (sFilterField.Contains("type"))
                            queryResults = queryResults.Where(e => e.Type.ToLower().Contains(sFilterValue));

                        else if (sFilterField.Contains("cost"))
                            EvaluateExpression(ref queryResults,"cost", sFilterValue);

                        else if (sFilterField.Contains("time"))
                            queryResults = queryResults.Where(e => e.CastTime.ToLower().Contains(sFilterValue));

                        else if (sFilterField.Contains("target"))
                            EvaluateExpression(ref queryResults, "target", sFilterValue);

                        else if (sFilterField.Contains("test"))
                            queryResults = queryResults.Where(e => e.Test.ToLower().Contains(sFilterValue));

                        else
                        {                           
                            await PrintEmbedMessage("Invalid entry", "'" + sFilterCombo + "' is not valid. Not including it in search.", Color.DarkRed);
                        }
                    }

                    else
                    {                       
                        if (sFilterField.Contains("description"))
                        {                            
                            await PrintEmbedMessage("Invalid entry", "Searching by description is not available.", Color.DarkRed);
                        }

                        else
                        {
                            await PrintEmbedMessage("Invalid entry", "'" + sFilterCombo + "' is not valid. Not including it in search.", Color.DarkRed);
                        }
                    }                                     
                }
                    
                //check to see if spellbook query was valid or too generic. (EmbedBuilder is limited by Discord to max number of fields)
                if (queryResults.Count() <= EmbedBuilder.MaxFieldCount)
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder()
                        .WithColor(Color.DarkBlue)
                        .WithTitle("__**Search Results**__");

                    //could not find any keys that contain lookup word
                    if (queryResults.Count() < 1)
                    {                      
                        embedBuilder.AddField("404", "No spells found matching your query.");
                    }

                    //display results
                   else
                    {
                        //loop through spellbook to extract each pair that contains the lookup word as a key
                        foreach (Spell entry in queryResults)
                        {                       
                            embedBuilder.AddInlineField("\u007F", entry.Name);
                        }
                    }                    

                    await ReplyAsync("", false, embedBuilder.Build());
                }

                else
                {
                    string sContent = "Query was either invalid or too generic that too many results were found. " +
                                      "\nPlease correct and/or be more specific with your query.";

                    await PrintEmbedMessage("Error", sContent, Color.DarkRed);
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

        //Attempts to filter query by locating any comparison operators for numerical field values
        private void EvaluateExpression(ref IEnumerable<Spell> query, string sFilterField, string sFilterValue)
        {
            int iFilterValue = 0;

            //just number, use equal expression
            if (Int32.TryParse(sFilterValue, out iFilterValue))
            {
                if (sFilterField == "cost")
                    query = query.Where(e => e.Cost == iFilterValue);

                else if (sFilterField == "target")
                    query = query.Where(e => e.TargetNumber == iFilterValue);
            }

            else
            {
                char[] cArrOperators = new char[] {'<', '>', '=' };
                
                //found an operator (must be left of number)
                if (sFilterValue.IndexOfAny(cArrOperators) == 0)
                {
                    //check for secondary operator
                    if (sFilterValue.Substring(1).IndexOfAny(cArrOperators) == 0)
                    {
                        string sOperators = sFilterValue.Substring(0, 2);

                        if (Int32.TryParse(sFilterValue.Substring(2), out iFilterValue))
                        {

                            if (sOperators == "==")
                            {
                                if (sFilterField == "cost")
                                    query = query.Where(e => e.Cost == iFilterValue);

                                else if (sFilterField == "target")
                                    query = query.Where(e => e.TargetNumber == iFilterValue);
                            }

                            else if (sOperators == "<=" || sOperators == "=<")
                            {
                                if (sFilterField == "cost")
                                    query = query.Where(e => e.Cost <= iFilterValue);

                                else if (sFilterField == "target")
                                    query = query.Where(e => e.TargetNumber <= iFilterValue);
                            }

                            else if (sOperators == ">=" || sOperators == "=>")
                            {
                                if (sFilterField == "cost")
                                    query = query.Where(e => e.Cost >= iFilterValue);

                                else if (sFilterField == "target")
                                    query = query.Where(e => e.TargetNumber >= iFilterValue);
                            }

                            else
                            {
                                EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.DarkBlue);
                                embedBuilder.AddField("Invalid entry", "'" + sFilterField + "-" + sFilterValue + "' is not valid. Not including it in search.");
                                ReplyAsync("", false, embedBuilder.Build());
                            }
                        }

                        else
                        {
                            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.DarkBlue);
                            embedBuilder.AddField("Invalid entry", "'" + sFilterField + "-" + sFilterValue + "' is not valid. Not including it in search.");
                            ReplyAsync("", false, embedBuilder.Build());
                        }
                    }

                    else
                    {
                        string sOperator = sFilterValue.Substring(0, 1);

                        if (Int32.TryParse(sFilterValue.Substring(1), out iFilterValue))
                        {
                            if (sOperator == "=")
                            {
                                if (sFilterField == "cost")
                                    query = query.Where(e => e.Cost == iFilterValue);

                                else if (sFilterField == "target")
                                    query = query.Where(e => e.TargetNumber == iFilterValue);
                            }

                            else if (sOperator == "<")
                            {
                                if (sFilterField == "cost")
                                    query = query.Where(e => e.Cost < iFilterValue);

                                else if (sFilterField == "target")
                                    query = query.Where(e => e.TargetNumber < iFilterValue);
                            }

                            else if (sOperator == ">")
                            {
                                if (sFilterField == "cost")
                                    query = query.Where(e => e.Cost > iFilterValue);

                                else if (sFilterField == "target")
                                    query = query.Where(e => e.TargetNumber > iFilterValue);
                            }

                            else
                            {
                                EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.DarkBlue);
                                embedBuilder.AddField("Invalid entry", "'" + sFilterField + "-" + sFilterValue + "' is not valid. Not including it in search.");
                                ReplyAsync("", false, embedBuilder.Build());
                            }
                        }

                        else
                        {                            
                            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.DarkBlue);
                            embedBuilder.AddField("Invalid entry", "'" + sFilterField + "-" + sFilterValue + "' is not valid. Not including it in search.");
                            ReplyAsync("", false, embedBuilder.Build());
                        }
                    }
                }

                //invalid entry
                else
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.DarkBlue);
                    embedBuilder.AddField("Invalid entry", "'" + sFilterField + "-" + sFilterValue + "' is not valid. Not including it in search.");
                    ReplyAsync("", false, embedBuilder.Build());
                }
            }            
        }
    }
}


