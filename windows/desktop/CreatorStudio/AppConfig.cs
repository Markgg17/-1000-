using System.Reflection;
namespace MarkggNexusDesktop;
internal static class AppConfig
{
 public const string AppId = "CreatorStudio";
 public const string Title = "Creator Studio";
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
