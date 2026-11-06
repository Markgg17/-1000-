using System.Reflection;
namespace MarkggNexusDesktop;
internal static class AppConfig
{
 public const string AppId = "ColorForge";
 public const string Title = "Color Forge";
 public static string Html
 {
  get
  {
   using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AppHtml") ?? throw new InvalidOperationException("Missing embedded app");
   using var reader = new StreamReader(stream);
   return reader.ReadToEnd();
  }
 }
}
