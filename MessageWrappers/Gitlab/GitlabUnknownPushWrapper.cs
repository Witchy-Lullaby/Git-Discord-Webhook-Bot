using DSharpPlus;
using LLM.GitHelper.Helpers;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Discord;
using LLM.GitHelper.Services.Parsers;

namespace LLM.GitHelper.MessageWrappers.Gitlab
{
    public class GitlabUnknownPushWrapper : BaseGitMessageWrapper
    {
        private readonly UserLinkEstablisherService _establisherService;
        private readonly DiscordClient _client;
        private readonly IResponseParser<GitlabResponse> _parser;

        public GitlabUnknownPushWrapper(string gitHelperType, DiscordClient client,
            IResponseParser<GitlabResponse> parse, UserLinkEstablisherService establisher,
            string[] keywords = null) : base(gitHelperType, keywords)
        {
            _client = client;
            _establisherService = establisher;
            _parser = parse;
        }

        public override async Task<string> ShowAccordingToTypeOrKeywords(string type, string[] identifiers, GitlabResponse response)
        {
            string author = response.GetAuthorFromResponse();
            if (string.IsNullOrEmpty(author)) author = $"Author ID: {response.ObjectAttributes.AuthorId}";

            var description = await _parser.ParseLinks(_client, response.ObjectAttributes.Description, _establisherService);
            var descriptionNote = await _parser.ParseLinks(_client, response.ObjectAttributes.Note, _establisherService);
            string info = $"✨ ** {response.Project.PathWithNamespace} ** ✨\n📌 __Author:__ ** {author} **\n\n> 🎯 __Title:__ ** {response.ObjectAttributes.Title} **\n> 📦 __Action:__ ** {response.ObjectAttributes.Action} **\n> 📦 __Repo:__ ** {response.Repository.Name} \n `{description}`\n `{descriptionNote}`";

            return info;
        }

        protected override async Task<string> OnShow(GitlabResponse response)
        {
            return string.Empty;
        }
    }
}