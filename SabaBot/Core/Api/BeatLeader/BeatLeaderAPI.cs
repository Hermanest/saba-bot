using System.Net;
using SabaBot.Models.BeatLeader;
using SabaBot.Utils;

namespace SabaBot;

internal class BeatLeaderAPI(HttpClient httpClient) : IBeatLeaderAPI {
    private const string HOST_URL = "https://api.beatleader.xyz";
    
    public async Task<ApiResponse<Player>> GetPlayerAsync(ulong id, IdProvider idProvider) {
        var provider = idProvider switch {
            IdProvider.Discord => "/discord",
            _ => ""
        };
        var url = $"{HOST_URL}/player{provider}/{id}";
        var response = await httpClient.GetAsync(url);
        return await response
            .ToResponse()
            .WithReason(HttpStatusCode.NotFound, "Player with such id does not exist. Make sure you have linked your BeatLeader account to Discord.")
            .WithReason(HttpStatusCode.RequestTimeout, "The server did not respond. Try again later.")
            .ToJsonAsync<Player>();
    }
}