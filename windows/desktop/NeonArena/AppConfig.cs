using System.Reflection;
namespace MarkggNexusDesktop;
internal static class AppConfig
{
 public const string AppId = "NeonArena";
 public const string Title = "Neon Arena";
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
