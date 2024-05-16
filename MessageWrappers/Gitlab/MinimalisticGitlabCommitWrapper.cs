using DSharpPlus;
using LLM.GitHelper.Helpers;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Discord;
using LLM.GitHelper.Services.Parsers;

namespace LLM.GitHelper.MessageWrappers.Gitlab
{
    public class MinimalisticGitlabCommitWrapper : BaseGitMessageWrapper
    {
        private readonly UserLinkEstablisherService _establisherService;
        private readonly DiscordClient _client;
        private readonly IResponseParser<GitlabResponse> _parser;

        public const int MAX_LAST_COMMIT_LENGTH = 240;

        public MinimalisticGitlabCommitWrapper(string gitHelperType, DiscordClient client,
            IResponseParser<GitlabResponse> parse, UserLinkEstablisherService establisher,
            string[] keywords = null) : base(gitHelperType, keywords)
        {
            _client = client;
            _establisherService = establisher;
            _parser = parse;
        }

        protected override async Task<string> OnShow(GitlabResponse response)
        {
            string lastCommit = string.IsNullOrEmpty(response.MergeRequest.LastCommit.Message) ? response.ObjectAttributes.LastCommit.Message : response.MergeRequest.LastCommit.Message;
            if (string.IsNullOrEmpty(lastCommit)) lastCommit = string.Empty;
            else
            {
                var parsedLinksLastCommit = await _parser.ParseLinks(_client, lastCommit, _establisherService);
                lastCommit = $"\n\n>>> 🚩 Last commit: \n{parsedLinksLastCommit.Truncate(MAX_LAST_COMMIT_LENGTH)}";
            }

            var description = await _parser.ParseLinks(_client, response.ObjectAttributes.Description, _establisherService);
            string info = $"\n> 📦 __Source:__ ** {response.ObjectAttributes.SourceBranch} **\n> 🎯 __Target:__ ** {response.ObjectAttributes.TargetBranch} **\n `{description}` [✉](https://api.bunbun.cloud/?title=Commit%20Info&description={description.ToWebVariant() + lastCommit.ToWebVariant()}) ";
            
            return info;
        }
    }
}