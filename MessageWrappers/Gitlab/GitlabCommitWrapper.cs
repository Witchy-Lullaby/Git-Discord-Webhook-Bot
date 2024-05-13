using DSharpPlus;
using LLM.GitHelper.Helpers;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Discord;
using LLM.GitHelper.Services.Parsers;

namespace LLM.GitHelper.MessageWrappers.Gitlab
{
    public class GitlabCommitWrapper : BaseGitMessageWrapper
    {
        private readonly UserLinkEstablisherService _establisherService;
        private readonly DiscordClient _client;
        private readonly IResponseParser<GitlabResponse> _parser;

        public const int MAX_LAST_COMMIT_LENGTH = 80;

        public GitlabCommitWrapper(string gitHelperType, DiscordClient client,
            IResponseParser<GitlabResponse> parse, UserLinkEstablisherService establisher,
            string[] keywords = null) : base(gitHelperType, keywords)
        {
            _client = client;
            _establisherService = establisher;
            _parser = parse;
        }

        protected override async Task<string> OnShow(GitlabResponse response)
        {
            string author = response.GetAuthorFromResponse();

            var description = await _parser.ParseLinks(_client, response.ObjectAttributes.Description, _establisherService);
            string info = $"✨ ** {response.Project.PathWithNamespace} ** ✨\n📌 __Author:__ ** {author} **\n\n> 🎯 __Target:__ ** {response.ObjectAttributes.TargetBranch} **\n> 📦 __Source:__ ** {response.ObjectAttributes.SourceBranch} **\n `{description}` ";

            string lastCommit = string.IsNullOrEmpty(response.MergeRequest.LastCommit.Message) ? response.ObjectAttributes.LastCommit.Message : response.MergeRequest.LastCommit.Message;
            if (string.IsNullOrEmpty(lastCommit)) lastCommit = string.Empty;
            else
            {
                var parsedLinksLastCommit = await _parser.ParseLinks(_client, lastCommit, _establisherService);
                lastCommit = $"\n\n>>> 🚩 Last commit: {parsedLinksLastCommit.Truncate(MAX_LAST_COMMIT_LENGTH)}";
            }

            info += lastCommit;
            return info;
        }
    }
}