using SabaBot.Models.BeatLeader;

namespace SabaBot;

public interface IBeatLeaderAPI {
    Task<ApiResponse<Player>> GetPlayerAsync(ulong id, IdProvider idProvider);
}