using LLM.GitHelper.Data.Git.Gitlab;

namespace LLM.GitHelper.MessageWrappers
{
    public interface IGitMessageWrapper
    {
        public string GitHelperType { get; }
        public string[] GitHelperKeywords { get; }

        public Task<string> ShowAccordingToType(string type, GitlabResponse response);
        public Task<string> ShowAccordingToTypeOrKeywords(string type, string[] keywords, GitlabResponse response);
    }
}