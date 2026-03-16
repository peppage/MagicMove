# MagicMove

A Windows GUI application that moves folders to a new location and leaves behind symbolic links at the original path. Useful for relocating large folders (e.g., games, project files) to another drive while keeping applications that reference the original path working seamlessly.

## Features

- **Move folders** to a new location with a symbolic link left at the original path
- **Revert moves** — removes the symlink and moves the folder back
- **Persistent tracking** — remembers all moved folders across sessions
- **Administrator enforcement** — UAC prompt ensures the app has the privileges needed to create symbolic links

## Requirements

- Windows 10 or later
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) (or the SDK to build from source)

## Building

```
cd MagicMove
dotnet build
```

## Running

```
cd MagicMove
dotnet run
```

When launched, Windows will display a UAC prompt requesting administrator privileges. This is required because creating symbolic links on Windows needs elevated permissions.

## Usage

1. Click **Move Folder** and select the folder you want to relocate.
2. Select the destination parent folder — the original folder will be moved inside it.
3. A symbolic link is created at the original location pointing to the new location.

To undo a move, select it in the list and click **Revert Selected**. This deletes the symlink and moves the folder back.

## How It Works

MagicMove performs two operations when moving a folder:

1. `Directory.Move(source, destination)` — relocates the folder
2. `Directory.CreateSymbolicLink(source, destination)` — creates a symlink at the original path

Any application referencing the original path will follow the symlink transparently.

Move records are stored in `%APPDATA%\MagicMove\settings.json`.

## License

MIT
