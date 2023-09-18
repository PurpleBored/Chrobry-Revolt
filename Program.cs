using Optionals;
using RevoltSharp.Commands;
using RevoltSharp;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Microsoft.Extensions.Configuration;
class Program
{
    public static RevoltClient Client;
    public static async Task Start()
    {
        IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("config.json")
            .Build();

        string token = Configuration["RevoltConfig:Token"];
        string apiUrl = Configuration["RevoltConfig:ApiUrl"];

        Client = new RevoltClient(token, ClientMode.WebSocket, new ClientConfig
        {
            ApiUrl = apiUrl
        });

        await Client.StartAsync();
        await Client.CurrentUser.ModifySelfAsync(statusText: new Option<string>("Now Open source! `?source`"));
        await Client.CurrentUser.ModifySelfAsync(statusType: new Option<UserStatusType>(UserStatusType.Focus));
        CommandHandler CommandHandler = new CommandHandler(Client);
        CommandHandler.LoadCommands();
        await Task.Delay(-1);
    }

    static void Main(string[] args)
    {
        Start().GetAwaiter().GetResult();
    }
}
// CommandHandler
public class CommandHandler
    {
        public CommandHandler(RevoltClient client)
        {
            Client = client;
            Client.OnMessageRecieved += Client_OnMessageRecieved;
            Service.OnCommandExecuted += Service_OnCommandExecuted;
        }
        private RevoltClient Client;
        private CommandService Service = new CommandService();
        
        // Here you can change the prefix Default one is "?"
        public const string Prefix = "?";
        
        public async Task LoadCommands()
        {
            await Service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }
        private void Client_OnMessageRecieved(Message msg)
        {
            UserMessage Message = msg as UserMessage;
            if (Message == null || Message.Author.IsBot)
                return;
            int argPos = 0;
            if (!(Message.HasCharPrefix('?', ref argPos) || Message.HasMentionPrefix(Client.CurrentUser, ref argPos)))
                return;
            CommandContext context = new CommandContext(Client, Message);
            Service.ExecuteAsync(context, argPos, null);
        }
        private void Service_OnCommandExecuted(Optional<CommandInfo> commandinfo, CommandContext context, IResult result)
        {
            if (result.IsSuccess)
                Console.WriteLine("Success command: " + commandinfo.Value.Name);
            else
            {
                if (!commandinfo.HasValue)
                    return;
                context.Channel.SendMessageAsync("Error: " + result.ErrorReason);
            }
        }


}
public class Commands : ModuleBase
{
    // Help commands:
    // Basic Help Command.
    [Command("help")]
    public async Task Help()
    {
        var helpMessage = "### List of available commands:\n\n" +
                          "### Practical Commands: \n" +
                          "`?help` - Displays this command.\n" +
                          "`?source` - Links to the bots source code. \n" +
                          "`?credits` - Displays bot's credit.\n" +
                          "`?test` - Simple test command you say test bot will response.\n" +
                          "`?invite` - Send a link to invite this bot to your server! \n" +
                          "`?changelog` - very simple command gives a list of changes and new additions! \n" +
                          "`?calculate` {num} {+, - , / , * } {num} - A very simple calculator. \n" +
                          "`?mod-help` - Displays a list of available mod commands. \n " +
                          "`?nsfw-help` - Sends a list of all aviable nsfw commands. \n " +
                          "### Fun Commands: \n" +
                          "`?roll` - Rolls a random number between 1 and 6.\n" +
                          "`?hi` - just says hi to the user. \n" +
                          "`?say` - says what the user told it to say!. \n" +
                          "`?dm` - Just DMs the user Hi :3. \n" +
                          "`?rps {paper,rock,scissors}` - Simple Rock paper scissors game.\n" +
                          "`?dogfact` - Gives a random dogfact using the Dogfact API!. \n" +
                          "`?catfact` - Gives a random Catfact using cat fact API (Currently somewhat bugged with the []. \n" +
                          "`?joke` - Very simple command just gives a joke using the Joke API. \n " +
                          "`?flipcoin` - a Command so easy a child could do it. \n " +
                          "`?fact` - Gives a random useless fact. \n " +
                          "`?urban + {word}` - uses the urban dictionary the search for the word. \n" +
                          "`?shitpost` - Sends a random shitpost. \n " +
                          "`?cat` - Cat :3 \n " +
                          "`?neko` - Neko command \n " +
                          "`?quote` - Gives a random quote using yet another API. \n ";
        await ReplyAsync(helpMessage);
    }

    // Mod Help command
    [Command("mod-help")]
    public async Task modhelp()
    {
        await ReplyAsync(
            "## List Of Available mod commands. \n `?kick {Mention}` - Kicks the user \n `?ban {mentions}` - Bans the User \n `?unban {mention}` - Unbans the user. \n");
    }

    // NSFW Help command:
    [Command("nsfw-help")]
    public async Task nsfwhelp()
    {
        await ReplyAsync("### Here are all the available NSFW commands: \n `?r34 {search}` - Searches rule34.xxx \n `?hentai` - Grabs a random hentai image \n ");
    }
    // End of the help command Section
    
    // Dm Commands (commands that dm the user in some way.):
    // Very simple DM command
    [Command("dm")]
    public async Task DM()
    {
        DMChannel DM = await Context.User.GetDMChannelAsync();
        if (DM == null)
        {
            await ReplyAsync("Could not open a DM :(");
            return;
        }

        await DM.SendMessageAsync("Hi :)");
    }

    // Invite Command - Sends a invite in the DM
    [Command("invite")]
    public async Task Invite()
    {
        DMChannel DM = await Context.User.GetDMChannelAsync();
        if (DM == null)
        {
            await ReplyAsync("Could not open DM :( wich means no invite :(((");
            return;
        }

        await DM.SendMessageAsync("## Invite me! \n" +
                                  "If you on app.haydar.dev use this link: \n" +
                                  "https://app.haydar.dev/bot/01HA55V3K8B26T87TBKMZMWRKJ \n" +
                                  "but if you on nightly.haydar.dev use this link \n" +
                                  "https://nightly.haydar.dev/bot/01HA55V3K8B26T87TBKMZMWRKJ \n" +
                                  "If you find any bugs report them to the bots creator. thank you bai");
    }
    
    // Simple Hi command DMs the user Hi
    [Command("hi")]
    public async Task Hi()
    {
        await ReplyAsync("Hi " + Context.User.Username);
    }
    // End of DM commands.
    
    // Debug commands.
    
    // Very Simple test Command
    [Command("test")]
    public async Task Test()
    {
        await ReplyAsync("Ig it works :tm:");
    }

    // Fun commands:

    // A Rock paper Scicors command.
    [Command("rps")]
    public async Task Rps([Remainder] string userChoice = null)
    {
        string[] options = { "rock", "paper", "scissors" };
        Random random = new Random();
        string botChoice = options[random.Next(options.Length)];

        if (userChoice == null)
        {
            await ReplyAsync("You need to provide a choice! Please choose from: rock, paper, scissors");
            return;
        }

        userChoice = userChoice.ToLower();

        if (Array.Exists(options, option => option == userChoice))
        {
            if (userChoice == botChoice)
            {
                await ReplyAsync($"It's a tie! I chose {botChoice} too.");
            }
            else if ((userChoice == "rock" && botChoice == "scissors") ||
                     (userChoice == "paper" && botChoice == "rock") ||
                     (userChoice == "scissors" && botChoice == "paper"))
            {
                await ReplyAsync($"You win! I chose {botChoice}.");
            }
            else
            {
                await ReplyAsync($"You lose! I chose {botChoice}.");
            }
        }
        else
        {
            await ReplyAsync($"Invalid choice. Please choose from: {string.Join(", ", options)}");
        }
    }

    // Roll command roles a random number between 1 - 6 (Perfect for bets :troll")
    [Command("roll")]
    public async Task Roll()
    {
        var random = new Random();
        var result = random.Next(1, 7);
        await ReplyAsync($"You rolled a {result}!");
    }

    // Very simple very fun say command.
        private List<string> blacklist = new List<string>
    {
        "nigga", "nigger", "n i g g a", "fuck", "shit", "piss", "cunt", "dick", "fag", "faggot", "kys", "ky$", // Blacklist for the say command
    };

    [Command("say")]
    public async Task Say([Remainder] string text)
    {
        var user = Context.User.Username;

        foreach (var word in blacklist)
        {
            if (text.ToLower().Contains(word))
            {
                text = text.Replace(word, new string('*', word.Length));
            }
        }

        await ReplyAsync($"{user} said: {text}");

    }

    // Haydar.
    [Command("miau")]
    public async Task miau()
    {
        await ReplyAsync("<@01H4H8M7E20HJAD6DXKDMXVKXF> Miau???");
    }

    // The flip a coin command.
    [Command("flipcoin")]
    public async Task FlipCoin()
    {
        Random random = new Random();
        int result = random.Next(2);

        string side = (result == 0) ? "Heads" : "Tails";

        await ReplyAsync($"It's {side}!");
    }
    // End of the Fun commands.

    // Api Commands (Commands which use some API (EXPECT NSFW COMMANDS!)):
    // Dog fact command very simple very fun
    [Command("dogfact")]
    public async Task DogFact()
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = "http://dog-api.kinduff.com/api/facts";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string factsJson = await response.Content.ReadAsStringAsync();
                JObject factsObject = JObject.Parse(factsJson);

                if (factsObject["facts"] is JArray factsArray && factsArray.Count > 0)
                {
                    Random random = new Random();
                    int randomIndex = random.Next(factsArray.Count);
                    string randomFact = factsArray[randomIndex].ToString();

                    await ReplyAsync(randomFact);
                }
                else
                {
                    await ReplyAsync("Sorry, I couldn't fetch a dog fact at the moment. Please try again later.");
                }
            }
            else
            {
                await ReplyAsync("Sorry, I couldn't fetch a dog fact at the moment. Please try again later.");
            }
        }
    }
    // Cat fact command very simple very fun
    [Command("catfact")]
    public async Task CatFact()
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = "http://meowfacts.herokuapp.com/";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string factJson = await response.Content.ReadAsStringAsync();
                JObject factObject = JObject.Parse(factJson);

                if (factObject["data"] != null)
                {
                    string fact = factObject["data"].ToString();
                    await ReplyAsync(fact);
                }
                else
                {
                    await ReplyAsync("Sorry, I couldn't fetch a cat fact at the moment. Please try again later.");
                }
            }
            else
            {
                await ReplyAsync("Sorry, I couldn't fetch a cat fact at the moment. Please try again later.");
            }
        }
    }
    // Joke command
    [Command("joke")]
    public async Task Joke()
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = "https://v2.jokeapi.dev/joke/Any?type=twopart";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string jokeJson = await response.Content.ReadAsStringAsync();
                JObject jokeObject = JObject.Parse(jokeJson);

                string setup = jokeObject["setup"]?.ToString();
                string punchline = jokeObject["delivery"]?.ToString();

                if (!string.IsNullOrEmpty(setup) && !string.IsNullOrEmpty(punchline))
                {
                    await ReplyAsync($"{setup}\n{punchline}");
                }
                else
                {
                    await ReplyAsync("Sorry, I couldn't fetch a joke at the moment. Please try again later.");
                }
            }
            else
            {
                await ReplyAsync("Sorry, I couldn't fetch a joke at the moment. Please try again later.");
            }
        }
    }
    // Simple quote command 
    [Command("quote")]
    public async Task Quote()
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = "https://api.quotable.io/random";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string quoteJson = await response.Content.ReadAsStringAsync();
                dynamic quoteObject = Newtonsoft.Json.JsonConvert.DeserializeObject(quoteJson);

                string content = quoteObject.content;
                string author = quoteObject.author;

                await ReplyAsync($"\"{content}\" - {author}");
            }
            else
            {
                await ReplyAsync("Sorry, I couldn't fetch a quote at the moment. Please try again later.");
            }
        }
    }
    // Fact command
    [Command("fact")]
    public async Task Fact()
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = "https://uselessfacts.jsph.pl/random.json?language=en";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string factJson = await response.Content.ReadAsStringAsync();
                dynamic factObject = Newtonsoft.Json.JsonConvert.DeserializeObject(factJson);

                string fact = factObject.text;
                await ReplyAsync(fact);
            }
            else
            {
                await ReplyAsync("Sorry, I couldn't fetch a fact at the moment. Please try again later.");
            }
        }
    }
    // Urban Dictionary
    [Command("urban")]
    public async Task Urban([Remainder] string term)
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = $"https://api.urbandictionary.com/v0/define?term={term}";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string urbanJson = await response.Content.ReadAsStringAsync();
                dynamic urbanObject = Newtonsoft.Json.JsonConvert.DeserializeObject(urbanJson);

                if (urbanObject.list.Count > 0)
                {
                    string definition = urbanObject.list[0].definition;

                    await ReplyAsync($"**Term:** {term}\n**Definition:** {definition}");
                }
                else
                {
                    await ReplyAsync("Sorry, I couldn't find a definition for that term.");
                }
            }
            else
            {
                await ReplyAsync("Sorry, I couldn't fetch a definition at the moment. Please try again later.");
            }
        }
    }
    // Shitpost command
    [Command("shitpost")]
    public async Task shitpost()
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = "https://api.thedailyshitpost.net/random";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string memeJson = await response.Content.ReadAsStringAsync();
                dynamic memeObject = Newtonsoft.Json.JsonConvert.DeserializeObject(memeJson);

                string title = memeObject.title;
                string imageUrl = memeObject.url;

                await ReplyAsync($"{imageUrl}");
            }
            else
            {
                await ReplyAsync("Sorry, I couldn't fetch a meme at the moment. Please try again later.");
            }
        }
    }
    // Cat.
    [Command("cat")]
    public async Task Cat()
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = "https://api.thecatapi.com/v1/images/search";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string catJson = await response.Content.ReadAsStringAsync();
                dynamic catObject = Newtonsoft.Json.JsonConvert.DeserializeObject(catJson);

                string imageUrl = catObject[0].url;

                await ReplyAsync(imageUrl);
            }
            else
            {
                await ReplyAsync("Sorry, I couldn't fetch a cat image at the moment. Please try again later.");
            }
        }
    }
    // Neko command
    [Command("neko")]
    public async Task Neko()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://nekos.life/api/v2/img/neko";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string nekoJson = await response.Content.ReadAsStringAsync();
                    JObject nekoObject = JObject.Parse(nekoJson);

                    string imageUrl = nekoObject["url"]?.ToString();

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        await ReplyAsync(imageUrl);
                    }
                    else
                    {
                        await ReplyAsync("Sorry, I couldn't fetch a neko at the moment. Please try again later.");
                    }
                }
                else
                {
                    await ReplyAsync("Sorry, I couldn't fetch a neko at the moment. Please try again later.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Neko command: {ex.Message}");
            await ReplyAsync("An error occurred while fetching the neko. Please try again later.");
        }
    }
    // End of neko command

    // The End of the API commands.
    // Misc Commands:
    // Give the credits of the bot
    [Command("credits")]
    public async Task Credits()
    {
        await ReplyAsync("This bot is made using revoltsharp by Purplebored known as Kniaż Jarema on nightly");
    }

    [Command("changelog")]
    public async Task changelog()
    {
        await ReplyAsync(
            "# Changelog: \n ### Neko! 0.0.4 \n New command! ?neko!)");
    }
    // Simple source command that leads to this project source
    [Command("source")]
    public async Task source()
    {
        await ReplyAsync("If you want to contribute or check out the source code you can do it here: https://codeberg.org/Purplebored/Chrobry");
    }
    // End of the misc commands

    // Calculate command
    [Command("calculate")]
    public async Task Calculate(string num1String, string operation, string num2String)
    {
        if (!double.TryParse(num1String, NumberStyles.Any, CultureInfo.InvariantCulture, out double num1) ||
            !double.TryParse(num2String, NumberStyles.Any, CultureInfo.InvariantCulture, out double num2))
        {
            await ReplyAsync("Error: Invalid input. Please provide valid numbers.");
            return;
        }

        double result = 0;

        switch (operation)
        {
            case "+":
                result = num1 + num2;
                break;
            case "-":
                result = num1 - num2;
                break;
            case "*":
                result = num1 * num2;
                break;
            case "/":
                if (num2 != 0) // Check for division by zero
                {
                    result = num1 / num2;
                }
                else
                {
                    await ReplyAsync("Error: Division by zero is not allowed.");
                    return;
                }

                break;
            default:
                await ReplyAsync("Invalid operation. Please use +, -, *, or /.");
                return;
        }

        await ReplyAsync($"Result: {result}");
    }

    // Moderation commands. from Darkly Bot
    // Stuff neded to make these commands work.
    private string RemoveMention(string mention)
    {
        return mention.Replace("<@", "").Replace(">", "");
    }
    private RevoltClient Client;
    // Stats command
    [Command("stats")]
    public async Task GetStats()
    {
        IReadOnlyCollection<ServerMember> result1 = await Context.Server.GetMembersAsync();
        int num2 = 0;
        foreach (ServerMember serverMember in result1)
            ++num2;
        IReadOnlyCollection<ServerMember> result2 = await Context.Server.GetMembersAsync(true);
        int num3 = 0;
        foreach (ServerMember serverMember in result2)
            ++num3;
        await ReplyAsync($"### Stats:\n \n ### Member count: {num2}\n ### Online member count: {num3}\n");
    }
    // Ban Command
    [Command("ban")]
    public async Task BanUser([Remainder] string args)
    {
        try
        {
            string userId = RemoveMention(args);
            await Context.Server.BanMemberAsync(userId, "Banned via command.");
            await ReplyAsync("### Ban\n whomp whomp.");
        }
        catch
        {
            await ReplyAsync("### ERROR \n Invalid Mention or Bot Perms.");
        }
    }
    // Unban Command
    [Command("unban")]
    public async Task UnbanUser([Remainder] string args)
    {
        try
        {
            string userId = RemoveMention(args);
            await Context.Server.UnBanMemberAsync(userId);
            await ReplyAsync("### Unbanned \n Very sadly a mod had a change of his heart.");
        }
        catch
        {
            await ReplyAsync("### ERROR \n Invalid Mention or Bot Perms.");
        }
    }
    // Kick Command
    [Command("kick")]
    public async Task KickUser([Remainder] string args)
    {
        try
        {
            string userId = RemoveMention(args);
            await Context.Server.KickMemberAsync(userId);
            await ReplyAsync("### Kicked \n And he gone.");
        }
        catch
        {
            await ReplyAsync("### ERROR \\n Invalid Mention or Bot Perms.");
        }
    }
    // NSFW COMMANDS
    // R34 command.
    [Command("r34")]
    public async Task ExampleCommand([Remainder] string query = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            await Context.Channel.SendMessageAsync("Please provide a search query. Usage: ?r34 {searchquery}");
            return;
        }

        if (!(Context.Channel is TextChannel textChannel) || !textChannel.IsNsfw)
        {
            await Context.Channel.SendMessageAsync(
                "This command is only allowed in NSFW channels. So go to a NSFW channel to get your NSFW smh");
            return;
        }

        using (HttpClient httpClient = new HttpClient())
        {
            string apiUrl = $"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&limit=1&json=1&tags={query}";

            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string jsonResult = await response.Content.ReadAsStringAsync();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResult);

                if (data != null && data.Count > 0 && data[0].file_url != null)
                {
                    string imageUrl = data[0].file_url;
                    await Context.Channel.SendMessageAsync(imageUrl);
                }
                else
                {
                    await Context.Channel.SendMessageAsync("No images found.");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(
                    "Sorry, I couldn't fetch an image at the moment. Please try again later.");
            }
        }
    }
    // Hentai command.
    [Command("hentai")]
    public async Task hentai()
    {
                if (!(Context.Channel is TextChannel textChannel) || !textChannel.IsNsfw)
        {
            await Context.Channel.SendMessageAsync(
                "This command is only allowed in NSFW channels. So go to a NSFW channel to get your NSFW smh");
            return;
        }
        using (HttpClient client = new HttpClient())
    {
        string apiUrl = "https://nekobot.xyz/api/image?type=hneko";
        HttpResponseMessage response = await client.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            string nekoJson = await response.Content.ReadAsStringAsync();
            dynamic nekoObject = Newtonsoft.Json.JsonConvert.DeserializeObject(nekoJson);

            string imageUrl = nekoObject["message"];

            await ReplyAsync(imageUrl);
        }
        else
        {
            await ReplyAsync("Sorry, I couldn't fetch a neko image at the moment. Please try again later.");
        }
    }
    }

}