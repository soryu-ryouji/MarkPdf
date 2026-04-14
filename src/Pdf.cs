using System.Security.Cryptography;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;

namespace MarkPdf;

class Pdf
{
    public static void ExportMarks(string pdfFile, string markFile)
    {
        using var reader = new PdfReader(pdfFile);
        using var pdfDoc = new PdfDocument(reader);

        var marks = GetBookmarks(pdfDoc);
        File.WriteAllLines(markFile, marks.Select(m => m.ToSimpleMark()).ToArray());
        Console.WriteLine($"Export {marks.Count} bookmarks to: {markFile}");
    }

    public static void ImportSimpleMarkText(string pdfFile, string markFile, bool replace = false)
    {
        if (!File.Exists(markFile))
        {
            throw new FileNotFoundException("mark file not found", markFile);
        }

        var markText = File.ReadAllText(markFile);
        var marks = Bookmark.ParseSimpleMark(markText);
        Console.WriteLine($"Parsed {marks.Count} bookmarks from mark file.");

        string outPdfPath;
        if (replace)
        {
            outPdfPath = Path.Combine(Path.GetTempPath(), $"markpdf_replace_{Guid.NewGuid()}.pdf");
        }
        else
        {
            outPdfPath = GetOutputPath(pdfFile);
        }

        // 读取原始 PDF 并添加书签
        using (var reader = new PdfReader(pdfFile))
        using (var writer = new PdfWriter(outPdfPath))
        using (var pdfDoc = new PdfDocument(reader, writer))
        {
            // 添加新书签（会自动替换原有书签）
            AddBookmarks(pdfDoc, marks);
        }

        if (!File.Exists(outPdfPath))
        {
            throw new IOException("Failed to generate new PDF.");
        }

        if (replace)
        {
            File.Copy(outPdfPath, pdfFile, true);
            File.Delete(outPdfPath);
            Console.WriteLine($"Wrote bookmarks and replaced: {pdfFile}");
        }
        else
        {
            Console.WriteLine($"Wrote bookmarks and generated: {outPdfPath}");
        }
    }

    /// <summary>
    /// 交互式编辑 PDF 书签
    /// </summary>
    public static void EditBookmarks(string pdfFile, string? editor = null)
    {
        if (!File.Exists(pdfFile))
        {
            throw new FileNotFoundException("PDF file not found", pdfFile);
        }

        // 获取现有书签
        string initialContent;
        using (var reader = new PdfReader(pdfFile))
        using (var pdfDoc = new PdfDocument(reader))
        {
            var marks = GetBookmarks(pdfDoc);
            if (marks.Count == 0)
            {
                // 如果没有书签，提供模板
                initialContent = $"# Example Chapter 1\n## Example Section 1.1 1\n# Example Chapter 2 5\n";
                initialContent += $"\n# Total pages: {pdfDoc.GetNumberOfPages()}\n";
                initialContent += "# Edit the bookmarks above and save to apply changes.\n";
                initialContent += "# Format: # Title PageNumber (use # for level 1, ## for level 2, etc.)\n";
            }
            else
            {
                initialContent = string.Join("\n", marks.Select(m => m.ToSimpleMark()));
            }
        }

        // 启动编辑器
        var editedContent = PdfEditor.EditBookmarks(pdfFile, initialContent, editor);

        if (editedContent == null)
        {
            Console.WriteLine("Edit cancelled or no changes made.");
            return;
        }

        // 解析并验证书签
        var newMarks = Bookmark.ParseSimpleMark(editedContent);
        Console.WriteLine($"Parsed {newMarks.Count} bookmarks from edited content.");

        if (newMarks.Count == 0)
        {
            Console.WriteLine("Warning: No valid bookmarks found in edited content. Aborting.");
            return;
        }

        // 直接替换原 PDF
        var tempPdfPath = Path.Combine(Path.GetTempPath(), $"markpdf_edit_{Guid.NewGuid()}.pdf");

        try
        {
            using (var reader = new PdfReader(pdfFile))
            using (var writer = new PdfWriter(tempPdfPath))
            using (var pdfDoc = new PdfDocument(reader, writer))
            {
                AddBookmarks(pdfDoc, newMarks);
            }

            // 替换原文件
            File.Copy(tempPdfPath, pdfFile, true);
            Console.WriteLine($"Successfully updated bookmarks in: {pdfFile}");
        }
        finally
        {
            TryDelete(tempPdfPath);
        }
    }

    /// <summary>
    /// 已知终端编辑器列表
    /// </summary>
    private static readonly HashSet<string> TerminalEditors = new(StringComparer.OrdinalIgnoreCase)
    {
        "nvim", "vim", "vi", "nano", "emacs", "micro", "ne"
    };

    /// <summary>
    /// 检查编辑器是否是终端编辑器
    /// </summary>
    private static bool IsTerminalEditor(string? editorCommand)
    {
        if (string.IsNullOrEmpty(editorCommand))
            return false;

        // 提取命令名（去掉参数和路径）
        var commandName = editorCommand.Trim();
        var spaceIndex = commandName.IndexOf(' ');
        if (spaceIndex > 0)
        {
            commandName = commandName.Substring(0, spaceIndex);
        }

        // 去掉路径，只保留文件名
        var fileName = Path.GetFileNameWithoutExtension(commandName);

        return TerminalEditors.Contains(fileName);
    }

    /// <summary>
    /// 监听模式编辑 PDF 书签 - 保存时自动更新 PDF
    /// </summary>
    public static void EditBookmarksWatchMode(string pdfFile, string? editor = null)
    {
        if (!File.Exists(pdfFile))
        {
            throw new FileNotFoundException("PDF file not found", pdfFile);
        }

        // 检查是否是终端编辑器
        if (IsTerminalEditor(editor))
        {
            Console.WriteLine("Error: Watch mode is not supported for terminal editors (vim, nvim, nano, etc.).");
            Console.WriteLine();
            Console.WriteLine("Terminal editors block the terminal and cannot be used with watch mode.");
            Console.WriteLine("Please use a GUI editor instead:");
            Console.WriteLine();
            Console.WriteLine("  MarkPdf edit --pdf book.pdf --editor \"code --wait\" --watch");
            Console.WriteLine("  MarkPdf edit --pdf book.pdf --editor \"subl -w\" --watch");
            Console.WriteLine();
            Console.WriteLine("Or use standard edit mode without --watch:");
            Console.WriteLine();
            Console.WriteLine($"  MarkPdf edit --pdf \"{pdfFile}\" --editor \"{editor}\"");
            return;
        }

        // 创建临时文件
        var tempFile = Path.Combine(Path.GetTempPath(), $"markpdf_watch_{Guid.NewGuid()}.txt");

        try
        {
            // 获取现有书签作为初始内容
            string initialContent;
            using (var reader = new PdfReader(pdfFile))
            using (var pdfDoc = new PdfDocument(reader))
            {
                var marks = GetBookmarks(pdfDoc);
                if (marks.Count == 0)
                {
                    initialContent = $"# Example Chapter 1\n## Example Section 1.1 1\n# Example Chapter 2 5\n";
                    initialContent += $"\n# Total pages: {pdfDoc.GetNumberOfPages()}\n";
                    initialContent += "# Edit the bookmarks above and save to apply changes.\n";
                    initialContent += "# Format: # Title PageNumber (use # for level 1, ## for level 2, etc.)\n";
                }
                else
                {
                    initialContent = string.Join("\n", marks.Select(m => m.ToSimpleMark()));
                }
            }

            File.WriteAllText(tempFile, initialContent);

            // 启动编辑器（不等待）
            var editorInfo = PdfEditor.GetEditorInfo(editor);
            Console.WriteLine($"Starting editor in watch mode: {editorInfo.Name}");
            Console.WriteLine($"Editing: {pdfFile}");
            Console.WriteLine($"Temporary file: {tempFile}");
            Console.WriteLine();
            Console.WriteLine("Watch mode is active. The PDF will be updated automatically when you save the bookmark file.");
            Console.WriteLine("Press Enter to stop watching and exit.");
            Console.WriteLine();

            using var process = PdfEditor.StartEditor(editorInfo, tempFile);

            // 设置文件监听器
            var fileWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(tempFile)!,
                Filter = Path.GetFileName(tempFile),
                NotifyFilter = NotifyFilters.LastWrite
            };

            // 防抖处理：避免多次保存触发多次更新
            var lastWriteTime = DateTime.MinValue;
            var debounceInterval = TimeSpan.FromMilliseconds(500);
            
            // 保存上一次的书签内容哈希，用于对比是否真的改变了
            string? lastContentHash = null;

            fileWatcher.Changed += (_, _) =>
            {
                var now = DateTime.Now;
                if (now - lastWriteTime < debounceInterval)
                {
                    return; // 忽略防抖间隔内的事件
                }
                lastWriteTime = now;

                // 延迟一下确保文件写入完成
                Thread.Sleep(100);

                try
                {
                    var content = File.ReadAllText(tempFile);
                    
                    // 计算内容哈希，检查是否真的改变了
                    var contentHash = ComputeHash(content);
                    if (contentHash == lastContentHash)
                    {
                        // 内容没有变化，跳过
                        return;
                    }
                    lastContentHash = contentHash;
                    
                    var marks = Bookmark.ParseSimpleMark(content);

                    if (marks.Count > 0)
                    {
                        // 更新 PDF
                        var tempPdfPath = Path.Combine(Path.GetTempPath(), $"markpdf_watch_{Guid.NewGuid()}.pdf");
                        try
                        {
                            using (var reader = new PdfReader(pdfFile))
                            using (var writer = new PdfWriter(tempPdfPath))
                            using (var pdfDoc = new PdfDocument(reader, writer))
                            {
                                AddBookmarks(pdfDoc, marks);
                            }

                            File.Copy(tempPdfPath, pdfFile, true);
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Auto-saved {marks.Count} bookmarks to PDF");
                        }
                        finally
                        {
                            TryDelete(tempPdfPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error: {ex.Message}");
                }
            };

            fileWatcher.EnableRaisingEvents = true;

            // 等待用户按 Enter 停止
            Console.ReadLine();

            // 停止监听
            fileWatcher.EnableRaisingEvents = false;

            // 尝试关闭编辑器进程（如果还在运行）
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch { }

            Console.WriteLine("Watch mode stopped.");
        }
        finally
        {
            TryDelete(tempFile);
        }
    }

    /// <summary>
    /// 获取 PDF 中的所有书签
    /// </summary>
    private static List<PdfMark> GetBookmarks(PdfDocument pdfDoc)
    {
        var marks = new List<PdfMark>();
        var rootOutline = pdfDoc.GetOutlines(false);

        if (rootOutline != null)
        {
            ExtractBookmarksRecursive(rootOutline, marks, 0, pdfDoc);
        }

        return marks;
    }

    /// <summary>
    /// 递归提取书签
    /// </summary>
    private static void ExtractBookmarksRecursive(PdfOutline outline, List<PdfMark> marks, int parentLevel, PdfDocument pdfDoc)
    {
        int currentLevel = parentLevel + 1;

        foreach (var child in outline.GetAllChildren())
        {
            var title = child.GetTitle() ?? "";
            var page = GetOutlinePageNumber(child, pdfDoc);

            marks.Add(new PdfMark(title, currentLevel, page));

            // 递归处理子书签
            ExtractBookmarksRecursive(child, marks, currentLevel, pdfDoc);
        }
    }

    /// <summary>
    /// 获取书签指向的页码
    /// </summary>
    private static int GetOutlinePageNumber(PdfOutline outline, PdfDocument pdfDoc)
    {
        var dest = outline.GetDestination();
        if (dest is PdfExplicitDestination explicitDest)
        {
            var pageRef = explicitDest.GetPdfObject();
            if (pageRef is PdfArray destArray && destArray.Size() > 0)
            {
                var pageObj = destArray.Get(0);
                if (pageObj is PdfDictionary pageDict)
                {
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    {
                        if (pdfDoc.GetPage(i).GetPdfObject() == pageDict)
                        {
                            return i;
                        }
                    }
                }
            }
        }

        // 如果无法解析，返回 1
        return 1;
    }

    /// <summary>
    /// 清除所有书签
    /// </summary>
    private static void ClearBookmarks(PdfOutline rootOutline)
    {
        // 获取所有子书签并移除
        var children = rootOutline.GetAllChildren().ToList();
        foreach (var child in children)
        {
            child.RemoveOutline();
        }
    }

    /// <summary>
    /// 向 PDF 添加书签
    /// </summary>
    private static void AddBookmarks(PdfDocument pdfDoc, List<PdfMark> marks)
    {
        // 获取根大纲并清除所有现有书签
        var rootOutline = pdfDoc.GetOutlines(true);
        ClearBookmarks(rootOutline);

        if (marks.Count == 0) return;

        // 重新获取根大纲（清除后需要重新获取）
        rootOutline = pdfDoc.GetOutlines(true);

        // 使用栈来管理父书签，索引 0 不使用（层级从 1 开始）
        var outlineStack = new PdfOutline?[marks.Max(m => m.Level) + 1];

        // 设置根大纲
        outlineStack[0] = rootOutline;

        foreach (var mark in marks)
        {
            var parentOutline = outlineStack[mark.Level - 1];
            if (parentOutline == null) continue;

            // 创建新书签
            var newOutline = parentOutline.AddOutline(mark.Title);

            // 设置目标页
            if (mark.Page >= 1 && mark.Page <= pdfDoc.GetNumberOfPages())
            {
                var page = pdfDoc.GetPage(mark.Page);
                var dest = PdfExplicitDestination.CreateFit(page);
                newOutline.AddDestination(dest);
            }

            // 设置当前书签作为下一级的父书签
            if (mark.Level < outlineStack.Length)
            {
                outlineStack[mark.Level] = newOutline;
            }
        }
    }

    private static string GetOutputPath(string pdfFile)
    {
        var dir = Path.GetDirectoryName(pdfFile) ?? ".";
        var name = Path.GetFileNameWithoutExtension(pdfFile);
        return Path.Combine(dir, $"{name}_new.pdf");
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    /// <summary>
    /// 计算字符串的 SHA256 哈希值
    /// </summary>
    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
