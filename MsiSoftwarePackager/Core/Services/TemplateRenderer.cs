namespace MsiSoftwarePackager.Core.Services;

public static class TemplateRenderer {
  public static string Render(
      string template,
      Dictionary<string,string> values) {
    string result = template;

    foreach (KeyValuePair<string,string> item in values) {
      result = result.Replace(
          "{{" + item.Key + "}}",
          item.Value
      );
    }

    return result;
  }
}