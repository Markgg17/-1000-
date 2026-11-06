using System.Reflection;
namespace MarkggNexusDesktop;
internal static class AppConfig
{
 public const string AppId = "Print3DLab";
 public const string Title = "3D Print Lab";
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
