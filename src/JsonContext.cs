using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarkPdf;

/// <summary>
/// JSON 序列化上下文（用于 Source Generator，支持裁剪）
/// </summary>
[JsonSerializable(typeof(AppConfig))]
[JsonSerializable(typeof(object))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true
)]
public partial class AppConfigJsonContext : JsonSerializerContext
{
}
