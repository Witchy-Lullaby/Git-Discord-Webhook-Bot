using DSharpPlus.Entities;
using LLM.GitHelper.Services;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Discord;
using LLM.GitHelper.Data.Development;
using LLM.GitHelper.Services.Development;

namespace LLM.GitHelper.Helpers
{
    public class GitCatcherHelper : IService
    {
        private readonly ThreadWatcherService _threadWatcher;
        private readonly IDebugger _debugger;
        private readonly BroadcastDataService _broadcastDataService;

        public GitCatcherHelper(ThreadWatcherService threadWatcherService,
            BroadcastDataService broadcastDataService)
        {
            _threadWatcher = threadWatcherService;
            _debugger = new Debugger();
            _broadcastDataService = broadcastDataService;
        }

        public Task InitializeService() => Task.CompletedTask;

        public async Task ParsePrefixesAndCreateThread(List<DiscordChannel> channelsTracked, string[] loweredLookupKeys, string[] prefixesFound,
            string title, DiscordMessageBuilder threadedMessage, string[] identifiers, GitlabResponse response)
        {

            foreach (var channel in channelsTracked)
            {
                if (!_threadWatcher.IsThreadCreated(channel, loweredLookupKeys) || prefixesFound.Contains("all")) //response.ObjectAttributes.Title)
                {
                    await _threadWatcher.CreateThread(channel, title, threadedMessage, identifiers);
                    _debugger.Log($"Created a thread named: '{title}'.", new DebugOptions(this, "[THREAD CREATED]"));
                }
                else
                {
                    var threadChannel = _threadWatcher.FindThread(channel, loweredLookupKeys); //response.ObjectAttributes.Title);
                    if (threadChannel != null)
                    {
                        var state = response.ObjectAttributes.State;
                        await _threadWatcher.Post(threadChannel, threadedMessage);
                        if (state.Contains("closed") || state.Contains("merged")) await _threadWatcher.RemoveEveryone(threadChannel); //remove all on close
                    }
                    else _debugger.Log($"Couldn't find a thread '{title}'.", new DebugOptions(this, "[THREAD NOT FOUND]"));
                }
            }
        }

        public async Task ForceCreateThreads(string title, DiscordMessageBuilder threadedMessage, 
            string[] identifiers, string[] loweredLookupKeys)
        {
            var channels = _broadcastDataService.GetChannels(new string[] { "all" });
            foreach (var channel in channels)
            {
                var thread = _threadWatcher.FindThread(channel, loweredLookupKeys);
                if (thread == null)
                {
                    await _threadWatcher.CreateThread(channel, title, threadedMessage, identifiers);
                }
                else
                {
                    await _threadWatcher.Post(thread, threadedMessage);
                }
            }
        }
    }
}
