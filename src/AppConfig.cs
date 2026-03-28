using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarkPdf;

/// <summary>
/// 应用程序配置
/// </summary>
public partial class AppConfig
{
    /// <summary>
    /// 默认编辑器
    /// </summary>
    [JsonPropertyName("editor")]
    public string? DefaultEditor { get; set; }

    /// <summary>
    /// 编辑书签时是否自动替换原文件（edit 命令）
    /// </summary>
    [JsonPropertyName("editReplace")]
    public bool EditReplace { get; set; }

    /// <summary>
    /// 导入书签时是否默认替换原文件
    /// </summary>
    [JsonPropertyName("importReplace")]
    public bool ImportReplace { get; set; }

    /// <summary>
    /// 导出文件的默认编码
    /// </summary>
    [JsonPropertyName("encoding")]
    public string Encoding { get; set; } = "utf-8";

    /// <summary>
    /// 导出文件时是否添加 BOM
    /// </summary>
    [JsonPropertyName("bom")]
    public bool IncludeBom { get; set; }

    /// <summary>
    /// 导出文件时默认添加的后缀
    /// </summary>
    [JsonPropertyName("exportSuffix")]
    public string? ExportSuffix { get; set; }

    /// <summary>
    /// 配置文件路径
    /// </summary>
    private static string ConfigPath => GetConfigFilePath();

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    private static string GetConfigFilePath()
    {
        var configDir = GetConfigDirectory();
        return Path.Combine(configDir, "config.json");
    }

    /// <summary>
    /// 获取配置目录
    /// </summary>
    private static string GetConfigDirectory()
    {
        // 统一使用 ~/.config/MarkPdf 目录（跨平台）
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var configDir = Path.Combine(home, ".config", "MarkPdf");

        // 确保目录存在
        Directory.CreateDirectory(configDir);
        return configDir;
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    public static AppConfig Load()
    {
        var configPath = ConfigPath;

        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize(json, AppConfigJsonContext.Default.AppConfig);
                if (config != null)
                {
                    // 应用智能默认值填充未设置的项
                    config.ApplyDefaults();
                    return config;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load config: {ex.Message}");
            }
        }

        // 返回带智能默认值的配置
        var defaultConfig = new AppConfig();
        defaultConfig.ApplyDefaults();
        return defaultConfig;
    }

    /// <summary>
    /// 应用智能默认值（用于填充配置中未设置的值）
    /// </summary>
    private void ApplyDefaults()
    {
        if (string.IsNullOrEmpty(DefaultEditor))
        {
            DefaultEditor = PlatformDefaults.GetRecommendedEditor();
        }

        if (Encoding == null)
        {
            Encoding = PlatformDefaults.GetRecommendedEncoding();
        }
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public void Save()
    {
        try
        {
            File.WriteAllText(ConfigPath, PlatformDefaults.GenerateDefaultConfig());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to save config: {ex.Message}");
        }
    }

    /// <summary>
    /// 初始化配置文件（如果不存在则创建默认配置）
    /// </summary>
    public static AppConfig Init()
    {
        var configPath = ConfigPath;

        if (!File.Exists(configPath))
        {
            // 创建带注释的配置文件
            File.WriteAllText(configPath, PlatformDefaults.GenerateDefaultConfig());
            Console.WriteLine($"Created default config at: {configPath}");
        }
        else
        {
            Console.WriteLine($"Config already exists at: {configPath}");
        }

        return Load();
    }

    /// <summary>
    /// 打开配置文件进行编辑
    /// </summary>
    public static void EditConfig()
    {
        var configPath = ConfigPath;

        // 确保配置文件存在
        if (!File.Exists(configPath))
        {
            Init();
        }

        // 打开编辑器编辑配置文件
        var editorInfo = PdfEditor.GetEditorInfo(null);
        using var process = PdfEditor.StartEditor(editorInfo, configPath);

        Console.WriteLine($"Editing config file: {configPath}");
        Console.WriteLine("Save and close the editor when done.");

        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            Console.WriteLine("Config updated.");
        }
    }

    /// <summary>
    /// 获取配置文件路径（供用户查看）
    /// </summary>
    public static string GetConfigPath()
    {
        return ConfigPath;
    }

    /// <summary>
    /// 获取当前有效的编辑器（考虑配置和自动检测）
    /// </summary>
    public string GetEffectiveEditor()
    {
        return DefaultEditor ?? PlatformDefaults.GetRecommendedEditor() ?? "notepad";
    }
}
