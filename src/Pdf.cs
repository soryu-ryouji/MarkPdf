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
    /// 向 PDF 添加书签
    /// </summary>
    private static void AddBookmarks(PdfDocument pdfDoc, List<PdfMark> marks)
    {
        if (marks.Count == 0) return;

        // 使用栈来管理父书签，索引 0 不使用（层级从 1 开始）
        var outlineStack = new PdfOutline?[marks.Max(m => m.Level) + 1];

        // 获取根大纲
        outlineStack[0] = pdfDoc.GetOutlines(true);

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
}
