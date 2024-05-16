
using DSharpPlus.Entities;
using LLM.GitHelper.Data.Database;
using LLM.GitHelper.Data.Git.Gitlab;

namespace LLM.GitHelper.Helpers
{
    public static class GitlabConverterHelper
    {
        public static string[] CreateIdentifiers(this GitlabResponse response)
        {
            return new string[4]
            {
                response.User.Username,
                response.User.Email,
                response.User.Name,
                response.ObjectAttributes.Note
            };
        }

        public static string ToWebVariant(this string text)
        {
            return text.Replace(" ", "%20");
        }

        public static string ToImage(this string resposeAction, GitlabResponse actionDiffer)
        {
            switch (resposeAction.ToLower())
            {
                case "":
                    return string.Empty;

                case Endpoints.GITLAB_MERGE_REQUEST_ATTRIBUTE:
                    return CheckMergeRequestState(actionDiffer.ObjectAttributes.State.ToLower(), actionDiffer.ObjectAttributes.Action);

                case Endpoints.GITLAB_COMMENT_ATTRIBUTE:
                    return CheckCommentAppliedTo(actionDiffer.ObjectAttributes.NoteableType.ToLower());

                default:
                    return "https://bunbun.cloud/admin/funkymonke/img/_drip_monkey_banner.gif";
            }
        }

        public static DiscordColor ToDiscordColor(this GitlabResponse response)
        {
            switch (response.ObjectKind.ToLower())
            {
                case "":
                    return DiscordColor.Magenta;

                case Endpoints.GITLAB_MERGE_REQUEST_ATTRIBUTE:
                    return CheckMergeRequestStateColor(response.ObjectAttributes.State.ToLower(), response.ObjectAttributes.Action);

                case Endpoints.GITLAB_COMMENT_ATTRIBUTE:
                    return new DiscordColor(64, 64, 64);

                default:
                    return DiscordColor.Black;
            }
        }

        public static string[] ToLookupKeys(this string objectKind, GitlabResponse response)
        {
            switch (objectKind.ToLower())
            {
                case Endpoints.GITLAB_COMMENT_ATTRIBUTE:
                    return new[]
                    {
                        response.ObjectAttributes.Title,
                        response.ObjectAttributes.SourceBranch,
                        response.MergeRequest.SourceBranch,
                        response.MergeRequest.Title,
                        response.ObjectAttributes.Source.Name,
                        response.ObjectAttributes.Note,
                        response.ObjectAttributes.LastCommit.Message
                    };

                case Endpoints.GITLAB_MERGE_REQUEST_ATTRIBUTE:
                default:
                    return new[]
                    {
                        response.ObjectAttributes.Title,
                        response.ObjectAttributes.SourceBranch,
                        response.ObjectAttributes.Action
                    };
            }
        }

        public static string[] ToLookupKeysLowered(this string objectKind, GitlabResponse response)
        {
            string[] keys = objectKind.ToLookupKeys(response);
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = keys[i].ToLower();
            }

            return keys;
        }

        public static string ToTitle(this string[] lookupKeys)
        {
            string title = string.Empty;
            for (int i = 0; i < lookupKeys.Length; i++)
            {
                title = lookupKeys[i];
                if (!string.IsNullOrEmpty(title)) return title;
            }

            return string.Empty;
        }

        private static string CheckCommentAppliedTo(string actionDiffer)
        {
            if (actionDiffer == Endpoints.GITLAB_COMMENT_PR_TYPE.ToLower()) return "https://bunbun.cloud/admin/funkymonke/img/prcommentking.png";
            else if (actionDiffer == Endpoints.GITLAB_COMMENT_COMMIT_TYPE.ToLower()) return "https://bunbun.cloud/admin/funkymonke/img/commitcommented.png";
            else return "https://bunbun.cloud/admin/funkymonke/img/_drip_monkey_banner.gif";
        }

        private static DiscordColor CheckMergeRequestStateColor(string actionDiffer, string prAction)
        {
            if (prAction != null && prAction.Length > 0 && prAction == "update") return DiscordColor.Yellow;
            if (actionDiffer == "opened") return new DiscordColor(115, 179, 255);
            else if (actionDiffer == "closed") return new DiscordColor(255, 121, 121);
            else return new DiscordColor(104, 255, 137);
        }

        private static string CheckMergeRequestState(string actionDiffer, string prAction)
        {
            if(prAction != null && prAction.Length > 0 && prAction == "update") return "https://bunbun.cloud/admin/funkymonke/img/prupdated.png";
            if (actionDiffer == "opened") return "https://bunbun.cloud/admin/funkymonke/img/prcreated.png";
            else if (actionDiffer == "closed") return "https://bunbun.cloud/admin/funkymonke/img/prclosed.png";
            else return "https://bunbun.cloud/admin/funkymonke/img/prmerged.png";
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string GetAuthorFromResponse(this GitlabResponse response)
        {
            string author = response.ObjectAttributes.LastCommit.Author.Name;
            if (string.IsNullOrEmpty(author)) author = response.MergeRequest.LastCommit.Author.Name;
            if (string.IsNullOrEmpty(author)) author = response.User.Name;

            return author;
        }
    }
}
