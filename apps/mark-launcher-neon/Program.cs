using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;

namespace MarkLauncherNeon;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LauncherWindow());
    }
}

internal sealed class LauncherSettings
{
    public string GameExe { get; set; } = "";
    public bool AlwaysOnTop { get; set; }
}

internal sealed class LauncherWindow : Form
{
    private static readonly Color Bg = Color.FromArgb(10, 12, 27);
    private static readonly Color Side = Color.FromArgb(17, 20, 41);
    private static readonly Color CardBg = Color.FromArgb(27, 31, 57);
    private static readonly Color TextColor = Color.FromArgb(238, 244, 255);
    private static readonly Color Muted = Color.FromArgb(159, 172, 201);
    private static readonly Color Cyan = Color.FromArgb(72, 218, 240);
    private static readonly Color Purple = Color.FromArgb(156, 118, 251);
    private readonly Panel page = new();
    private readonly Label status = new();
    private readonly Button[] navigation = new Button[4];
    private readonly string dataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MarkLauncherNeon");
    private readonly LauncherSettings settings;
    private int currentSection;
    private TextBox? astraInput;

    public LauncherWindow()
    {
        settings = LoadSettings();
        Text = "MARK LAUNCHER NEON";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1080, 680);
        MinimumSize = new Size(900, 580);
        BackColor = Bg;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 10);
        TopMost = settings.AlwaysOnTop;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 218));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(layout);

        var sidebar = new Panel { Dock = DockStyle.Fill, BackColor = Side };
        layout.Controls.Add(sidebar, 0, 0);
        Label(sidebar, "✦ MARKGG", 20, 23, 180, 35, 22, Cyan, true);
        Label(sidebar, "LAUNCHER NEON", 22, 59, 180, 26, 11, Muted, true);
        Label(sidebar, "РАЗДЕЛЫ", 20, 124, 180, 24, 9, Muted, true);

        string[] names = { "Играть", "Скачать", "Настройки", "Astra" };
        for (int i = 0; i < names.Length; i++)
        {
            int n = i;
            navigation[i] = Button(sidebar, names[i], 15, 155 + i * 58, 190, 46,
                () => ShowSection(n));
            navigation[i].TextAlign = ContentAlignment.MiddleLeft;
            navigation[i].Padding = new Padding(12, 0, 0, 0);
        }
        Label(sidebar, "v1.0 • Windows x64", 21, 510, 180, 27, 10, Muted);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.Controls.Add(right, 1, 0);

        var header = new Panel { Dock = DockStyle.Fill, BackColor = Bg };
        right.Controls.Add(header, 0, 0);
        Label(header, "MARK LAUNCHER NEON", 29, 21, 680, 36, 23, TextColor, true);
        Label(header, "Твой игровой центр • MARKGG", 30, 60, 680, 24, 10, Muted);

        page.Dock = DockStyle.Fill;
        page.BackColor = Bg;
        page.AutoScroll = true;
        right.Controls.Add(page, 0, 1);

        var footer = new Panel { Dock = DockStyle.Fill, BackColor = Side };
        right.Controls.Add(footer, 0, 2);
        status.Text = "●  Готов к работе";
        status.Location = new Point(30, 11);
        status.Size = new Size(700, 28);
        status.ForeColor = Cyan;
        footer.Controls.Add(status);
        Shown += (_, _) => ShowSection(0);
        Log("Лаунчер запущен.");
    }

    private static Label Label(Control host, string value, int x, int y, int w, int h,
        float size, Color color, bool bold = false)
    {
        var control = new Label
        {
            Text = value, Location = new Point(x, y), Size = new Size(w, h),
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = color, BackColor = Color.Transparent, AutoEllipsis = true
        };
        host.Controls.Add(control);
        return control;
    }

    private static Button Button(Control host, string value, int x, int y, int w, int h,
        Action action, bool primary = false)
    {
        var control = new Button
        {
            Text = value, Location = new Point(x, y), Size = new Size(w, h),
            BackColor = primary ? Cyan : CardBg, ForeColor = primary ? Bg : TextColor,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            UseVisualStyleBackColor = false
        };
        control.FlatAppearance.BorderColor = Color.FromArgb(70, 80, 112);
        control.FlatAppearance.BorderSize = primary ? 0 : 1;
        control.Click += (_, _) => action();
        host.Controls.Add(control);
        return control;
    }

    private Panel Card(int y, int height)
    {
        var control = new Panel
        {
            BackColor = CardBg,
            Location = new Point(28, y),
            Size = new Size(Math.Max(610, page.ClientSize.Width - 61), height),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        page.Controls.Add(control);
        return control;
    }

    private void ShowSection(int section)
    {
        currentSection = section;
        for (int i = 0; i < navigation.Length; i++)
        {
            navigation[i].BackColor = i == section ? Purple : CardBg;
            navigation[i].ForeColor = TextColor;
        }
        page.Controls.Clear();
        astraInput = null;
        string[] title = { "ИГРАТЬ", "СКАЧАТЬ", "НАСТРОЙКИ", "ASTRA" };
        string[] desc =
        {
            "Открой установленную игру или приложение.",
            "Безопасные ссылки на официальные сайты.",
            "Персональные параметры запуска.",
            "Локальный помощник с настоящими командами."
        };
        Label(page, title[section], 29, 13, 700, 46, 24, TextColor, true);
        Label(page, desc[section], 30, 64, 690, 28, 11, Muted);
        switch (section)
        {
            case 0: ShowPlay(); break;
            case 1: ShowDownloads(); break;
            case 2: ShowSettings(); break;
            case 3: ShowAstra(); break;
        }
    }

    private void ShowPlay()
    {
        var card = Card(115, 226);
        Label(card, "🎮  ТВОЯ ИГРА", 25, 20, 670, 32, 17, TextColor, true);
        Label(card, string.IsNullOrEmpty(settings.GameExe) ? "Игра ещё не выбрана"
            : Path.GetFileName(settings.GameExe), 25, 62, 650, 32, 16, Cyan, true);
        Label(card, string.IsNullOrEmpty(settings.GameExe)
            ? "Выбери установленный EXE-файл своей игры."
            : settings.GameExe, 25, 103, 650, 30, 10, Muted);
        Button(card, "▶  ИГРАТЬ", 24, 155, 165, 45, LaunchGame, true);
        Button(card, "Выбрать EXE", 199, 155, 170, 45, ChooseGame);
        Button(card, "Открыть папку", 380, 155, 172, 45, OpenGameFolder);

        var note = Card(357, 106);
        Label(note, "⚡ Настоящий запуск установленного приложения", 22, 17, 680, 30, 14, TextColor, true);
        Label(note, "Выбирай только проверенные EXE. Ничего не скачивается само.", 23, 56, 660, 26, 10, Muted);
    }

    private void ShowDownloads()
    {
        var card = Card(115, 225);
        Label(card, "⬇  ОФИЦИАЛЬНЫЕ ЗАГРУЗКИ", 24, 22, 680, 32, 17, TextColor, true);
        Label(card, "Кнопки откроют страницы разработчиков в браузере.", 24, 67, 670, 30, 11, Muted);
        Button(card, "Minecraft", 24, 118, 166, 45,
            () => OpenUrl("https://www.minecraft.net/download"), true);
        Button(card, "Xbox", 201, 118, 166, 45,
            () => OpenUrl("https://www.xbox.com/apps/xbox-app-on-pc"));
        Button(card, "MARKGG GitHub", 378, 118, 190, 45,
            () => OpenUrl("https://github.com/Markgg17/-1000-"));
        Label(card, "Установка и вход в аккаунт выполняются только на официальных сайтах.",
            24, 179, 650, 28, 10, Muted);
    }

    private void ShowSettings()
    {
        var card = Card(115, 270);
        Label(card, "⚙  НАСТРОЙКИ ИГРЫ", 24, 19, 670, 34, 17, TextColor, true);
        Label(card, "Выбранный EXE:", 24, 67, 650, 25, 10, Muted);
        Label(card, string.IsNullOrEmpty(settings.GameExe) ? "Пока не выбран"
            : settings.GameExe, 24, 95, 650, 33, 11, Cyan);
        Button(card, "Изменить EXE", 24, 142, 195, 44, ChooseGame, true);
        Button(card, "Убрать выбор", 231, 142, 175, 44, () =>
        {
            settings.GameExe = "";
            SaveSettings();
            ShowSection(2);
        });
        var check = new CheckBox
        {
            Text = "Поверх других окон", Checked = settings.AlwaysOnTop,
            Location = new Point(25, 215), AutoSize = true,
            ForeColor = TextColor, BackColor = CardBg
        };
        check.CheckedChanged += (_, _) =>
        {
            settings.AlwaysOnTop = check.Checked;
            TopMost = check.Checked;
            SaveSettings();
        };
        card.Controls.Add(check);
    }

    private void ShowAstra()
    {
        var card = Card(115, 290);
        Label(card, "✦  ASTRA / ЛОКАЛЬНЫЙ ПОМОЩНИК", 24, 18, 680, 36, 17, Cyan, true);
        Label(card, "Команды: играть, скачать, настройки, статус, журнал, помощь.",
            24, 67, 665, 30, 10, Muted);
        astraInput = new TextBox
        {
            Location = new Point(24, 110), Size = new Size(440, 36),
            BackColor = Bg, ForeColor = TextColor,
            BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 12),
            PlaceholderText = "Введи команду..."
        };
        astraInput.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; AstraCommand(); }
        };
        card.Controls.Add(astraInput);
        Button(card, "Выполнить", 475, 107, 166, 45, AstraCommand, true);
        Button(card, "Открыть журнал", 24, 179, 182, 45, OpenLog);
        Button(card, "Очистить журнал", 219, 179, 182, 45, ClearLog);
        Label(card, "Astra не является облачным ИИ и не выполняет произвольные команды системы.",
            24, 251, 655, 24, 10, Muted);
    }

    private void AstraCommand()
    {
        switch (astraInput?.Text.Trim().ToLowerInvariant() ?? "")
        {
            case "играть": LaunchGame(); break;
            case "скачать": ShowSection(1); break;
            case "настройки": ShowSection(2); break;
            case "статус":
                MessageBox.Show(string.IsNullOrEmpty(settings.GameExe)
                    ? "Игра пока не выбрана" : "Выбрано: " + settings.GameExe, "Astra");
                break;
            case "журнал": OpenLog(); break;
            case "помощь":
            case "":
                MessageBox.Show("Команды: играть, скачать, настройки, статус, журнал, помощь.", "Astra");
                break;
            default: MessageBox.Show("Неизвестная команда. Введи «помощь».", "Astra"); break;
        }
    }

    private void ChooseGame()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Выбери установленную игру", Filter = "Windows EXE (*.exe)|*.exe",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        settings.GameExe = dialog.FileName;
        SaveSettings();
        Log("Выбрана игра: " + Path.GetFileName(settings.GameExe));
        ShowSection(currentSection);
    }

    private void LaunchGame()
    {
        if (string.IsNullOrEmpty(settings.GameExe) || !File.Exists(settings.GameExe))
        {
            MessageBox.Show("Сначала выбери существующий EXE своей игры.", "Mark Launcher Neon");
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = settings.GameExe,
                WorkingDirectory = Path.GetDirectoryName(settings.GameExe)!,
                UseShellExecute = true
            });
            Log("Запущена игра: " + Path.GetFileName(settings.GameExe));
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка запуска"); }
    }

    private void OpenGameFolder()
    {
        if (string.IsNullOrEmpty(settings.GameExe) || !File.Exists(settings.GameExe))
        {
            MessageBox.Show("Сначала выбери игру.", "Mark Launcher Neon");
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe", Arguments = "/select,\"" + settings.GameExe + "\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Проводник"); }
    }

    private static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Браузер"); }
    }

    private LauncherSettings LoadSettings()
    {
        try
        {
            var path = Path.Combine(dataDir, "settings.json");
            return File.Exists(path)
                ? JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(path)) ?? new()
                : new();
        }
        catch { return new(); }
    }

    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(dataDir);
            File.WriteAllText(Path.Combine(dataDir, "settings.json"),
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            status.Text = "●  Настройки сохранены";
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Не удалось сохранить настройки"); }
    }

    private void Log(string message)
    {
        status.Text = "●  " + message;
        try
        {
            Directory.CreateDirectory(dataDir);
            File.AppendAllText(Path.Combine(dataDir, "launcher.log"),
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + Environment.NewLine);
        }
        catch { }
    }

    private void OpenLog()
    {
        try
        {
            Directory.CreateDirectory(dataDir);
            string file = Path.Combine(dataDir, "launcher.log");
            if (!File.Exists(file)) File.WriteAllText(file, "");
            Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Журнал"); }
    }

    private void ClearLog()
    {
        try
        {
            Directory.CreateDirectory(dataDir);
            File.WriteAllText(Path.Combine(dataDir, "launcher.log"), "");
            status.Text = "●  Журнал очищен";
            MessageBox.Show("Журнал очищен.", "Astra");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Журнал"); }
    }
}
