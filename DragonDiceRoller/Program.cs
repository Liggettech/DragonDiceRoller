using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DragonDiceRoller
{
    //TODO: Make InitList more dynamic of object types
    //TODO: Make games commands
    //TODO: Make advanced skill test command
    //TODO: Expand poison making
    class Program
    {                
        public static readonly CommandService _commands = new CommandService();
        public static readonly List<Spell> _lstGrimoire = new List<Spell>();
        public static readonly List<Talent> _lstTalents = new List<Talent>();
        public static readonly List<Drink> _lstDrinks = new List<Drink>();

        private string _sToken;

        // Keep the CommandService and IServiceCollection around for use with commands.
        private readonly DiscordSocketClient _client;
        private readonly IServiceCollection _map;       
        private IServiceProvider _services;       

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                // How much logging do you want to see?
                LogLevel = LogSeverity.Info,

                // If you or another service needs to do anything with messages
                // (eg. checking Reactions, checking the content of edited/deleted messages),
                // you must set the MessageCacheSize. You may adjust the number as needed.
                //MessageCacheSize = 50,
            });

            _map = new ServiceCollection();

        }

        // Example of a logging handler. This can be re-used by addons
        // that ask for a Func<LogMessage, Task>.
        private static Task Logger(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }

        private async Task MainAsync()
        {
            // Subscribe the logging handler.
            _client.Log += Logger;

            // Centralize the logic for commands into a seperate method.
            await InitCommands();

            // Login and connect.
            await _client.LoginAsync(TokenType.Bot, _sToken);
            await _client.StartAsync();

            // Initialiaze static lists
            await InitList("Spell");
            await InitList("Talent");
            await InitList("Drink");
            //await InitLists();

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(-1);
        }

        private async Task InitCommands()
        {
            // Repeat this for all the service classes
            // and other dependencies that your commands might need.
            //_map.AddSingleton(new SomeServiceClass());
            _map.AddSingleton(_client);
            _map.AddSingleton(_commands);

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            _services = _map.BuildServiceProvider();            

            // Subscribe a handler to see if a message invokes a command.
            _client.MessageReceived += HandleCommandAsync;
            
            // Either search the program and add all Module classes that can be found.
            // Module classes *must* be marked 'public' or they will be ignored.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            // Or add Modules manually if you prefer to be a little more explicit:
            //await _commands.AddModuleAsync<>();

            // Read token string from txt file      
            string sPathToken = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\Token.txt");
            using (StreamReader reader = File.OpenText(sPathToken))
            {                
                _sToken = await reader.ReadLineAsync();                
            }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            SocketUserMessage msg = arg as SocketUserMessage;
            if (msg is null || msg.Author.IsBot)
            {
                return;
            }

            // Create a number to track where the prefix ends and the command begins
            int pos = 0;

            // Replace the '/' with whatever character
            // you want to prefix your commands with.
            if (msg.HasCharPrefix('/', ref pos))
            {
                // Create a Command Context.
                SocketCommandContext context = new SocketCommandContext(_client, msg);

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed succesfully).
                IResult result = await _commands.ExecuteAsync(context, pos, _services);

                // Uncomment the following lines if you want the bot
                // to send a message if it failed (not advised for most situations).
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
            // If user calls bot name, list all available commands         
            else if (msg.MentionedUsers.First().Id == _client.CurrentUser.Id)
            {
                //EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.Blue);
                //string sCommands = "The following is a list of all commands currently available. For in-depth usage, enter the command followed by a '?' or 'help'.\n";

                //// List all available commands
                //foreach (ModuleInfo module in _commands.Modules)
                //{
                //    sCommands = module.Name.ToString();

                //    if (module.Aliases != null)
                //    {
                //        sCommands += " AKA (" + module.Aliases.ToString() + ")";
                //    }

                //    sCommands += ": " + module.Summary.ToString() + "\n";
                //}

                //embedBuilder.AddField("Commands", sCommands);

                EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Color.Magenta);

                embedBuilder.AddField("__**HELLO**__", "Hey");

                await msg.Channel.SendMessageAsync ("", false, embedBuilder.Build());
            }
        }

        private async Task InitList(string sInFileName)
        {
            //grab text files containing spells
            string sPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\" + sInFileName + ".txt");            

            int iNumProperties = 0;
            string[] sLinesArray = null;

            try
            {
                //get number of properties in requested type               
                iNumProperties = Type.GetType("DragonDiceRoller." + sInFileName, true, true).GetProperties().Length;

                //read all lines
                sLinesArray = await File.ReadAllLinesAsync(sPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }                   

            if (iNumProperties > 0 && sLinesArray != null)
            {
                //iterate through each line, parsing out 
                foreach (string sLine in sLinesArray)
                {
                    string[] sLineSplit = sLine.Split('~');

                    if (sLineSplit.Length == iNumProperties)
                    {
                        try
                        {
                            switch (sInFileName)
                            {
                                case "Spell":
                                    Spell newSpell = new Spell(sLineSplit[0], sLineSplit[1], sLineSplit[2], Int32.Parse(sLineSplit[3]), sLineSplit[4],
                                                       Int32.Parse(sLineSplit[5]), sLineSplit[6], sLineSplit[7], sLineSplit[8]);

                                    //store in dictionary
                                    _lstGrimoire.Add(newSpell);
                                    break;

                                case "Talent":
                                    Talent newTalent = new Talent(sLineSplit[0], sLineSplit[1], sLineSplit[2], sLineSplit[3], sLineSplit[4],
                                                   sLineSplit[5], sLineSplit[6]);

                                    //store in dictionary
                                    _lstTalents.Add(newTalent);
                                    break;

                                case "Drink":
                                    Drink newDrink = new Drink(sLineSplit[0], sLineSplit[1], sLineSplit[2]);

                                    //store in dictionary
                                    _lstDrinks.Add(newDrink);
                                    break;
                            }                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    else
                    {
                        Console.WriteLine("Invalid number of fields in line to generate item " + sLineSplit[0]);
                    }
                }

                Console.WriteLine(sInFileName + " list compiled.");
            }
        }
    }
}
