using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("pause")]
        public async Task PauseAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                RainBorg.Paused = true;
                await Context.Message.Author.SendMessageAsync("Bot paused.");
                try
                {
                    RainBorg.Log("Command", "Paused by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("resume")]
        public async Task ResumeAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                RainBorg.Paused = false;
                await Context.Message.Author.SendMessageAsync("Bot resumed.");
                try
                {
                    RainBorg.Log("Command", "Resumed by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("adduser")]
        public async Task AddUserAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (SocketUser user in Context.Message.MentionedUsers)
                    try
                    {
                        if (user != null && RainBorg.UserPools.ContainsKey(Context.Channel.Id) && !RainBorg.UserPools[Context.Channel.Id].Contains(user.Id))
                        {
                            RainBorg.UserPools[Context.Channel.Id].Add(user.Id);
                            RainBorg.Log("Command", "{0} added to tip pool on channel {1} ({2}) by {3}", user.Id,
                                Context.Channel.Name, Context.Channel.Id, Context.User.Username);
                        }
                    }
                    catch { }
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("adduser")]
        public async Task AddUserAsync(params ulong[] users)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (ulong user in users)
                    try
                    {
                        if (RainBorg.UserPools.ContainsKey(Context.Channel.Id) && !RainBorg.UserPools[Context.Channel.Id].Contains(user))
                        {
                            RainBorg.UserPools[Context.Channel.Id].Add(user);
                            RainBorg.Log("Command", "{0} added to tip pool on channel {1} ({2}) by {3}", user,
                                Context.Channel.Name, Context.Channel.Id, Context.User.Username);
                        }
                    }
                    catch { }
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("removeuser")]
        public async Task RemoveUserAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (SocketUser user in Context.Message.MentionedUsers)
                    try
                    {
                        if (RainBorg.UserPools.ContainsKey(Context.Channel.Id) && RainBorg.UserPools[Context.Channel.Id].Contains(user.Id))
                        {
                            RainBorg.UserPools[Context.Channel.Id].Remove(user.Id);
                            RainBorg.Log("Command", "{0} removed from tip pool on channel {1} ({2}) by {3}", user.Id,
                                Context.Channel.Name, Context.Channel.Id, Context.User.Username);
                        }
                    }
                    catch { }
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("removeuser")]
        public async Task RemoveUserAsync(params ulong[] users)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (ulong user in users)
                    try
                    {
                        if (RainBorg.UserPools.ContainsKey(Context.Channel.Id) && RainBorg.UserPools[Context.Channel.Id].Contains(user))
                        {
                            RainBorg.UserPools[Context.Channel.Id].Remove(user);
                            RainBorg.Log("Command", "{0} removed from tip pool on channel {1} ({2}) by {3}", user,
                                Context.Channel.Name, Context.Channel.Id, Context.User.Username);
                        }
                    }
                    catch { }
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("reset")]
        public async Task ResetAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                foreach (KeyValuePair<ulong, List<ulong>> Entry in RainBorg.UserPools)
                    Entry.Value.Clear();
                RainBorg.Greylist.Clear();
                await Context.Message.Author.SendMessageAsync("User pools and greylist cleared.");
                try
                {
                    RainBorg.Log("Command", "All tip pools reset by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("dotip")]
        public async Task DoTipAsync([Remainder]string Remainder = null)
        {
            try { await Context.Message.DeleteAsync(); }
            catch { }
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                RainBorg.Waiting = RainBorg.waitTime;
                try
                {
                    RainBorg.Log("Command", "Manual tip called by {0}", Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("megatip")]
        public async Task DoMegaTipAsync(decimal Amount, [Remainder]string Remainder = null)
        {
            try { await Context.Message.DeleteAsync(); }
            catch { }
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                await RainBorg.MegaTipAsync(Amount);
                try
                {
                    RainBorg.Log("Command", "Megatip for {0} {1} called by {2}", RainBorg.Format(Amount), RainBorg.currencyName, Context.User.Username);

                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("exit")]
        public Task ExitAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                RainBorg.Log("Command", "Exited by {0}", Context.User.Username);

                RainBorg.ConsoleEventCallback(2);
                Environment.Exit(0);
            }
            return Task.CompletedTask;
        }

        /*[Command("say")]
        public async Task SaySpecAsync(ulong ChannelId, [Remainder]string Remainder = null)
        {
            try { await Context.Message.DeleteAsync(); }
            catch { }
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                if (!string.IsNullOrEmpty(Remainder))
                    await (Context.Client.GetChannel(ChannelId) as SocketTextChannel).SendMessageAsync(Remainder);
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }

        [Command("say")]
        public async Task SayAsync([Remainder]string Remainder = null)
        {
            try { await Context.Message.DeleteAsync(); }
            catch { }
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                if (!string.IsNullOrEmpty(Remainder)) foreach (ulong Channel in RainBorg.StatusChannel)
                        await (Context.Client.GetChannel(Channel) as SocketTextChannel).SendMessageAsync(Remainder);
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("👌"));
                }
            }
        }*/

        [Command("restart")]
        public void RestartAsync([Remainder]string Remainder = null)
        {
            if (Operators.ContainsKey(Context.Message.Author.Id))
            {
                RainBorg.Log("Command", "Restarted by {0}", Context.User.Username);

                RainBorg.Relaunch();
            }
        }
    }
}
