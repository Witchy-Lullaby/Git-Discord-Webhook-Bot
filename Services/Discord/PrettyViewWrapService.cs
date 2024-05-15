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
    public class PrettyViewWrapService : IService
    {
        private readonly UserLinkEstablisherService _establisherService;
        private readonly DiscordClient _client;
        private readonly IResponseParser<GitlabResponse> _parser;

        private readonly BaseGitMessageWrapper[] _gitlabMessageWrappers;
        private readonly GitlabUnknownPushWrapper _unknownMessageWrapper;

        public PrettyViewWrapService(UserLinkEstablisherService establisherService,
            DiscordClient discordClient, IResponseParser<GitlabResponse> parser)
        {
            _establisherService = establisherService;
            _client = discordClient;
            _parser = parser;

            _gitlabMessageWrappers = new BaseGitMessageWrapper[] {
                new GitlabMergeRequestWrapper(Endpoints.GITLAB_MERGE_REQUEST_ATTRIBUTE, _client, _parser, _establisherService),
                new GitlabCommentWrapper(Endpoints.GITLAB_COMMENT_ATTRIBUTE, _client, _parser, _establisherService),
                new GitlabCommitWrapper(Endpoints.GITLAB_PUSH_ATTRIBUTE, _client, _parser, _establisherService)
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

            return new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor()
                    {
                        Name = response.User.Username,
                        IconUrl = avatar
                    },

                    ImageUrl = response.ObjectKind.ToImage(response),
                    Color = DiscordColor.Black,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                    {
                        Url = avatar,
                        Width = 25,
                        Height = 25
                    },

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


        [Obsolete("Old syntax, better use GetDescription(response, keywords, descriptor)")]
        private async Task<string> GetDescriptionBasedOnDescriptor(GitlabResponse response, string[] identifiers, string descriptor)
        {
            string author = response.ObjectAttributes.LastCommit.Author.Name;
            if (string.IsNullOrEmpty(author)) author = response.MergeRequest.LastCommit.Author.Name;
            if (string.IsNullOrEmpty(author)) author = response.User.Name;

            string lastCommit = string.IsNullOrEmpty(response.MergeRequest.LastCommit.Message) ? response.ObjectAttributes.LastCommit.Message : response.MergeRequest.LastCommit.Message;
            if (string.IsNullOrEmpty(lastCommit)) lastCommit = string.Empty;
            else
            {
                var parsedLinksLastCommit = await _parser.ParseLinks(_client, lastCommit, _establisherService);
                lastCommit = $"\n\n>>> 🚩 Last commit: {parsedLinksLastCommit}";
            }

            string info = string.Empty;
            if (descriptor == Endpoints.GITLAB_COMMENT_ATTRIBUTE)
            {
                var note = await _parser.ParseLinks(_client, response.ObjectAttributes.Note, _establisherService);
                info = $"✨ ** {response.Project.PathWithNamespace} ** ✨\n📌 __Author:__ ** {author} **\n\n> **{response.User.Name}** commented: \n `{note}`";
            }
            else
            {
                var description = await _parser.ParseLinks(_client, response.ObjectAttributes.Description, _establisherService);
                info = $"✨ ** {response.Project.PathWithNamespace} ** ✨\n📌 __Author:__ ** {author} **\n\n> 🎯 __Target:__ ** {response.ObjectAttributes.TargetBranch} **\n> 📦 __Source:__ ** {response.ObjectAttributes.SourceBranch} **\n `{description}` ";
            }

            info += lastCommit;

            Console.WriteLine($"[FULL PAYLOAD]\n{info}");
            return info;
        }
    }
}
