namespace SabaBot.Models.BeatLeader;

public class Player {
    public required string id;
    public required int rank;
    public required string name;
    public required string? avatar;
    public required string country;
    public required int countryRank;
    public required float pp;
    public required string role;
    public required Clan[] clans;
}