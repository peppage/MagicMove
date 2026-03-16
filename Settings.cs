namespace MagicMove;

public sealed class MoveEntry
{
    public string OriginalPath { get; set; } = "";
    public string MovedToPath { get; set; } = "";
    public DateTime MovedAt { get; set; }
}
