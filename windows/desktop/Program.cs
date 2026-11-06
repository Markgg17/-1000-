using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Windows.Forms;

namespace MarkggNexusDesktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new AppWindow());
    }
}

internal sealed class AppWindow : Form
{
    private readonly WebView2 view = new() { Dock = DockStyle.Fill };
    private readonly Label info = new() { Dock = DockStyle.Top, Height = 35, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, ForeColor = System.Drawing.Color.White, BackColor = System.Drawing.Color.FromArgb(19, 29, 53) };

    public AppWindow()
    {
        Text = AppConfig.Title + " — MARKGG NEXUS";
        Width = 1150;
        Height = 800;
        MinimumSize = new System.Drawing.Size(640, 480);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = System.Drawing.Color.FromArgb(8, 12, 25);
        info.Text = "MARKGG NEXUS  •  " + AppConfig.Title + "  •  локальная версия";
        Controls.Add(view);
        Controls.Add(info);
        Shown += async (_, _) => await LoadApp();
    }

    private async Task LoadApp()
    {
        try
        {
            var data = AppConfig.Html;
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MARKGG", AppConfig.AppId);
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, "index.html");
            await File.WriteAllTextAsync(path, data);
            var userData = Path.Combine(folder, "webview-profile");
            var env = await CoreWebView2Environment.CreateAsync(null, userData);
            await view.EnsureCoreWebView2Async(env);
            view.CoreWebView2.Settings.AreDevToolsEnabled = true;
            view.CoreWebView2.Settings.IsStatusBarEnabled = false;
            view.CoreWebView2.Navigate(new Uri(path).AbsoluteUri);
        }
        catch (Exception ex)
        {
            info.Text = "Ошибка запуска: " + ex.Message;
            MessageBox.Show("Для запуска требуется Microsoft Edge WebView2 Runtime.\n\n" + ex.Message,
                "MARKGG NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
