namespace MagicMove.Services;

public interface ISettingsRepository
{
    IReadOnlyList<MoveEntry> GetEntries();
    void AddEntry(MoveEntry entry);
    void RemoveEntry(MoveEntry entry);
}
