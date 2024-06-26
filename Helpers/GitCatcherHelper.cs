﻿using DSharpPlus.Entities;
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
                    var threadChannel = _threadWatcher.FindThread(channel, loweredLookupKeys); //find already created thread with these keywords
                    if (threadChannel != null)
                    {
                        var state = response.ObjectAttributes.State;
                        await _threadWatcher.AddNeededMembersToThread(threadChannel, identifiers); //if someone was mentioned add them to thread
                        await _threadWatcher.Post(threadChannel, threadedMessage); //post to thread
                        if (state.Contains("closed") || state.Contains("merged"))
                        {
                            await ClearRelatedMessages(threadChannel, channel, title, threadedMessage); 
                            //await _threadWatcher.RemoveEveryone(threadChannel); //remove all on close
                        }
                    }
                    else _debugger.Log($"Couldn't find a thread '{title}'.", new DebugOptions(this, "[THREAD NOT FOUND]"));
                }
            }
        }

        private async Task ClearRelatedMessages(DiscordThreadChannel threadChannel, DiscordChannel channel, string title, DiscordMessageBuilder messageToRedactTo)
        {
            await threadChannel.DeleteAsync();
            var messages = await channel.GetMessagesAsync();
            foreach (var message in messages)
            {
                if (!message.Content.Contains(title)) continue;
                await message.ModifyAsync(messageToRedactTo);
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
