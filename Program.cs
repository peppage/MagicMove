using System.Security.Principal;
using MagicMove.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MagicMove;

static class Program
{
    [STAThread]
    static void Main()
    {
        if (!IsRunningAsAdmin())
        {
            MessageBox.Show(
                "MagicMove requires administrator privileges to create symbolic links.\n\nPlease right-click the application and select \"Run as administrator\".",
                "Administrator Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        var services = new ServiceCollection();
        services.AddSingleton<ISettingsRepository, JsonSettingsRepository>();
        services.AddSingleton<IMoveService, MoveService>();
        services.AddTransient<MainForm>();

        using var provider = services.BuildServiceProvider();

        ApplicationConfiguration.Initialize();
        Application.Run(provider.GetRequiredService<MainForm>());
    }

    private static bool IsRunningAsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
