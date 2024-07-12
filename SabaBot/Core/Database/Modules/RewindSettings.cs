namespace SabaBot.Database;

public class RewindSettings {
    public int MessagesThreshold { get; set; } = 1000;
    public bool EnableAutomaticReplies { get; set; } = true;
    public bool EnablePingReplies { get; set; } = true;
    public int AutomaticRepliesCooldown { get; set; } = 10;
    public int CooldownCounter { get; set; }
    public List<RewindMessage> Messages { get; set; } = new();
}
