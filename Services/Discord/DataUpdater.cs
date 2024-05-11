using Newtonsoft.Json;
using LLM.GitHelper.Data;
using LLM.GitHelper.Data.Discord;
using LLM.GitHelper.Data.Database;

namespace LLM.GitHelper.Services.Discord
{
    public class DataUpdater
    {
        public static void UpdateBroadcastData(Dictionary<ulong, BroadcastData> broadcastData)
        {
            DataGrabber.CreateConfig(JsonConvert.SerializeObject(broadcastData), Endpoints.DISCORD_BROADCASTERS_CONFIG);
        }

        public static void UpdateEstablishedConnections(List<GitToDiscordLinkData> connections)
        {
            DataGrabber.CreateConfig(JsonConvert.SerializeObject(connections), Endpoints.ESTABLISHED_CONNECTIONS_CONFIG);
        }
    }
}
