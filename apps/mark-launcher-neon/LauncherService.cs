using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;

namespace MarkLauncherNeon;

internal sealed class LauncherSettings
{
    public string GameExe { get; set; } = "";
    public string MinecraftDirectory { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
    public string SelectedVersion { get; set; } = "";
    public string OllamaModel { get; set; } = "";
    public int MemoryGb { get; set; } = 4;
    public bool AlwaysOnTop { get; set; }
}

internal sealed record MinecraftVersion(string Id, string Type)
{
    public override string ToString() => Id + "   •   " + Type;
}

internal sealed record ModInfo(string Name, string Loader)
{
    public override string ToString() => Name + "   [" + Loader + "]";
}

internal sealed class LauncherService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(150) };
    private const string ManifestUrl = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
    private const string OllamaUrl = "http://127.0.0.1:11434";
    public string DataDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MarkLauncherNeon");
    public LauncherSettings Settings { get; }
    public string ModsDirectory => Path.Combine(Settings.MinecraftDirectory, "mods");
    public string VersionsDirectory => Path.Combine(Settings.MinecraftDirectory, "versions");

    public LauncherService()
    {
        try
        {
            string path = Path.Combine(DataDir, "settings.json");
            Settings = File.Exists(path)
                ? JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(path)) ?? new LauncherSettings()
                : new LauncherSettings();
        }
        catch { Settings = new LauncherSettings(); }
        if (string.IsNullOrWhiteSpace(Settings.MinecraftDirectory))
            Settings.MinecraftDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
        Settings.MemoryGb = Math.Clamp(Settings.MemoryGb, 2, 16);
    }

    public void SaveSettings()
    {
        Directory.CreateDirectory(DataDir);
        File.WriteAllText(Path.Combine(DataDir, "settings.json"),
            JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(DataDir);
            File.AppendAllText(Path.Combine(DataDir, "launcher.log"),
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + Environment.NewLine);
        }
        catch { }
    }

    public async Task<IReadOnlyList<MinecraftVersion>> GetVersionsAsync()
    {
        string cache = Path.Combine(DataDir, "version-manifest.json");
        string json;
        try
        {
            json = await Http.GetStringAsync(ManifestUrl);
            Directory.CreateDirectory(DataDir);
            File.WriteAllText(cache, json);
        }
        catch when (File.Exists(cache))
        {
            json = await File.ReadAllTextAsync(cache);
        }

        using var doc = JsonDocument.Parse(json);
        var versions = new List<MinecraftVersion>();
        foreach (var entry in doc.RootElement.GetProperty("versions").EnumerateArray())
        {
            string id = entry.GetProperty("id").GetString() ?? "";
            string type = entry.GetProperty("type").GetString() ?? "";
            if (!string.IsNullOrWhiteSpace(id))
                versions.Add(new MinecraftVersion(id, type));
        }
        return versions;
    }

    public bool HasVersionMetadata(string version)
    {
        if (string.IsNullOrWhiteSpace(version) || version.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return false;
        return File.Exists(Path.Combine(VersionsDirectory, version, version + ".json"));
    }

    public string VersionHealth(string version)
    {
        if (!HasVersionMetadata(version))
            return "Этой версии нет в папке игры. Установи её в официальном лаунчере.";
        string folder = Path.Combine(VersionsDirectory, version);
        string jar = Path.Combine(folder, version + ".jar");
        if (File.Exists(jar))
        {
            try
            {
                using var archive = ZipFile.OpenRead(jar);
                return archive.Entries.Count > 0
                    ? "Локальные файлы версии обнаружены. JAR читается."
                    : "Внимание: JAR-файл пуст.";
            }
            catch { return "Внимание: JAR-файл повреждён или не читается."; }
        }
        return "Метаданные найдены, но отдельного JAR нет: возможно, это модифицированный профиль.";
    }

    public IReadOnlyList<ModInfo> GetMods()
    {
        if (!Directory.Exists(ModsDirectory)) return Array.Empty<ModInfo>();
        var items = new List<ModInfo>();
        foreach (var file in Directory.EnumerateFiles(ModsDirectory, "*.jar", SearchOption.TopDirectoryOnly))
        {
            string loader;
            try { loader = DetectLoader(file); } catch { loader = "повреждён"; }
            items.Add(new ModInfo(Path.GetFileName(file), loader));
        }
        return items.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static string DetectLoader(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var entries = archive.Entries.Select(x => x.FullName.Replace('\\', '/')).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (entries.Contains("fabric.mod.json")) return "Fabric";
        if (entries.Contains("quilt.mod.json")) return "Quilt";
        if (entries.Contains("META-INF/neoforge.mods.toml")) return "NeoForge";
        if (entries.Contains("META-INF/mods.toml")) return "Forge";
        if (entries.Contains("mcmod.info")) return "Forge/Legacy";
        return "неизвестный";
    }

    public string ImportMod(string sourceFile, bool overwrite)
    {
        if (!File.Exists(sourceFile) ||
            !string.Equals(Path.GetExtension(sourceFile), ".jar", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Для модов выбери файл .jar.");
        string loader = DetectLoader(sourceFile);
        Directory.CreateDirectory(ModsDirectory);
        string destination = Path.Combine(ModsDirectory, Path.GetFileName(sourceFile));
        if (Path.GetFullPath(sourceFile).Equals(Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
            return loader;
        File.Copy(sourceFile, destination, overwrite);
        Log("Добавлен мод: " + Path.GetFileName(sourceFile) + " / " + loader);
        return loader;
    }

    public string MoveModToDisabled(string modName)
    {
        if (!Path.GetFileName(modName).Equals(modName, StringComparison.Ordinal) ||
            !modName.EndsWith(".jar", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Неверное имя мода.");
        string source = Path.Combine(ModsDirectory, modName);
        if (!File.Exists(source)) throw new FileNotFoundException("Мод не найден.", source);
        string disabled = Path.Combine(Settings.MinecraftDirectory, "mods-disabled");
        Directory.CreateDirectory(disabled);
        string target = Path.Combine(disabled, DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + modName);
        File.Move(source, target);
        Log("Мод перемещён в mods-disabled: " + modName);
        return target;
    }

    public async Task<IReadOnlyList<string>> GetOllamaModelsAsync()
    {
        using var response = await Http.GetAsync(OllamaUrl + "/api/tags");
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("models").EnumerateArray()
            .Select(x => x.GetProperty("name").GetString() ?? "")
            .Where(x => x.Length > 0).Distinct().OrderBy(x => x).ToArray();
    }

    public async Task<string> ChatWithOllamaAsync(
        string model, IReadOnlyList<(string Role, string Text)> history)
    {
        if (string.IsNullOrWhiteSpace(model)) throw new InvalidOperationException("Не выбрана модель Ollama.");
        var messages = new List<object>
        {
            new { role = "system", content =
                "Ты Astra — дружелюбный русскоязычный помощник лаунчера MARK LAUNCHER NEON. " +
                "Помогай с Minecraft Java, версиями и модами. Не утверждай, что выполнил действия в системе: " +
                "у тебя нет инструментов для запуска игр, удаления файлов и установки программ. " +
                "Не проси пароли, токены и персональные данные." }
        };
        foreach (var item in history.TakeLast(16))
            messages.Add(new { role = item.Role, content = item.Text });
        using var response = await Http.PostAsJsonAsync(OllamaUrl + "/api/chat",
            new { model, messages, stream = false });
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("message").GetProperty("content").GetString()
            ?? "Модель вернула пустой ответ.";
    }
}
