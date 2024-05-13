using LLM.GitHelper.Data.Git.Gitlab;

namespace LLM.GitHelper.MessageWrappers
{
    public abstract class BaseGitMessageWrapper : IGitMessageWrapper
    {
        public string GitHelperType { get; private set; }
        public string[] GitHelperKeywords { get; private set; }

        public BaseGitMessageWrapper(string gitHelperType, string[] gitHelperIdentifiers = null)
        {
            GitHelperType = gitHelperType.ToLower();

            if (gitHelperIdentifiers == null) return;

            for (int i = 0; i < gitHelperIdentifiers.Length; i++)
            {
                gitHelperIdentifiers[i] = gitHelperIdentifiers[i].ToLower();
            }

            GitHelperKeywords = gitHelperIdentifiers;
        }

        public virtual async Task<string> ShowAccordingToType(string type, GitlabResponse response)
        {
            if (type.ToLower() != GitHelperType) return string.Empty;

            return await OnShow(response);
        }

        public virtual async Task<string> ShowAccordingToTypeOrKeywords(string type, string[] identifiers, GitlabResponse response)
        {
            if (type.ToLower() == GitHelperType) return await OnShow(response);
            if (GitHelperKeywords == null) return string.Empty;

            foreach (string id in identifiers)
            {
                if (GitHelperKeywords.Contains(id)) return await OnShow(response);
            }
            return string.Empty;
        }

        protected abstract Task<string> OnShow(GitlabResponse text);
    }
}