namespace SabaBot.Database;

public class ReactionChampSettings {
    public bool Enabled { get; set; }
    public string? EmoteId { get; set; }
    public int ReactionThreshold { get; set; }
    public List<RewindMessage> DeletedMessages { get; set; } = new();
}