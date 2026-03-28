using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarkPdf;

/// <summary>
/// 编辑器定义
/// </summary>
public class EditorDefinition
{
    /// <summary>
    /// 编辑器名称（用于显示）
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 命令名称
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// 额外参数
    /// </summary>
    public string[] Arguments { get; }

    /// <summary>
    /// 检测方式
    /// </summary>
    public Func<bool> Detector { get; }

    public EditorDefinition(string name, string command, string[]? arguments = null, Func<bool>? detector = null)
    {
        Name = name;
        Command = command;
        Arguments = arguments ?? Array.Empty<string>();

        if (detector != null)
        {
            Detector = detector;
        }
        else
        {
            // 默认检测器：检查命令是否可用
            Detector = () =>
            {
                try
                {
                    var process = new Process();
                    process.StartInfo.FileName = OperatingSystem.IsWindows() ? "where" : "which";
                    process.StartInfo.Arguments = command;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
                catch
                {
                    return false;
                }
            };
        }
    }

    /// <summary>
    /// 获取完整的命令字符串
    /// </summary>
    public string GetFullCommand()
    {
        if (Arguments.Length == 0)
        {
            return Command;
        }
        return $"{Command} {string.Join(" ", Arguments)}";
    }
}

/// <summary>
/// 平台默认设置检测
/// </summary>
public static class PlatformDefaults
{
    /// <summary>
    /// 编辑器优先级列表（按优先级从高到低）
    /// </summary>
    private static readonly List<EditorDefinition> EditorPriorityList = new()
    {
        // GUI 编辑器（高优先级，体验更好）
        new EditorDefinition("VS Code", "code", new[] { "--wait" }),
        new EditorDefinition("Sublime Text", "subl", new[] { "-w" }),
        new EditorDefinition("Cursor", "cursor", new[] { "--wait" }),
        new EditorDefinition("Zed", "zed", new[] { "--wait" }),
        new EditorDefinition("Fleet", "fleet", new[] { "--wait" }),
        new EditorDefinition("TextMate", "mate", new[] { "-w" }),
        new EditorDefinition("BBEdit", "bbedit"),
        
        // Windows 专用 GUI 编辑器
        new EditorDefinition("Notepad++", "notepad++", detector: () => IsWindowsAppInstalled("notepad++", "Notepad++", "notepad++.exe")),
        new EditorDefinition("Notepad2", "notepad2", detector: () => IsWindowsAppInstalled("notepad2", "Notepad2", "Notepad2.exe")),
        
        // 终端编辑器
        new EditorDefinition("Neovim", "nvim"),
        new EditorDefinition("Vim", "vim"),
        new EditorDefinition("GNU Nano", "nano"),
        new EditorDefinition("Vi", "vi"),
        
        // 平台默认（保底选项）
        new EditorDefinition("Notepad", "notepad", detector: () => OperatingSystem.IsWindows()),
        new EditorDefinition("TextEdit", "open", new[] { "-t", "-W", "-n" }, () => OperatingSystem.IsMacOS()),
    };

    /// <summary>
    /// 获取推荐的默认编辑器
    /// </summary>
    public static string? GetRecommendedEditor()
    {
        foreach (var editor in EditorPriorityList)
        {
            if (editor.Detector())
            {
                return editor.GetFullCommand();
            }
        }

        return null;
    }

    /// <summary>
    /// 获取所有可用编辑器列表（用于显示）
    /// </summary>
    public static List<EditorDefinition> GetAvailableEditors()
    {
        return EditorPriorityList.Where(e => e.Detector()).ToList();
    }

    /// <summary>
    /// 获取推荐的导出文件后缀
    /// </summary>
    public static string? GetRecommendedExportSuffix()
    {
        return null;
    }

    /// <summary>
    /// 获取推荐的编码
    /// </summary>
    public static string GetRecommendedEncoding()
    {
        return "utf-8";
    }

    /// <summary>
    /// 是否应该使用 BOM
    /// </summary>
    public static bool GetRecommendedBom()
    {
        return OperatingSystem.IsWindows();
    }

    /// <summary>
    /// edit 命令是否默认替换原文件
    /// </summary>
    public static bool GetRecommendedEditReplace()
    {
        return true;
    }

    /// <summary>
    /// import 命令是否默认替换原文件
    /// </summary>
    public static bool GetRecommendedImportReplace()
    {
        return false;
    }

    /// <summary>
    /// 检查命令是否可用
    /// </summary>
    private static bool IsCommandAvailable(string command)
    {
        try
        {
            var process = new Process();
            process.StartInfo.FileName = OperatingSystem.IsWindows() ? "where" : "which";
            process.StartInfo.Arguments = command;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检测 Windows 应用是否已安装
    /// </summary>
    private static bool IsWindowsAppInstalled(string commandName, string folderName, string exeName)
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        // 检查命令
        if (IsCommandAvailable(commandName))
        {
            return true;
        }

        // 检查常见安装路径
        var searchPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), folderName, exeName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), folderName, exeName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName, exeName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", folderName, exeName),
        };

        return searchPaths.Any(File.Exists);
    }

    /// <summary>
    /// 生成默认配置文件内容
    /// </summary>
    public static string GenerateDefaultConfig()
    {
        var config = new AppConfig
        {
            DefaultEditor = GetRecommendedEditor(),
            EditReplace = GetRecommendedEditReplace(),
            ImportReplace = GetRecommendedImportReplace(),
            Encoding = GetRecommendedEncoding(),
            IncludeBom = GetRecommendedBom(),
            ExportSuffix = GetRecommendedExportSuffix()
        };

        return JsonSerializer.Serialize(config, AppConfigJsonContext.Default.AppConfig);
    }
}
