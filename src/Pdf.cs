using System.Diagnostics;

namespace MarkPdf;

class Pdf
{
    // Step 1: 从PDF导出Mark文本
    public static void ExportMarks(string pdfFile, string markFile)
    {
        var infoPath = $"{pdfFile}.info";

        // 调用pdftk导出
        ExportPdfInfo(pdfFile, infoPath);
        if (File.Exists(infoPath) is false)
        {
            throw new IOException("Failed to export PDF info.");
        }

        // 提取并写入mark
        var infoText = File.ReadAllText(infoPath);
        var marksText = Bookmark.ExtractTkMark(infoText);
        var marks = Bookmark.ParseTkMark(marksText);

        using (var writer = new StreamWriter(markFile, false, System.Text.Encoding.UTF8))
        {
            foreach (var mark in marks) writer.WriteLine(mark.ToSimpleMark());
        }

        File.Delete(infoPath);
        Console.WriteLine($"已导出{marks.Count}条书签到: {markFile}");
    }

    // Step 2: 从Mark文件写回书签到PDF
    public static void ImportSimpleMarkText(string pdfFile, string markFile)
    {
        if (File.Exists(markFile) is false)
        {
            throw new FileNotFoundException("mark file not found");
        }

        var markText = File.ReadAllText(markFile);
        var marks = Bookmark.ParseSimpleMark(markText);

        // pdftk导出原info
        var infoPath = $"{pdfFile}.info";
        ExportPdfInfo(pdfFile, infoPath);
        if (File.Exists(infoPath) is false)
        {
            throw new FileNotFoundException("pdf info file not found");
        }

        // 清理并插入书签
        var infoText = File.ReadAllText(infoPath);
        var cleaned = Bookmark.RemoveTkMarkFromPdfInfo(infoText);
        cleaned += Bookmark.ToTkMark(marks);
        File.WriteAllText(infoPath, cleaned);

        var outPdfPath = Path.Combine(Path.GetDirectoryName(pdfFile) ?? ".",
            Path.GetFileNameWithoutExtension(pdfFile) + "_new.pdf");
        UpdatePdfFromInfo(pdfFile, infoPath, outPdfPath);
        File.Delete(infoPath);

        if (File.Exists(outPdfPath) is false)
        {
            throw new IOException("Failed to generate new PDF.");
        }

        Console.WriteLine($"已写入书签并生成: {outPdfPath}");
    }

    public static void ExportPdfInfo(string pdf, string infoPath)
    {
        var p = new Process();
        p.StartInfo.FileName = "pdftk";
        p.StartInfo.Arguments = $"\"{pdf}\" dump_data_utf8 output \"{infoPath}\"";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.Start();
        p.WaitForExit();
    }

    public static void UpdatePdfFromInfo(string pdf, string infoPath, string outPdf)
    {
        var p = new Process();
        p.StartInfo.FileName = "pdftk";
        p.StartInfo.Arguments = $"\"{pdf}\" update_info_utf8 \"{infoPath}\" output \"{outPdf}\"";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.Start();
        p.WaitForExit();
    }
}