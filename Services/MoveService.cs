namespace MagicMove.Services;

public sealed class MoveService(ISettingsRepository settings) : IMoveService
{
    public IReadOnlyList<MoveEntry> GetEntries() => settings.GetEntries();

    public bool IsSymlink(string path)
    {
        try
        {
            var di = new DirectoryInfo(path);
            return di.Exists && di.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }
        catch
        {
            return false;
        }
    }

    public MoveEntry MoveFolder(string sourcePath, string destinationParent)
    {
        if (!Directory.Exists(sourcePath))
        {
            throw new InvalidOperationException("The selected folder does not exist.");
        }

        if (IsSymlink(sourcePath))
        {
            throw new InvalidOperationException("The selected folder is already a symbolic link.");
        }

        var folderName = Path.GetFileName(sourcePath);
        var destFolder = Path.Combine(destinationParent, folderName);

        if (Directory.Exists(destFolder))
        {
            throw new InvalidOperationException(
                $"A folder named \"{folderName}\" already exists at the destination."
            );
        }

        if (destinationParent.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot move a folder into itself.");
        }

        Directory.Move(sourcePath, destFolder);
        Directory.CreateSymbolicLink(sourcePath, destFolder);

        var entry = new MoveEntry
        {
            OriginalPath = sourcePath,
            MovedToPath = destFolder,
            MovedAt = DateTime.Now,
        };

        settings.AddEntry(entry);
        return entry;
    }

    public void RevertMove(MoveEntry entry)
    {
        var linkInfo = new DirectoryInfo(entry.OriginalPath);
        if (linkInfo.Exists && linkInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            linkInfo.Delete();
        }
        else if (Directory.Exists(entry.OriginalPath))
        {
            throw new InvalidOperationException(
                "The original path exists but is not a symbolic link. Revert aborted to prevent data loss."
            );
        }

        if (!Directory.Exists(entry.MovedToPath))
        {
            throw new InvalidOperationException(
                $"The moved folder no longer exists at:\n{entry.MovedToPath}"
            );
        }

        Directory.Move(entry.MovedToPath, entry.OriginalPath);
        settings.RemoveEntry(entry);
    }

    public void RemoveEntry(MoveEntry entry)
    {
        settings.RemoveEntry(entry);
    }
}
