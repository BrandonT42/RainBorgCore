using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RainBorg
{
    partial class RainBorg
    {
        // Initialization
        static void Main(string[] args)
        {
            // Vanity
            Console.WriteLine(Banner);

            // Create exit handler
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                handler = new ConsoleEventDelegate(ConsoleEventCallback);
                SetConsoleCtrlHandler(handler, true);
            }

            // Run bot
            Start();
        }

        // Main loop
        public static Task Start()
        {
            // Begin bot process in its own thread
            new Thread(delegate ()
            {
                new RainBorg().RunBotAsync().GetAwaiter().GetResult();
            }).Start();

            // Begin timeout loop in its own thread
            new Thread(delegate ()
            {
                UserTimeout();
            }).Start();

            // Get console commands
            string command = "";
            while (command.ToLower() != "exit")
            {
                // Get command
                command = Console.ReadLine();

                if (command.ToLower().StartsWith("dotip"))
                {
                    Waiting = waitTime;
                    Console.WriteLine("Tip sent.");
                }
                else if (command.ToLower().StartsWith("reset"))
                {
                    foreach (KeyValuePair<ulong, List<ulong>> Entry in UserPools)
                        Entry.Value.Clear();
                    Greylist.Clear();
                    Console.WriteLine("Pools reset.");
                }
                else if (command.ToLower().StartsWith("loglevel"))
                {
                    logLevel = int.Parse(command.Substring(command.IndexOf(' ')));
                    Config.Save();
                    Console.WriteLine("Log level changed.");
                }
                else if (command.ToLower().StartsWith("say"))
                {
                    foreach (ulong Channel in StatusChannel)
                        (_client.GetChannel(Channel) as SocketTextChannel).SendMessageAsync(command.Substring(command.IndexOf(' ')));
                    Console.WriteLine("Sent message.");
                }
                else if (command.ToLower().StartsWith("addoperator"))
                {
                    if (!Operators.ContainsKey(ulong.Parse(command.Substring(command.IndexOf(' ')))))
                        Operators.Add(ulong.Parse(command.Substring(command.IndexOf(' '))));
                    Console.WriteLine("Added operator.");
                }
                else if (command.ToLower().StartsWith("removeoperator"))
                {
                    if (Operators.ContainsKey(ulong.Parse(command.Substring(command.IndexOf(' ')))))
                        Operators.Remove(ulong.Parse(command.Substring(command.IndexOf(' '))));
                    Console.WriteLine("Removed operator.");
                }
                else if (command.ToLower().StartsWith("test"))
                {
                    Stats.Tip(DateTime.Now, 1, 1, 1000000);
                    Console.WriteLine("Added tip to database.");
                }
                else if (command.ToLower().StartsWith("restart"))
                {
                    Relaunch();
                }
            }

            // Completed, exit bot
            return Task.CompletedTask;
        }

        // Initiate bot
        public async Task RunBotAsync()
        {
            // Populate API variables
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            // Add event handlers
            _client.Log += Log;
            _client.Ready += Ready;

            // Load local files
            Log("RainBorg", "Loading config");
            await Config.Load();
            Log("RainBorg", "Loading database");
            Database.Load();

            // Register commands and start bot
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();

            // Resume if told to
            if (File.Exists(resumeFile))
            {
                Log("RainBorg", "Resuming bot...");
                JObject Resuming = JObject.Parse(File.ReadAllText(resumeFile));
                UserPools = Resuming["userPools"].ToObject<Dictionary<ulong, List<ulong>>>();
                Greylist = Resuming["greylist"].ToObject<List<ulong>>();
                UserMessages = Resuming["userMessages"].ToObject<Dictionary<ulong, UserMessage>>();
                File.Delete(resumeFile);
            }

            // Start tip cycle
            await DoTipAsync();

            // Rest infinitely
            await Task.Delay(-1);
        }

        // Ready event handler
        private Task Ready()
        {
            // Show start up message in all tippable channels
            if (Startup && entranceMessage != "")
            {
                _client.CurrentUser.ModifyAsync(m => { m.Username = _username; });
                foreach(ulong ChannelId in UserPools.Keys)
                    (_client.GetChannel(ChannelId) as SocketTextChannel).SendMessageAsync(entranceMessage);
                Startup = false;
            }

            // Developer ping
            /*if (developerDonations)
                foreach (IGuild Guild in _client.Guilds)
                    if (Guild.GetUserAsync(DID).Result == null)
                        foreach (ulong ChannelId in ChannelWeight.Distinct().ToList())
                            if (Guild.GetChannelAsync(ChannelId).Result != null)
                            {
                                try
                                {
                                    var channel = _client.GetChannel(ChannelId) as SocketGuildChannel;
                                    var invite = channel.CreateInviteAsync().Result;
                                    var owner = _client.GetUser(Guild.OwnerId);
                                    _client.GetUser(DID).SendMessageAsync(string.Format("Borg launched for \"{0}\" on server {1} (owned by {2}):\n{3}",
                                        currencyName, Guild.Name, owner.Username, invite));
                                }
                                catch { }
                                break;
                            }*/

            // Completed
            return Task.CompletedTask;
        }

        // Log event handler
        private Task Log(LogMessage arg)
        {
            // Write message to console
            Console.WriteLine(arg);

            // Relaunch if disconnected
            if (arg.Message.Contains("Disconnected")) Relaunch();

            // Completed
            return Task.CompletedTask;
        }

        // Register commands within API
        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += MessageReceivedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        // Message received
        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            // Get message and create a context
            var message = arg as SocketUserMessage;
            if (message == null) return;
            var context = new SocketCommandContext(_client, message);

            // Check if channel is a tippable channel
            if (UserPools.ContainsKey(message.Channel.Id) && !message.Author.IsBot)
            {
                // Check for spam
                await CheckForSpamAsync(message, out bool IsSpam);
                if (!IsSpam && !UserPools[message.Channel.Id].Contains(message.Author.Id))
                {
                    // Add user to tip pool
                    UserPools[message.Channel.Id].Add(message.Author.Id);
                    if (logLevel >= 1)
                        Log("Tipper", "Adding {0} ({1}) to user pool on channel #{2}", message.Author.Username, message.Author.Id, message.Channel);
                }

                // Remove users from pool if pool exceeds the threshold
                if (UserPools[message.Channel.Id].Count > userMax)
                    UserPools[message.Channel.Id].RemoveAt(0);
            }

            // Check if message is a commmand
            int argPos = 0;
            if (message.HasStringPrefix(botPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Execute command and log errors to console
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        // Tip loop
        public static async Task DoTipAsync()
        {
            Start:
            // If client is connected
            if (_client.ConnectionState == ConnectionState.Connected)
            {
                // Create a randomizer
                Random r = new Random();

                try
                {
                    // Get balance
                    tipBalance = GetBalance();

                    // Check tip balance against minimum tip
                    if (tipBalance - tipFee < tipMin && tipBalance >= 0)
                    {
                        // Log low balance message
                        Log("Tipper", "Balance does not meet minimum tip threshold.");

                        // Check if bot should show a donation message
                        if (ShowDonation)
                        {
                            // Create message
                            var builder = new EmbedBuilder();
                            builder.ImageUrl = donationImages[r.Next(0, donationImages.Count)];
                            builder.WithTitle("UH OH");
                            builder.WithColor(Color.Green);
                            builder.Description = String.Format(tipBalanceError, RainBorg.Format(tipMin + tipFee - tipBalance));

                            // Cast message to all status channels
                            foreach (ulong u in StatusChannel)
                                await (_client.GetChannel(u) as SocketTextChannel).SendMessageAsync("", false, builder);

                            // Reset donation message
                            ShowDonation = false;
                        }
                    }

                    // Grab eligible channels
                    List<ulong> Channels = EligibleChannels();

                    // No eligible channels
                    if (Channels.Count < 1) Log("Tipper", "No eligible tipping channels.");
                    else
                    {
                        // Roll for a megatip
                        if (r.NextDouble() * 100 <= megaTipChance)
                        {
                            // Do megatip
                            await MegaTipAsync(megaTipAmount);
                        }
                        else
                        {
                            // Roll until an eligible channel is chosen
                            ulong ChannelId = 0;
                            while (!Channels.Contains(ChannelId))
                                ChannelId = ChannelWeight[r.Next(0, ChannelWeight.Count)];

                            // Add developer donation
                            try
                            {
                                if (developerDonations && (_client.GetChannel(ChannelId) as SocketGuildChannel).GetUser(DID) != null)
                                {
                                    if (!UserPools[ChannelId].Contains(DID)) UserPools[ChannelId].Add(DID);
                                }
                            }
                            catch { }

                            // Check user count
                            if (tipBalance - tipFee < tipMin && UserPools[ChannelId].Count < userMin)
                                Log("Tipper", "Not enough users to meet threshold, will try again next tipping cycle.");

                            // Do a tip cycle
                            else if (tipBalance - tipFee >= tipMin && UserPools[ChannelId].Count >= userMin)
                            {
                                // Set tip amount
                                if (tipBalance - tipFee > tipMax)
                                    tipAmount = tipMax / UserPools[ChannelId].Count;
                                else tipAmount = (tipBalance - tipFee) / UserPools[ChannelId].Count;

                                // Round tip amount down
                                tipAmount = Floor(tipAmount);

                                // Begin creating tip message
                                int userCount = 0;
                                decimal tipTotal = 0;
                                DateTime tipTime = DateTime.Now;
                                Log("Tipper", "Sending tip of {0} to {1} users in channel #{2}", RainBorg.Format(tipAmount),
                                    UserPools[ChannelId].Count, _client.GetChannel(ChannelId));
                                string m = $"{RainBorg.tipPrefix}tip " + RainBorg.Format(tipAmount) + " ";

                                // Loop through user pool and add them to tip
                                for (int i = 0; i < UserPools[ChannelId].Count; i++)
                                {
                                    try
                                    {
                                        // Make sure the message size is below the max discord message size
                                        if ((m + _client.GetUser(UserPools[ChannelId][i]).Mention + " ").Length <= 2000)
                                        {
                                            // Add a username mention
                                            m += _client.GetUser(UserPools[ChannelId][i]).Mention + " ";

                                            // Increment user count
                                            userCount++;

                                            // Add to tip total
                                            tipTotal += tipAmount;

                                            // Add tip to stats
                                            try
                                            {
                                                await Stats.Tip(tipTime, ChannelId, UserPools[ChannelId][i], tipAmount);
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine("Error adding tip to stat sheet: " + e.Message);
                                            }
                                        }
                                    }
                                    catch { }
                                }

                                // Send tip message to channel
                                await (_client.GetChannel(ChannelId) as SocketTextChannel).SendMessageAsync(m);

                                // Begin building status message
                                var builder = new EmbedBuilder();
                                builder.WithTitle("TUT TUT");
                                builder.ImageUrl = statusImages[r.Next(0, statusImages.Count)];
                                builder.Description = "Huzzah, " + RainBorg.Format(tipTotal) + " " + currencyName + " just rained on " + userCount +
                                    " chatty user";
                                if (UserPools[ChannelId].Count > 1) builder.Description += "s";
                                builder.Description += " in #" + _client.GetChannel(ChannelId) + ", they ";
                                if (UserPools[ChannelId].Count > 1) builder.Description += "each ";
                                builder.Description += "got " + RainBorg.Format(tipAmount) + " " + currencyName + "!";
                                builder.WithColor(Color.Green);

                                // Send status message to all status channels
                                foreach (ulong u in StatusChannel)
                                    await (_client.GetChannel(u) as SocketTextChannel).SendMessageAsync("", false, builder);

                                // Clear user pool
                                if (flushPools) UserPools[ChannelId].Clear();
                                Greylist.Clear();
                                ShowDonation = true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error sending tip: " + e);
                }

                // Calculate wait time until next tip
                if (waitMin < waitMax)
                    waitTime = r.Next(waitMin, waitMax);
                else waitTime = 10 * 60 * 1000;
                waitNext = DateTime.Now.AddSeconds(waitTime).ToString("HH:mm:ss") + " " + _timezone;
                Log("Tipper", "Next tip in {0} seconds({1})", waitTime, waitNext);

                // Wait for X seconds
                Waiting = 0;
                while (Waiting < waitTime || Paused)
                {
                    if (Waiting < waitTime)
                    {
                        await Task.Delay(1000);
                        Waiting += 1;
                    }
                }
            }

            // Restart tip loop
            goto Start;
        }

        // Remove expired users from userpools
        private static async void UserTimeout()
        {
            while (true)
            {
                try
                {
                    // Do not run loop while paused
                    while (Paused) { }

                    // Create a deletion buffer
                    Dictionary<ulong, List<ulong>> Temp = new Dictionary<ulong, List<ulong>>();

                    // Loop through pools and check for timeout
                    foreach (KeyValuePair<ulong, List<ulong>> UserPool in UserPools)
                    {
                        // Iterate over users within pool
                        List<ulong> Pool = new List<ulong>();
                        for (int i = 0; i < UserPool.Value.Count; i++)
                        {
                            // Check if their last message was created beyond the timeout period
                            if (DateTimeOffset.Now.ToUnixTimeSeconds() - UserMessages[UserPool.Value[i]].CreatedAt.ToUnixTimeSeconds() > timeoutPeriod)
                            {
                                if (logLevel >= 3)
                                    Console.WriteLine("{0} {1}     Checking {2} against {3} on channel #{4}", DateTime.Now.ToString("HH:mm:ss"), "Timeout",
                                        UserMessages[UserPool.Value[i]].CreatedAt.ToUnixTimeSeconds(), DateTimeOffset.Now.ToUnixTimeSeconds(), _client.GetChannel(UserPool.Key));

                                // Remove user from channel's pool
                                if (logLevel >= 1)
                                    Console.WriteLine("{0} {1}     Removed {2} ({3}) from user pool on channel #{4}", DateTime.Now.ToString("HH:mm:ss"), "Timeout",
                                        _client.GetUser(UserPool.Value[i]), UserPool.Value[i], _client.GetChannel(UserPool.Key));
                                //await RemoveUserAsync(_client.GetUser(Pool[i]), UserPool.Key);
                                Pool.Add(UserPool.Value[i]);
                            }
                        }
                        Temp.Add(UserPool.Key, Pool);
                    }

                    // Iterate over all channel pools
                    foreach (KeyValuePair<ulong, List<ulong>> UserPool in Temp)
                        for (int i = 0; i < UserPool.Value.Count; i++)
                            await RemoveUserAsync(_client.GetUser(UserPool.Value[i]), UserPool.Key);
                }
                catch { }

                // Wait
                await Task.Delay(1000);
            }
        }

        // Megatip
        public static Task MegaTipAsync(decimal amount)
        {
            Log("RainBorg", "Megatip called");

            // Get balance
            tipBalance = GetBalance();

            // Check that tip amount is within bounds
            if (amount + (tipFee * UserPools.Keys.Count) > tipBalance && tipBalance >= 0)
            {
                Log("RainBorg", "Insufficient balance for megatip, need {0}", RainBorg.Format(tipBalance + (tipFee * UserPools.Keys.Count)));
                // Insufficient balance
                return Task.CompletedTask;
            }

            // Get total user amount
            int TotalUsers = 0;
            foreach (List<ulong> List in UserPools.Values)
                foreach (ulong User in List)
                    TotalUsers++;

            // Set tip amount
            tipAmount = amount / TotalUsers;
            tipAmount = Floor(tipAmount);

            // Loop through user pools and add them to tip
            decimal tipTotal = 0;
            DateTime tipTime = DateTime.Now;
            foreach (ulong ChannelId in UserPools.Keys)
            {
                if (UserPools[ChannelId].Count > 0)
                {
                    string m = $"{RainBorg.tipPrefix}tip " + RainBorg.Format(tipAmount) + " ";
                    for (int i = 0; i < UserPools[ChannelId].Count; i++)
                    {
                        try
                        {
                            // Make sure the message size is below the max discord message size
                            if ((m + _client.GetUser(UserPools[ChannelId][i]).Mention + " ").Length <= 2000)
                            {
                                // Add a username mention
                                m += _client.GetUser(UserPools[ChannelId][i]).Mention + " ";

                                // Add to tip total
                                tipTotal += tipAmount;

                                // Add tip to stats
                                try
                                {
                                    Stats.Tip(tipTime, ChannelId, UserPools[ChannelId][i], tipAmount);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error adding tip to stat sheet: " + e.Message);
                                }
                            }
                        }
                        catch { }
                    }

                    // Send tip message to channel
                    (_client.GetChannel(ChannelId) as SocketTextChannel).SendMessageAsync(m);

                    // Clear list
                    if (flushPools) UserPools[ChannelId].Clear();
                }
            }

            // Clear greylist
            Greylist.Clear();
            
            // Begin building status message
            var builder = new EmbedBuilder();
            builder.WithTitle("TUT TUT");
            builder.ImageUrl = statusImages[new Random().Next(0, statusImages.Count)];
            builder.Description = "Wow, a megatip! " + RainBorg.Format(tipTotal) + " " + currencyName + " just rained on " + TotalUsers + " chatty user";
            if (TotalUsers > 1) builder.Description += "s";
            builder.Description += ", they ";
            if (TotalUsers > 1) builder.Description += "each ";
            builder.Description += "got " + RainBorg.Format(tipAmount) + " " + currencyName + "!";
            builder.WithColor(Color.Green);

            // Send status message to all status channels
            foreach (ulong u in StatusChannel)
                (_client.GetChannel(u) as SocketTextChannel).SendMessageAsync("", false, builder);

            // Completed
            return Task.CompletedTask;
        }
    }
}
