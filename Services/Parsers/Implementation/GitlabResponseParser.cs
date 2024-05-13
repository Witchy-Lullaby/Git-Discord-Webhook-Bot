
using DSharpPlus;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Discord;

namespace LLM.GitHelper.Services.Parsers.Implementation
{
    public class GitlabResponseParser : IService, IResponseParser<GitlabResponse>
    {
        public const int MAX_LAST_COMMIT_LENGTH = 80;
        public Task InitializeService() => Task.CompletedTask;

        public string[] ParsePrefixes(GitlabResponse response, string[] prefixes)
        {
            List<string> prefixesFound = new List<string>();
            string[] listToLookup =
            {
                response.ObjectAttributes.Title,
                response.ObjectAttributes.SourceBranch,
                response.ObjectKind
            };

            foreach (string stringToLookup in listToLookup)
            {
                foreach (var prefix in prefixes)
                {
                    if (!stringToLookup.ToLower().Contains(prefix)) continue;
                    prefixesFound.Add(prefix);
                }
            }

            return prefixesFound.ToArray();
        }

        public async Task<string> ParseLinks(DiscordClient client, string description, UserLinkEstablisherService establisher)
        {
            if (!description.Contains('@')) return description;

            string[] divided = description.Split('@', ' ');

            foreach (var item in divided)
            {
                Console.WriteLine(item);
            }

            for (int i = 0; i < divided.Length; i++)
            {
                var link = establisher.GetConnection(divided[i]);
                if (link == null) continue;
                var user = await client.GetUserAsync(link.DiscordSnowflakeId);
                divided[i] = $"`{user.Mention}`";
            }

            description = string.Empty;
            foreach (var word in divided)
            {
                description += $"{word} ";
            }


            description = description.Replace('#', ' '); //Removing headings
            if (description.Length > MAX_LAST_COMMIT_LENGTH) description = description.Substring(0, MAX_LAST_COMMIT_LENGTH);

            return description;
        }
    }
}
