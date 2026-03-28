using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MarkPdf;

/// <summary>
/// 跨平台 PDF 书签编辑器
/// </summary>
public static class PdfEditor
{
    /// <summary>
    /// 编辑 PDF 书签
    /// </summary>
    /// <param name="pdfPath">PDF 文件路径</param>
    /// <param name="initialContent">初始书签内容</param>
    /// <param name="editor">指定编辑器（可选）</param>
    /// <returns>编辑后的内容，如果编辑取消则返回 null</returns>
    public static string? EditBookmarks(string pdfPath, string initialContent, string? editor = null)
    {
        // 创建临时文件
        var tempFile = Path.Combine(Path.GetTempPath(), $"markpdf_edit_{Guid.NewGuid()}.txt");
        File.WriteAllText(tempFile, initialContent);

        Console.WriteLine($"Temp file created: {tempFile}");

        try
        {
            // 启动编辑器
            var editorInfo = GetEditorInfo(editor);
            Console.WriteLine($"Using editor: {editorInfo.Name}");

            using var process = StartEditor(editorInfo, tempFile);

            Console.WriteLine($"Editing bookmarks for: {pdfPath}");
            Console.WriteLine("Waiting for editor to close...");
            Console.WriteLine("(Please save and close the editor to continue)");

            // 等待编辑器关闭
            process.WaitForExit();
            Console.WriteLine($"Editor exited with code: {process.ExitCode}");

            // 读取编辑后的内容
            var editedContent = File.ReadAllText(tempFile);

            // 检查内容是否改变
            if (editedContent.Trim() == initialContent.Trim())
            {
                Console.WriteLine("No changes detected. Aborting.");
                return null;
            }

            return editedContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during editing: {ex.Message}");
            Console.WriteLine("You can manually edit the file:");
            Console.WriteLine($"  {tempFile}");
            Console.WriteLine("Press Enter when you're done editing...");
            Console.ReadLine();

            // 尝试再次读取
            if (File.Exists(tempFile))
            {
                return File.ReadAllText(tempFile);
            }
            return null;
        }
        finally
        {
            // 清理临时文件
            TryDelete(tempFile);
        }
    }

    /// <summary>
    /// 已知 GUI 编辑器列表（小写）
    /// </summary>
    private static readonly HashSet<string> GuiEditors = new(StringComparer.OrdinalIgnoreCase)
    {
        "code", "code-insiders", "code-oss", "code-server",
        "cursor",
        "zed",
        "fleet",
        "subl", "sublime_text",
        "atom",
        "notepad++", "notepad++64",
        "notepad2", "notepad3",
        "textmate", "mate",
        "bbedit", "bbedit",
        "textwrangler",
        "coteditor",
        "macvim", "mvim",
        "notepad",
        "wordpad",
        "open"
    };

    /// <summary>
    /// 解析编辑器命令，分离命令名和参数
    /// </summary>
    private static (string Command, string[] Args) ParseCommand(string commandLine)
    {
        var parts = commandLine.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return ("nano", Array.Empty<string>());
        }
        if (parts.Length == 1)
        {
            return (parts[0], Array.Empty<string>());
        }
        return (parts[0], parts[1..]);
    }

    /// <summary>
    /// 判断编辑器是否是 GUI 编辑器
    /// </summary>
    private static bool IsGuiEditor(string editorCommand)
    {
        // 提取命令名（去掉参数和路径）
        var (commandName, _) = ParseCommand(editorCommand);
        
        // 去掉路径，只保留文件名
        var fileName = Path.GetFileNameWithoutExtension(commandName);
        
        return GuiEditors.Contains(fileName);
    }

    /// <summary>
    /// 获取编辑器信息
    /// </summary>
    public static EditorInfo GetEditorInfo(string? preferredEditor)
    {
        // 如果用户指定了编辑器，优先使用
        if (!string.IsNullOrEmpty(preferredEditor))
        {
            var (command, args) = ParseCommand(preferredEditor);
            var isGui = IsGuiEditor(command);
            return new EditorInfo(command, args, !isGui, false);
        }

        // 检查环境变量
        var envEditor = Environment.GetEnvironmentVariable("EDITOR");
        if (!string.IsNullOrEmpty(envEditor))
        {
            return ParseEditorCommand(envEditor);
        }

        // 根据平台选择默认编辑器
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsEditor();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetMacOSEditor();
        }
        else
        {
            return GetLinuxEditor();
        }
    }

    /// <summary>
    /// 获取 Windows 平台编辑器
    /// </summary>
    private static EditorInfo GetWindowsEditor()
    {
        // Windows 优先使用 notepad
        return new EditorInfo("notepad", Array.Empty<string>(), false, true);
    }

    /// <summary>
    /// 获取 macOS 平台编辑器
    /// </summary>
    private static EditorInfo GetMacOSEditor()
    {
        // 优先尝试使用 open -t（系统默认文本编辑器）
        // -t: 使用默认文本编辑器打开
        // -W: 等待应用退出
        // -n: 总是新建一个应用实例
        if (IsCommandAvailable("open"))
        {
            // open -t -W 不需要终端
            return new EditorInfo("open", new[] { "-t", "-W", "-n" }, false, false);
        }

        // 尝试终端编辑器
        var editors = new[] { "nano", "vim", "vi" };
        foreach (var editor in editors)
        {
            if (IsCommandAvailable(editor))
            {
                return new EditorInfo(editor, Array.Empty<string>(), true, true);
            }
        }

        // 默认使用 nano
        return new EditorInfo("nano", Array.Empty<string>(), true, true);
    }

    /// <summary>
    /// 获取 Linux 平台编辑器
    /// </summary>
    private static EditorInfo GetLinuxEditor()
    {
        // 尝试查找可用的编辑器
        var editors = new[] { "nano", "vim", "vi" };
        foreach (var editor in editors)
        {
            if (IsCommandAvailable(editor))
            {
                return new EditorInfo(editor, Array.Empty<string>(), true, true);
            }
        }

        // 默认使用 nano
        return new EditorInfo("nano", Array.Empty<string>(), true, true);
    }

    /// <summary>
    /// 解析编辑器命令（用于环境变量 EDITOR）
    /// </summary>
    private static EditorInfo ParseEditorCommand(string commandLine)
    {
        var (command, args) = ParseCommand(commandLine);
        if (string.IsNullOrEmpty(command))
        {
            return new EditorInfo("nano", Array.Empty<string>(), true, true);
        }

        var isGui = IsGuiEditor(command);
        var useTerminal = !isGui && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        return new EditorInfo(command, args, useTerminal, !isGui);
    }

    /// <summary>
    /// 检查命令是否可用
    /// </summary>
    private static bool IsCommandAvailable(string command)
    {
        try
        {
            var process = new Process();
            process.StartInfo.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which";
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
    /// 启动编辑器
    /// </summary>
    public static Process StartEditor(EditorInfo editorInfo, string filePath)
    {
        var process = new Process();

        // GUI 编辑器（Windows notepad, macOS open -t, VS Code 等）
        if (!editorInfo.UseTerminal)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && editorInfo.Name == "notepad")
            {
                process.StartInfo.FileName = "notepad";
                process.StartInfo.Arguments = $"\"{filePath}\"";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && editorInfo.Name == "open")
            {
                // macOS open -t -W -n
                process.StartInfo.FileName = "open";
                process.StartInfo.Arguments = $"-t -W -n \"{filePath}\"";
            }
            else
            {
                // 通用 GUI 编辑器处理
                process.StartInfo.FileName = editorInfo.Name;
                var allArgs = editorInfo.Arguments.ToList();
                allArgs.Add(filePath);
                process.StartInfo.Arguments = string.Join(" ", allArgs.Select(a => $"\"{a}\""));
            }

            process.StartInfo.UseShellExecute = true;
        }
        else
        {
            // 终端编辑器 - 直接在当前终端启动
            return StartTerminalEditor(editorInfo.Name, editorInfo.Arguments, filePath);
        }

        Console.WriteLine($"Starting: {process.StartInfo.FileName} {process.StartInfo.Arguments}");
        process.Start();
        return process;
    }

    /// <summary>
    /// 启动终端编辑器
    /// 对于终端编辑器，我们直接在**当前终端**启动它们（因为 MarkPdf 本身就是通过终端启动的）
    /// </summary>
    private static Process StartTerminalEditor(string editorName, string[] editorArgs, string filePath)
    {
        var process = new Process();
        
        // 直接在当前终端启动编辑器
        process.StartInfo.FileName = editorName;
        process.StartInfo.Arguments = string.Join(" ", editorArgs.Concat(new[] { $"\"{filePath}\"" }));
        process.StartInfo.UseShellExecute = false;
        
        Console.WriteLine($"Starting terminal editor: {process.StartInfo.FileName} {process.StartInfo.Arguments}");
        Console.WriteLine("(Editor will open in this terminal window)");
        
        process.Start();
        return process;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    /// <summary>
    /// 编辑器信息
    /// </summary>
    /// <param name="Name">编辑器名称</param>
    /// <param name="Arguments">额外参数</param>
    /// <param name="UseTerminal">是否需要在终端中运行</param>
    /// <param name="WaitForExit">是否需要等待退出</param>
    public record EditorInfo(string Name, string[] Arguments, bool UseTerminal, bool WaitForExit);
}
