using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace MarkggNexus;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new NexusForm());
    }
}

internal sealed class NexusForm : Form
{
    const string Site = "https://markgg17.github.io/-1000-/";
    const string Repo = "https://github.com/Markgg17/-1000-";
    readonly FlowLayoutPanel cards = new();
    readonly Label status = new();
    readonly Label heading = new();
    readonly TextBox search = new();
    readonly List<(string Title, string Subtitle, string Path, string Icon)> projects = new()
    {
        ("NEON ARENA", "Аркадная игра с волнами и рекордами", "apps/neon-arena/", "🚀"),
        ("PIXEL STUDIO", "Пиксель-арт, кисть, заливка и PNG", "apps/pixel-studio/", "🎨"),
        ("CREATOR STUDIO", "Планирование роликов и резервные копии", "apps/creator-studio/", "🎬"),
        ("3D PRINT LAB", "Модели и экспорт STL", "apps/3d-lab/", "🧊"),
        ("MARK CODE LAB", "Редактор сайтов HTML, CSS, JavaScript", "apps/code-lab/", "💻"),
        ("COLOR FORGE", "Градиенты, палитры и экспорт SVG", "apps/color-forge/", "🌈")
    };
    readonly Color bg = Color.FromArgb(8, 13, 28);
    readonly Color surface = Color.FromArgb(18, 30, 54);
    readonly Color accent = Color.FromArgb(98, 231, 250);
    readonly Color muted = Color.FromArgb(164, 185, 209);

    public NexusForm()
    {
        Text = "MARKGG NEXUS — Windows Launcher";
        MinimumSize = new Size(780, 600);
        Size = new Size(1050, 760);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = bg;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.Dpi;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, Padding = new Padding(24, 18, 24, 16) };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 85));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        Controls.Add(layout);

        var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        heading.Text = "✦  MARKGG NEXUS";
        heading.Font = new Font("Segoe UI", 23, FontStyle.Bold);
        heading.ForeColor = accent;
        heading.Dock = DockStyle.Fill;
        heading.TextAlign = ContentAlignment.MiddleLeft;
        top.Controls.Add(heading, 0, 0);
        var links = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = true };
        links.Controls.Add(MakeButton("GitHub ↗", () => Open(Repo)));
        links.Controls.Add(MakeButton("Главный сайт ↗", () => Open(Site)));
        top.Controls.Add(links, 1, 0);
        layout.Controls.Add(top, 0, 0);

        var intro = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        intro.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        intro.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        var desc = new Label
        {
            Text = "PLAY · CREATE · IMAGINE\nШесть настоящих приложений в одном лаунчере.",
            Font = new Font("Segoe UI", 12),
            ForeColor = muted,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        intro.Controls.Add(desc, 0, 0);
        var find = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 22, 0, 22) };
        search.Dock = DockStyle.Fill;
        search.BackColor = surface;
        search.ForeColor = Color.White;
        search.BorderStyle = BorderStyle.FixedSingle;
        search.PlaceholderText = "Поиск приложений...";
        search.TextChanged += (_, _) => RenderCards();
        find.Controls.Add(search);
        intro.Controls.Add(find, 1, 0);
        layout.Controls.Add(intro, 0, 1);

        cards.Dock = DockStyle.Fill;
        cards.AutoScroll = true;
        cards.WrapContents = true;
        cards.Padding = new Padding(0, 5, 0, 5);
        cards.Resize += (_, _) => ResizeCards();
        layout.Controls.Add(cards, 0, 2);

        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        status.Text = "Готово · Windows 10/11 · Нужен интернет для запуска веб-приложений";
        status.ForeColor = muted;
        status.Dock = DockStyle.Fill;
        status.TextAlign = ContentAlignment.MiddleLeft;
        footer.Controls.Add(status, 0, 0);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        actions.Controls.Add(MakeButton("Проверить сайт", CheckSite));
        footer.Controls.Add(actions, 1, 0);
        layout.Controls.Add(footer, 0, 3);
        RenderCards();
    }

    Button MakeButton(string label, Action action)
    {
        var button = new Button
        {
            Text = label,
            AutoSize = true,
            MinimumSize = new Size(110, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = surface,
            ForeColor = accent,
            Margin = new Padding(5),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(49, 82, 111);
        button.Click += (_, _) => action();
        return button;
    }

    void RenderCards()
    {
        cards.SuspendLayout();
        cards.Controls.Clear();
        var query = search.Text.Trim();
        foreach (var project in projects.Where(p =>
            p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Subtitle.Contains(query, StringComparison.OrdinalIgnoreCase)))
        {
            var card = new Panel { Size = new Size(430, 145), BackColor = surface, Margin = new Padding(0, 0, 15, 15), Padding = new Padding(16) };
            var title = new Label { Text = project.Icon + "  " + project.Title, ForeColor = accent, Font = new Font("Segoe UI", 15, FontStyle.Bold), Dock = DockStyle.Top, Height = 40 };
            var subtitle = new Label { Text = project.Subtitle, ForeColor = muted, Dock = DockStyle.Top, Height = 42 };
            var button = MakeButton("Открыть приложение ↗", () => Open(Site + project.Path));
            button.Dock = DockStyle.Bottom;
            card.Controls.Add(button);
            card.Controls.Add(subtitle);
            card.Controls.Add(title);
            cards.Controls.Add(card);
        }
        cards.ResumeLayout();
        ResizeCards();
    }

    void ResizeCards()
    {
        int width = cards.ClientSize.Width - 20;
        int columns = width >= 750 ? 2 : 1;
        int cardWidth = Math.Max(300, (width - (columns - 1) * 15) / columns - 3);
        foreach (Control card in cards.Controls) card.Width = cardWidth;
    }

    void Open(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception e) { MessageBox.Show("Не удалось открыть браузер:\n" + e.Message, "MARKGG NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    async void CheckSite()
    {
        status.Text = "Проверяю публикацию сайта...";
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
            using var response = await client.GetAsync(Site);
            status.Text = response.IsSuccessStatusCode
                ? "✓ Главный сайт доступен: HTTP " + (int)response.StatusCode
                : "Сайт пока недоступен: HTTP " + (int)response.StatusCode + " · Проверь GitHub Pages";
        }
        catch (Exception e)
        {
            status.Text = "Не удалось проверить сайт: " + e.Message;
        }
    }
}
