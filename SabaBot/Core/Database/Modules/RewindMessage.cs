using System.ComponentModel.DataAnnotations;

namespace SabaBot.Database;

public class RewindMessage {
    [Key]
    public int Id { get; set; }
    
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
    
    public ulong AuthorId { get; set; }
}