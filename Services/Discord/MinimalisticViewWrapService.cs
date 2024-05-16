using DSharpPlus;
using DSharpPlus.Entities;
using LLM.GitHelper.Helpers;
using LLM.GitHelper.Data.Database;
using LLM.GitHelper.MessageWrappers;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Parsers;
using LLM.GitHelper.MessageWrappers.Gitlab;

namespace LLM.GitHelper.Services.Discord
{
    public class MinimalisticViewWrapService : IService, IPrettyViewService
    {
        private readonly UserLinkEstablisherService _establisherService;
        private readonly DiscordClient _client;
        private readonly IResponseParser<GitlabResponse> _parser;

        private readonly BaseGitMessageWrapper[] _gitlabMessageWrappers;
        private readonly GitlabUnknownPushWrapper _unknownMessageWrapper;

        public MinimalisticViewWrapService(UserLinkEstablisherService establisherService,
            DiscordClient discordClient, IResponseParser<GitlabResponse> parser)
        {
            _establisherService = establisherService;
            _client = discordClient;
            _parser = parser;

            _gitlabMessageWrappers = new BaseGitMessageWrapper[] {
                new MinimalisticGitlabMergeRequestWrapper(Endpoints.GITLAB_MERGE_REQUEST_ATTRIBUTE, _client, _parser, _establisherService),
                new MinimalisticGitlabCommentWrapper(Endpoints.GITLAB_COMMENT_ATTRIBUTE, _client, _parser, _establisherService),
                new MinimalisticGitlabCommitWrapper(Endpoints.GITLAB_PUSH_ATTRIBUTE, _client, _parser, _establisherService)
            };

            _unknownMessageWrapper = new
            GitlabUnknownPushWrapper(
            Endpoints.GITLAB_PUSH_ATTRIBUTE,
            _client,
            _parser,
            _establisherService,
            new string[] { "push", "comment", "merge", "issue", "wiki", "fix", "hotfix", "bug", "commit" });
        }

        public Task InitializeService() => Task.CompletedTask;

        public async Task<DiscordMessageBuilder> WrapResponseInEmbed(GitlabResponse response, string descriptor, string[] lookupKeys)
        {
            string[] identifiers = response.CreateIdentifiers();
            string avatar = await CheckAvatarBasedOnLink(response, identifiers);
            string description = await GetDescription(response, identifiers, descriptor);
            DiscordColor color = response.ToDiscordColor();

            return new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor()
                    {
                        Name = response.User.Username,
                        IconUrl = avatar
                    },

                    Color = color,

                    Title = lookupKeys.ToTitle(), //response.ObjectAttributes.Title,
                    Description = description,
                    Url = response.ObjectAttributes.Url
                });
        }

        private async Task<string> CheckAvatarBasedOnLink(GitlabResponse response, string[] identifiers)
        {
            var connection = _establisherService.GetConnection(identifiers);
            if (connection == null) return response.User.AvatarUrl;
            else
            {
                var user = await _client.GetUserAsync(connection.DiscordSnowflakeId);
                return user.AvatarUrl;
            }
        }

        private async Task<string> GetDescription(GitlabResponse response, string[] identifiers, string descriptor)
        {
            string info = string.Empty;

            foreach (var helper in _gitlabMessageWrappers)
            {
                info = await helper.ShowAccordingToType(descriptor, response);
                if (string.IsNullOrEmpty(info)) continue;
                else break;
            }

            if (string.IsNullOrEmpty(info)) info = await _unknownMessageWrapper.ShowAccordingToTypeOrKeywords(info, identifiers, response);

            return info;
        }
    }
}
