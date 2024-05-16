using DSharpPlus;
using LLM.GitHelper.Helpers;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Discord;
using LLM.GitHelper.Services.Parsers;

namespace LLM.GitHelper.MessageWrappers.Gitlab
{
    public class MinimalisticGitlabMergeRequestWrapper : BaseGitMessageWrapper
    {
        private readonly UserLinkEstablisherService _establisherService;
        private readonly DiscordClient _client;
        private readonly IResponseParser<GitlabResponse> _parser;

        public const int MAX_DESCRIPTION_LENGTH = 20;

        public MinimalisticGitlabMergeRequestWrapper(string gitHelperType, DiscordClient client,
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
            return $"\n> 📦 __Source:__ ** {response.ObjectAttributes.SourceBranch} **\n> 🎯 __Target:__ ** {response.ObjectAttributes.TargetBranch} **\n `{description.Truncate(MAX_DESCRIPTION_LENGTH)}` [✉](https://api.bunbun.cloud/?title={author.ToWebVariant()}&description={description.ToWebVariant()}) ";
        }
    }
}