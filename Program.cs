using System.Diagnostics;
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

    public static async Task Main(string[] args)
    {
        await StartAsync();
    }

    public static async Task StartAsync()
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

        // Modify user status
        await Client.CurrentUser.ModifySelfAsync(
            statusText: new Option<string>("?help."),
            statusType: new Option<UserStatusType>(UserStatusType.Focus)
        );

        CommandHandler commandHandler = new CommandHandler(Client);
        commandHandler.LoadCommands();

        await Task.Delay(-1);
    }
}
// CommandHandler
public class CommandHandler
{
    private readonly RevoltClient _client;
    private readonly CommandService _service = new CommandService();

    // Here you can change the prefix. Default is "?"
    public const string Prefix = "?";

    public CommandHandler(RevoltClient client)
    {
        _client = client;
        _client.OnMessageRecieved += Client_OnMessageReceived;
        _service.OnCommandExecuted += Service_OnCommandExecuted;
    }

    public async Task LoadCommands()
    {
        // Load commands from the entry assembly
        await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
    }

    private void Client_OnMessageReceived(Message msg)
    {
        if (msg is not UserMessage userMessage || userMessage.Author.IsBot)
            return;

        int argPos = 0;
        if (!(userMessage.HasCharPrefix('?', ref argPos) ||
              userMessage.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            return;

        CommandContext context = new CommandContext(_client, userMessage);
        _service.ExecuteAsync(context, argPos, null);
    }

    private void Service_OnCommandExecuted(Optional<CommandInfo> commandInfo, CommandContext context, IResult result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine("Success command: " + commandInfo.Value.Name);
        }
        else
        {
            if (commandInfo.HasValue)
            {
                context.Channel.SendMessageAsync("Error: " + result.ErrorReason);
            }
        }
    }

    // Only god and the developer of RevoltSharp know what's going on here.
}
public class Commands : ModuleBase
{
    // Help commands:

    // Help command.
    [Command("help")]
    public async Task Help()
    {
        var helpMessage =
            "#### List of available commands:\n\n" +
            "#### Practical Commands: \n" +
            "`?help` - Displays this command.\n" +
            "`?credits` - Displays bot's credit.\n" +
            "`?test` - Simple test command you say test bot will response.\n" +
            "`?invite` - Send a link to invite this bot to your server! \n" +
            "`?calculate` {num} {+, - , / , * } {num} - A very simple calculator. \n" +
            "`?mod-help` - Displays a list of available mod commands. \n " +
            "`?ping` - tests the bot ping. \n" +
            "`?nsfw-help` - Sends a list of all available nsfw commands. \n " +
            "#### Fun Commands: \n" +
            "`?dice` - Rolls a random number between 1 and 6.\n" +
            "`?say` - says what the user told it to say!. \n" +
            "`?dm` - Just DMs the user Hi :3. \n" +
            "`?rps {paper,rock,scissors}` - Simple Rock paper scissors game.\n" +
            "`?dogfact` - Gives a random dogfact using the Dogfact API!. \n" +
            "`?catfact` - Gives a random Catfact using cat fact API (Currently somewhat bugged with the []). \n" +
            "`?joke` - Very simple command just gives a joke using the Joke API. \n " +
            "`?coinflip` - a Command so easy a child could do it. \n " +
            "`?fact` - Gives a random useless fact. \n " +
            "`?urban + {word}` - uses the urban dictionary to search for the word. \n" +
            "`?shitpost` - Sends a random shitpost. (The shitpost API is broken.) \n " +
            "`?cat` - Cat :3 \n " +
            "`?neko` - Neko command \n " +
            "`?advice` - Gives the user a life Advice \n " +
            "`?quote` - Gives a random quote using yet another API. \n " +
            "`?gif {term}` - Allows the user to search for gifs using giphy (Beta) \n" +
            "`?avatar` - Sends the user Avatar (Beta) \n ";
        await ReplyAsync(helpMessage);
    }
    // End of the Help command.

    // Mod Help command
    [Command("mod-help")]
    public async Task ModHelp()
    {
        string helpMessage =
            "## List Of Available mod commands. \n" +
            "`?kick {Mention}` - Kicks the user \n" +
            "`?ban {mentions}` - Bans the User \n" +
            "`?unban {mention}` - Unbans the user. \n";

        await ReplyAsync(helpMessage);
    }
    // End of the mod-Help command

    // NSFW Help command:
    [Command("nsfw-help")]
    public async Task nsfwhelp()
    {
        await ReplyAsync("### Here are all the available NSFW commands: \n `?r34 {search}` - Searches rule34.xxx \n `?hentai` - Grabs a random hentai image \n ");
    }
    // End of the NSFW-HELP command

    // End of the help commands

    // Dm Commands (commands that dm the user in some way.):

    // Invite Command - Sends a invite in the DM
    [Command("invite")]
    public async Task Invite()
    {
        DMChannel DM = await Context.User.GetDMChannelAsync();

        if (DM == null)
        {
            await ReplyAsync("Could not open DM :(");
            return;
        }

        await DM.SendMessageAsync(
            "## Invite me! \n" +
            "If you're on app.haydar.dev, use this link: \n" +
            "https://app.haydar.dev/bot/01HA55V3K8B26T87TBKMZMWRKJ \n" +
            "But if you're on nightly.haydar.dev, use this link: \n" +
            "https://nightly.haydar.dev/bot/01HA55V3K8B26T87TBKMZMWRKJ \n" +
            "If you find any bugs, report them to the bot's creator. Thank you and bye!"
        );
    }
    // End of the Invite command

    // The Debug command.
    [Command("debug")]
    public async Task Debug()
    {
        DMChannel DM = await Context.User.GetDMChannelAsync();

        if (DM == null)
        {
            await ReplyAsync("Your code is so messed up that even DMs are broken :skull: wow");
            return;
        }

        try
        {
            // Send initial message
            await DM.SendMessageAsync("DMs work if you received this message.");
            await DM.SendMessageAsync("-----");
            await DM.SendMessageAsync("You should receive the ping of the bot and a shitpost");

            // Ping part
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await DM.SendMessageAsync("-----");
            stopwatch.Stop();
            var latency = stopwatch.ElapsedMilliseconds;
            await DM.SendMessageAsync($"The ping is: {latency}ms");
            await DM.SendMessageAsync("-----");

            // Shitpost part
            await SendRandomMeme(DM);
            await DM.SendMessageAsync("If you saw a Shitpost and the ping of the bot everything should work correctly.");
        }
        catch (Exception ex)
        {
            await DM.SendMessageAsync($"An error occurred: {ex.Message}");
        }
    }

    // The Shitpost Part.
    private async Task SendRandomMeme(DMChannel DM)
    {
        const string apiUrl = "https://api.thedailyshitpost.net/random";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string memeJson = await response.Content.ReadAsStringAsync();
                    dynamic memeObject = Newtonsoft.Json.JsonConvert.DeserializeObject(memeJson);

                    string title = memeObject.title;
                    string imageUrl = memeObject.url;

                    await DM.SendMessageAsync($"{imageUrl}");
                }
                else
                {
                    await DM.SendMessageAsync("Sorry, The Shitpost API is having issues again.");
                }
            }
            catch (Exception ex)
            {
                await DM.SendMessageAsync($"An error occurred while fetching a meme: {ex.Message}");
            }
        }
    }

    // End of the Debug command.

    // End of DM commands.

    // Debug commands.

    // Very Simple test Command
    [Command("test")]
    public async Task Test()
    {
        await ReplyAsync("Ig it works :tm:");
    }

    // Ping command.
    [Command("ping")]
    public async Task PingCommand()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await ReplyAsync("Ping...");

        stopwatch.Stop();
        var latency = stopwatch.ElapsedMilliseconds;

        await ReplyAsync($"Pong! (Latency: {latency}ms)");
    }

    // End of the Debug commands.

    // Fun commands:

    // Gif command
    [Command("gif")]
    public async Task GifCommand([Remainder] string keyword)
    {
        using (HttpClient client = new HttpClient())
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("giphy.json")
                .Build();

            string apiKey = Configuration["GiphyApiKey"];
            string apiUrl = $"https://api.giphy.com/v1/gifs/search?api_key={apiKey}&q={keyword}&limit=1";

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string gifJson = await response.Content.ReadAsStringAsync();
                JObject gifObject = JObject.Parse(gifJson);

                string gifUrl = gifObject["data"][0]["images"]["original"]["url"]?.ToString();

                if (!string.IsNullOrEmpty(gifUrl))
                {
                    await Context.Channel.SendMessageAsync(gifUrl);
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Sorry, I couldn't find a GIF for that keyword.");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("Sorry, I couldn't fetch a GIF at the moment. Please try again later.");
            }
        }
    }
    // End of the GIF commands

    // A Rock paper Scissors command.
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

        if (options.Contains(userChoice))
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
    // End of the Rock paper scicors command

    // Dice command rolls a random number between 1 - 6 (Perfect for bets :troll")
    [Command("dice")]
    public async Task Dice()
    {
        var random = new Random();
        var result = random.Next(1, 7);
        await ReplyAsync($"You rolled a {result}!");
    }
    // End of the Dice command

    // Pick Command
    [Command("roll")]
    public async Task Roll([Remainder] string randnum = null)
    {
        if (randnum == null)
        {
            await ReplyAsync("Please specify a number to roll");
        }
        else if (int.TryParse(randnum, out int maxNumber) && maxNumber > 0)
        {
            try
            {
                var random = new Random();
                var result = random.Next(1, maxNumber + 1);
                await ReplyAsync($"You rolled a {result}!");
            }
            catch (Exception ex)
            {
                await ReplyAsync("An error occurred while rolling the dice.");
            }
        }
        else
        {
            await ReplyAsync("Please pick a valid number.");
        }
    }
    // End of the pick command

    // Very simple say command.
    private List<string> blacklist = new List<string>
{
    "nigga", "nigger", "n i g g a", "fuck", "shit", "piss",
    "cunt", "dick", "fag", "faggot", "kys", "ky$", 
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
    // End of the Say command

    // The coinflip command.
    [Command("coinflip")]
    public async Task Coinflip()
    {
        Random random = new Random();
        int result = random.Next(2);

        string side = (result == 0) ? "Heads" : "Tails";

        await ReplyAsync($"It's {side}!");
    }
    // End of the coinflip command.

    // End of the Fun commands.

    // Api commands

    // Advice command.
    [Command("advice")]
    public async Task Advice()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string apiUrl = "https://api.adviceslip.com/advice";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string adviceJson = await response.Content.ReadAsStringAsync();
                    JObject adviceObject = JObject.Parse(adviceJson);

                    string advice = adviceObject["slip"]?["advice"]?.ToString();

                    if (!string.IsNullOrEmpty(advice))
                    {
                        await ReplyAsync($"Advice: {advice}");
                    }
                    else
                    {
                        await ReplyAsync("Sorry, I couldn't fetch advice at the moment. Please try again later.");
                    }
                }
                else
                {
                    await ReplyAsync("Sorry, I couldn't fetch advice at the moment. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }
    }
    // End of the Advice command

    // Dog fact command very simple very fun
    [Command("dogfact")]
    public async Task DogFact()
    {
        using (HttpClient client = new HttpClient())
        {
            try
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
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }
    }
    // End of the Dogfact command

    // Cat fact command very simple very fun
    [Command("catfact")]
    public async Task CatFact()
    {
        using (HttpClient client = new HttpClient())
        {
            try
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
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }
    }
    // End of the Cat fact command

    // Joke command
    [Command("joke")]
    public async Task Joke()
    {
        using (HttpClient client = new HttpClient())
        {
            try
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
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }
    }
    // End of the Joke command

    // Simple quote command 
    [Command("quote")]
    public async Task Quote()
    {
        using (HttpClient client = new HttpClient())
        {
            try
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
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }
    }
    // End of the qoute command

    // Fact command
    [Command("fact")]
    public async Task Fact()
    {
        using (HttpClient client = new HttpClient())
        {
            try
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
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }
    }
    // End of the fact command

    // Urban command
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
                    int maxDefinitions = 3;

                    for (int i = 0; i < Math.Min(maxDefinitions, urbanObject.list.Count); i++)
                    {
                        string definition = urbanObject.list[i].definition;
                        string formattedMessage = $"```\nTerm: {term}\nDefinition {i + 1}:\n{definition}\n```";
                        await ReplyAsync(formattedMessage);
                    }
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
    // End of the Urban command

    // Shitpost command
    [Command("shitpost")]
    public async Task Shitpost()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string apiUrl = "https://api.thedailyshitpost.net/random";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string memeJson = await response.Content.ReadAsStringAsync();
                    dynamic memeObject = Newtonsoft.Json.JsonConvert.DeserializeObject(memeJson);

                    string title = memeObject.title;
                    string imageUrl = memeObject.url;

                    await ReplyAsync(imageUrl);
                }
                else
                {
                    await ReplyAsync("Sorry, I couldn't fetch a meme at the moment. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }
    }
    // End of the shitpost command

    // Cat command.
    [Command("cat")]
    public async Task Cat()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string apiUrl = "https://api.thecatapi.com/v1/images/search";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string catJson = await response.Content.ReadAsStringAsync();
                    dynamic catObject = Newtonsoft.Json.JsonConvert.DeserializeObject(catJson);

                    string imageUrl = catObject[0]?.url;

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        await ReplyAsync(imageUrl);
                    }
                    else
                    {
                        await ReplyAsync("Sorry, I couldn't fetch a cat image at the moment. Please try again later.");
                    }
                }
                else
                {
                    await ReplyAsync("Sorry, I couldn't fetch a cat image at the moment. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred: {ex.Message}");
            }
        }
    }
    // End of the cat command

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
    // End of the Credits command

    // Avatar command
    [Command("avatar")]
    public async Task AvatarCommand()
    {
        var user = Context.Message.Author.GetAvatarUrl();
        var avatarUrl = $"{user}";

        await Context.Channel.SendMessageAsync($"Your avatar: {avatarUrl}");
    }
    // End of the Avatar command

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
    // End of the Calculat command.

    // End of the misc commands

    // Mod Commands

    // Stuff needed to make these commands work.
    private string RemoveMention(string mention)
    {
        return mention.Replace("<@", "").Replace(">", "");
    }

    private RevoltClient Client;

    // Stats command
    [Command("stats")]
    public async Task GetStats()
    {
        try
        {
            IReadOnlyCollection<ServerMember> result1 = await Context.Server.GetMembersAsync();
            int num2 = result1.Count;
            IReadOnlyCollection<ServerMember> result2 = await Context.Server.GetMembersAsync(true);
            int num3 = result2.Count;
            await ReplyAsync($"### Stats:\n\n### Member count: {num2}\n### Online member count: {num3}\n");
        }
        catch (Exception ex)
        {
            await ReplyAsync($"An error occurred: {ex.Message}");
        }
    }

    // Ban Command
    [Command("ban")]
    public async Task BanUser([Remainder] string args)
    {
        try
        {
            string userId = RemoveMention(args);
            await Context.Server.BanMemberAsync(userId, "Banned via command.");
            await ReplyAsync("### Ban\nwhomp whomp.");
        }
        catch
        {
            await ReplyAsync("### ERROR\nInvalid Mention or Bot Permissions.");
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
            await ReplyAsync("### Unbanned\nVery sadly a mod had a change of heart.");
        }
        catch
        {
            await ReplyAsync("### ERROR\nInvalid Mention or Bot Permissions.");
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
            await ReplyAsync("### Kicked\nAnd he's gone.");
        }
        catch
        {
            await ReplyAsync("### ERROR\nInvalid Mention or Bot Permissions.");
        }
    }

    // End of mod commands

    // NSFW COMMANDS

    // R34 command.
    [Command("r34")]
    public async Task ExampleCommand([Remainder] string query = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await Context.Channel.SendMessageAsync("Please provide a search query. Usage: ?r34 {searchquery}");
                return;
            }

            if (!(Context.Channel is TextChannel textChannel) || !textChannel.IsNsfw)
            {
                await Context.Channel.SendMessageAsync("This command is only allowed in NSFW channels. So go to a NSFW channel to get your NSFW smh");
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
                    await Context.Channel.SendMessageAsync("Sorry, I couldn't fetch an image at the moment. Please try again later.");
                }
            }
        }
        catch (Exception ex)
        {
            await Context.Channel.SendMessageAsync($"An error occurred: {ex.Message}");
        }
    }

    // Hentai command.
    [Command("hentai")]
    public async Task Hentai()
    {
        try
        {
            if (!(Context.Channel is TextChannel textChannel) || !textChannel.IsNsfw)
            {
                await Context.Channel.SendMessageAsync("This command is only allowed in NSFW channels. So go to a NSFW channel to get your NSFW smh");
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
        catch (Exception ex)
        {
            await Context.Channel.SendMessageAsync($"An error occurred: {ex.Message}");
        }
    }
    // End of NSFW commands
    // End of the Bot
}