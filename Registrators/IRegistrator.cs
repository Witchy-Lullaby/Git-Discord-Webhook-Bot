namespace LLM.GitHelper.Registrators
{
    public interface IRegistrator
    {
        public Task Register(WebApplicationBuilder builder);
    }
}
