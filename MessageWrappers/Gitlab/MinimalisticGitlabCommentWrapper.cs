using DSharpPlus;
using LLM.GitHelper.Helpers;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Discord;
using LLM.GitHelper.Services.Parsers;

namespace LLM.GitHelper.MessageWrappers.Gitlab
{
    public class MinimalisticGitlabCommentWrapper : BaseGitMessageWrapper
    {
        private readonly UserLinkEstablisherService _establisherService;
        private readonly DiscordClient _client;
        private readonly IResponseParser<GitlabResponse> _parser;

        public const int MAX_NOTE_LENGTH = 35;

        public MinimalisticGitlabCommentWrapper(string gitHelperType, DiscordClient client,
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

            var note = await _parser.ParseLinks(_client, response.ObjectAttributes.Note, _establisherService);
            return $"\n> **{response.User.Name}** commented: \n `{note.Truncate(MAX_NOTE_LENGTH)}` [✉](https://api.bunbun.cloud/?title={author.ToWebVariant()}&description={note.ToWebVariant()})";
        }
    }
}