
using DSharpPlus;
using LLM.GitHelper.Data.Discord;
using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Discord;

namespace LLM.GitHelper.Services.Parsers.Implementation
{
    public class GitlabResponseParser : IService, IResponseParser<GitlabResponse>
    {
        public Task InitializeService() => Task.CompletedTask;

        public string[] ParsePrefixes(GitlabResponse response, string[] prefixes)
        {
            List<string> prefixesFound = new List<string>();
            string[] listToLookup =
            {
                response.ObjectAttributes.Title,
                response.ObjectAttributes.SourceBranch,
                response.ObjectAttributes.Action,
                response.ObjectAttributes.Description,
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

            string oldDescription = description;

            string[] divided = description.Split('@', ' ');

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

            if (string.IsNullOrEmpty(description) || description.Length <= 1) description = oldDescription;
            if(description.Contains('#')) description = description.Replace('#', ' '); //Removing headings

            return description;
        }

        public List<GitToDiscordLinkData> GetParsedLinks(DiscordClient client, string[] textToSeekUser, UserLinkEstablisherService establisher)
        {
            List<string> parsedContent = new List<string>();
            foreach (var description in textToSeekUser)
            {
                if (!description.Contains('@')) continue;
                parsedContent.Add(description);
            }

            List<GitToDiscordLinkData> parsedLinks = new List<GitToDiscordLinkData>();
            if (parsedContent.Count <= 0) return parsedLinks;

            foreach (var contentToParse in parsedContent)
            {
                string[] divided = contentToParse.Split('@', ' ');

                for (int i = 0; i < divided.Length; i++)
                {
                    var link = establisher.GetConnection(divided[i]);
                    if (link == null) continue;
                    parsedLinks.Add(link);
                }

                if (divided == null || divided.Length <= 0) continue;
            }

            return parsedLinks;
        }
    }
}
