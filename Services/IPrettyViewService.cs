using DSharpPlus.Entities;
using LLM.GitHelper.Data.Git.Gitlab;

namespace LLM.GitHelper.Services
{
    public interface IPrettyViewService
    {
        public Task<DiscordMessageBuilder> WrapResponseInEmbed(GitlabResponse response, string descriptor, string[] lookupKeys);
    }
}
