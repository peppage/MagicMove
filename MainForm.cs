using MagicMove.Services;

namespace MagicMove;

public sealed class MainForm : Form
{
    // Color palette
    private static readonly Color BgDark = Color.FromArgb(30, 30, 30);
    private static readonly Color BgPanel = Color.FromArgb(40, 40, 40);
    private static readonly Color BgCard = Color.FromArgb(50, 50, 50);
    private static readonly Color BgCardHover = Color.FromArgb(62, 62, 62);
    private static readonly Color BgCardSelected = Color.FromArgb(55, 70, 95);
    private static readonly Color Accent = Color.FromArgb(100, 140, 255);
    private static readonly Color AccentHover = Color.FromArgb(130, 165, 255);
    private static readonly Color TextPrimary = Color.FromArgb(230, 230, 230);
    private static readonly Color TextSecondary = Color.FromArgb(150, 150, 150);
    private static readonly Color DangerColor = Color.FromArgb(220, 80, 80);
    private static readonly Color DangerHover = Color.FromArgb(240, 110, 110);
    private static readonly Color SuccessColor = Color.FromArgb(80, 190, 120);

    private readonly IMoveService _moveService;
    private readonly Panel _listPanel;
    private readonly Label _emptyLabel;
    private int _selectedIndex = -1;

    public MainForm(IMoveService moveService)
    {
        _moveService = moveService;

        Text = "MagicMove";
        Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "icon.ico"));
        Size = new Size(800, 520);
        MinimumSize = new Size(650, 400);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BgDark;
        ForeColor = TextPrimary;
        Font = new Font("Segoe UI", 9.5f);
        DoubleBuffered = true;

        // Header
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = BgPanel,
            Padding = new Padding(24, 0, 24, 0),
        };

        var titleLabel = new Label
        {
            Text = "MagicMove",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = TextPrimary,
            AutoSize = true,
            Location = new Point(24, 12),
        };

        var subtitleLabel = new Label
        {
            Text = "Move folders and leave behind symbolic links",
            Font = new Font("Segoe UI", 9f),
            ForeColor = TextSecondary,
            AutoSize = true,
            Location = new Point(26, 42),
        };

        header.Controls.Add(titleLabel);
        header.Controls.Add(subtitleLabel);

        // Separator line under header
        var headerLine = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(60, 60, 60),
        };

        // Scrollable list area
        _listPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(24, 16, 24, 16),
            BackColor = BgDark,
        };

        _emptyLabel = new Label
        {
            Text = "No moved folders yet.\nClick \"Move Folder\" to get started.",
            ForeColor = TextSecondary,
            Font = new Font("Segoe UI", 11f),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
        };

        // Bottom button bar
        var bottomBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            BackColor = BgPanel,
            Padding = new Padding(24, 0, 24, 0),
        };

        var bottomLine = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = Color.FromArgb(60, 60, 60),
        };

        var btnMove = CreateStyledButton("Move Folder...", Accent, AccentHover);
        btnMove.Location = new Point(24, 16);
        btnMove.Click += BtnMove_Click;

        var btnRevert = CreateStyledButton("Revert Selected", DangerColor, DangerHover);
        btnRevert.Location = new Point(164, 16);
        btnRevert.Click += BtnRevert_Click;

        var btnRemove = CreateStyledButton(
            "Remove Entry",
            Color.FromArgb(90, 90, 90),
            Color.FromArgb(110, 110, 110)
        );
        btnRemove.Location = new Point(304, 16);
        btnRemove.Click += BtnRemoveEntry_Click;

        bottomBar.Controls.AddRange(btnMove, btnRevert, btnRemove);

        // Assemble — order matters for docking
        Controls.Add(_listPanel);
        Controls.Add(bottomLine);
        Controls.Add(bottomBar);
        Controls.Add(headerLine);
        Controls.Add(header);

        RefreshList();
    }

    private static Button CreateStyledButton(string text, Color bgColor, Color hoverColor)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(130, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = bgColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand,
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = hoverColor;
        btn.FlatAppearance.MouseDownBackColor = bgColor;
        return btn;
    }

    private void RefreshList()
    {
        _listPanel.Controls.Clear();
        _selectedIndex = -1;

        var entries = _moveService.GetEntries();
        if (entries.Count == 0)
        {
            _listPanel.Controls.Add(_emptyLabel);
            return;
        }

        // Build cards bottom-up so newest are at top with docking
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            var entry = entries[i];
            var card = CreateCard(entry, i);
            card.Dock = DockStyle.Top;
            _listPanel.Controls.Add(card);
        }
    }

    private Panel CreateCard(MoveEntry entry, int index)
    {
        var card = new Panel
        {
            Height = 80,
            Margin = new Padding(0, 0, 0, 8),
            BackColor = BgCard,
            Cursor = Cursors.Hand,
            Tag = index,
        };

        var linkHealthy = _moveService.IsSymlink(entry.OriginalPath);

        var statusDot = new Label
        {
            Text = "\u25CF",
            ForeColor = linkHealthy ? SuccessColor : DangerColor,
            Font = new Font("Segoe UI", 11f),
            AutoSize = true,
            Location = new Point(14, 14),
            BackColor = Color.Transparent,
        };

        var lblOriginal = new Label
        {
            Text = entry.OriginalPath,
            ForeColor = TextPrimary,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(36, 12),
            BackColor = Color.Transparent,
            MaximumSize = new Size(680, 0),
        };

        var lblDest = new Label
        {
            Text = $"\u2192  {entry.MovedToPath}",
            ForeColor = TextSecondary,
            Font = new Font("Segoe UI", 9f),
            AutoSize = true,
            Location = new Point(36, 34),
            BackColor = Color.Transparent,
            MaximumSize = new Size(680, 0),
        };

        var lblDate = new Label
        {
            Text = entry.MovedAt.ToString("yyyy-MM-dd  HH:mm"),
            ForeColor = Color.FromArgb(100, 100, 100),
            Font = new Font("Segoe UI", 8f),
            AutoSize = true,
            Location = new Point(36, 56),
            BackColor = Color.Transparent,
        };

        card.Controls.AddRange(statusDot, lblOriginal, lblDest, lblDate);

        // Hover + selection behavior
        void OnEnter(object? s, EventArgs e)
        {
            if (_selectedIndex != index)
            {
                card.BackColor = BgCardHover;
            }
        }
        void OnLeave(object? s, EventArgs e)
        {
            if (_selectedIndex != index)
            {
                card.BackColor = BgCard;
            }
        }
        void OnClick(object? s, EventArgs e)
        {
            // Deselect previous
            if (_selectedIndex >= 0)
            {
                foreach (Control c in _listPanel.Controls)
                {
                    if (c is Panel p)
                    {
                        p.BackColor = BgCard;
                    }
                }
            }

            _selectedIndex = index;
            card.BackColor = BgCardSelected;
        }

        card.MouseEnter += OnEnter;
        card.MouseLeave += OnLeave;
        card.MouseClick += OnClick;

        // Forward child events to parent card
        foreach (Control child in card.Controls)
        {
            child.MouseEnter += (_, _) => OnEnter(card, EventArgs.Empty);
            child.MouseLeave += (_, _) =>
            {
                var pos = card.PointToClient(Cursor.Position);
                if (!card.ClientRectangle.Contains(pos))
                {
                    OnLeave(card, EventArgs.Empty);
                }
            };
            child.MouseClick += (_, _) => OnClick(card, EventArgs.Empty);
        }

        return card;
    }

    private MoveEntry? GetSelectedEntry()
    {
        var entries = _moveService.GetEntries();
        if (_selectedIndex < 0 || _selectedIndex >= entries.Count)
        {
            return null;
        }

        return entries[_selectedIndex];
    }

    private void BtnMove_Click(object? sender, EventArgs e)
    {
        string? sourceFolder;
        using (var dlg = new FolderBrowserDialog { Description = "Select the folder to move" })
        {
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            sourceFolder = dlg.SelectedPath;
        }

        string? destParent;
        using (
            var dlg = new FolderBrowserDialog
            {
                Description = "Select the destination parent folder",
            }
        )
        {
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            destParent = dlg.SelectedPath;
        }

        try
        {
            Cursor = Cursors.WaitCursor;

            var entry = _moveService.MoveFolder(sourceFolder, destParent);
            RefreshList();

            MessageBox.Show(
                $"Folder moved successfully.\n\nFrom: {entry.OriginalPath}\nTo: {entry.MovedToPath}\n\nA symbolic link has been created at the original location.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to move folder: {ex.Message}");
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void BtnRevert_Click(object? sender, EventArgs e)
    {
        var entry = GetSelectedEntry();
        if (entry is null)
        {
            ShowError("Select an entry first.");
            return;
        }

        var result = MessageBox.Show(
            $"This will:\n\n1. Remove the symbolic link at:\n   {entry.OriginalPath}\n\n2. Move the folder back from:\n   {entry.MovedToPath}\n\nContinue?",
            "Confirm Revert",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;

            _moveService.RevertMove(entry);
            RefreshList();

            MessageBox.Show(
                "Folder reverted successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to revert: {ex.Message}");
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void BtnRemoveEntry_Click(object? sender, EventArgs e)
    {
        var entry = GetSelectedEntry();
        if (entry is null)
        {
            ShowError("Select an entry first.");
            return;
        }

        var result = MessageBox.Show(
            "Remove this entry from the list?\n\nThis does NOT move the folder back or remove the symbolic link.",
            "Remove Entry",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
        {
            return;
        }

        _moveService.RemoveEntry(entry);
        RefreshList();
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
