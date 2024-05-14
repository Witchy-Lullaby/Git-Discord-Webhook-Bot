using DSharpPlus;
using DSharpPlus.Entities;
using LLM.GitHelper.Data.Discord;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Parsers;

namespace LLM.GitHelper.Services.Discord
{
    public class ThreadWatcherService : IService, IThreadWatcher
    {
        private readonly UserLinkEstablisherService _userLinkEstablisherService;
        private readonly DiscordClient _client;
        private readonly IResponseParser<GitlabResponse> _parser;

        public ThreadWatcherService(UserLinkEstablisherService userLinkEstablisherService,
            DiscordClient client, IResponseParser<GitlabResponse> parser)
        {
            _userLinkEstablisherService = userLinkEstablisherService;
            _client = client;
            _parser = parser;
        }

        public Task InitializeService() => Task.CompletedTask;

        public async Task CreateThread(DiscordChannel discordChannel, string title, DiscordMessageBuilder discordMessageBuilder, string[] identifiers)
        {
            var message = await discordMessageBuilder.SendAsync(discordChannel);
            var thread = await discordChannel.CreateThreadAsync(message, title, AutoArchiveDuration.ThreeDays);
            await AddNeededMembersToThread(thread, identifiers);
        }

        public async Task AddNeededMembersToThread(DiscordThreadChannel thread, string[] identifiers)
        {
            var additionalLinks = _parser.GetParsedLinks(_client, identifiers, _userLinkEstablisherService);

            var links = _userLinkEstablisherService.GetConnections(identifiers);

            if(additionalLinks.Count > 0) links.Concat(additionalLinks);
            await NotifyUsersInThread(additionalLinks, thread);
        }

        private async Task NotifyUsersInThread(List<GitToDiscordLinkData> links, DiscordThreadChannel thread)
        {
            string names = " ";
            string notifyNames = " ";
            foreach (var link in links)
            {
                var user = await _client.GetUserAsync(link.DiscordSnowflakeId);
                names += $"{link.GitUniqueIdentifier} ";
                notifyNames += $"@silent {user.Mention} ";
            }

            var message = await thread.SendMessageAsync(notifyNames);
            await message.ModifyAsync($"🎲 You've been auto-invited by identifier parsing: {names} ✨");
        }

        public async Task CreateThread(List<DiscordChannel> discordChannels, string title, DiscordMessageBuilder discordMessageBuilder, string[] identifiers)
        {
            foreach (var channel in discordChannels)
            {
                var message = await discordMessageBuilder.SendAsync(channel);
                var thread = await channel.CreateThreadAsync(message, title, AutoArchiveDuration.ThreeDays);
                await AddNeededMembersToThread(thread, identifiers);
            }
        }

        public bool IsThreadCreated(DiscordChannel discordChannel, string[] lookupKeys)
        {
            if (discordChannel.Threads == null || discordChannel.Threads.Count <= 0) return false;
            foreach (var thread in discordChannel.Threads)
            {
                if (lookupKeys.Contains($"WIP: {thread.Name}".ToLower()) || lookupKeys.Contains(thread.Name.ToLower())) return true;
            }

            return false;
        }

        public bool IsThreadCreated(List<DiscordChannel> discordChannels, string[] lookupKeys)
        {
            foreach (var channel in discordChannels)
            {
                if(IsThreadCreated(channel, lookupKeys)) return true;
            }

            return false;
        }

        public DiscordThreadChannel FindThread(DiscordChannel channel, string[] lookupKeys)
        {
            foreach (var thread in channel.Threads)
            {
                if (lookupKeys.Contains(thread.Name.ToLower()) || lookupKeys.Contains($"WIP: {thread.Name}".ToLower())) return thread;
            }

            return null;
        }

        public async Task Post(DiscordThreadChannel threadChannel, DiscordMessageBuilder threadedMessage) => await threadChannel.SendMessageAsync(threadedMessage);

        public async Task RemoveEveryone(DiscordThreadChannel threadChannel)
        {
            var members = await threadChannel.ListJoinedMembersAsync();
            foreach (var member in members)
            {
                await threadChannel.RemoveThreadMemberAsync(member.Member);
            }
        }
    }
}
