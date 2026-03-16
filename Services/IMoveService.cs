namespace MagicMove.Services;

public interface IMoveService
{
    MoveEntry MoveFolder(string sourcePath, string destinationParent);
    void RevertMove(MoveEntry entry);
    void RemoveEntry(MoveEntry entry);
    bool IsSymlink(string path);
    IReadOnlyList<MoveEntry> GetEntries();
}
