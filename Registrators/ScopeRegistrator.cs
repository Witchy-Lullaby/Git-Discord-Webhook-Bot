using LLM.GitHelper.Data.Git.Gitlab;
using LLM.GitHelper.Services.Parsers;
using LLM.GitHelper.Services.Discord;
using LLM.GitHelper.Services.Development;
using LLM.GitHelper.Services.Parsers.Implementation;

namespace LLM.GitHelper.Registrators
{
    public class ScopeRegistrator : IRegistrator
    {
        public Task Register(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IDebugger, Debugger>();
            builder.Services.AddScoped<IResponseParser<GitlabResponse>, GitlabResponseParser>(); //can be changed to any parser

            return Task.CompletedTask;
        }
    }
}
