using System.Diagnostics;

namespace MarkPdf;

class Pdf
{
    // Step 1: 从PDF导出Mark文本
    public static int ExportMarks(string pdfFile, string markFile)
    {
        var infoPath = $"{pdfFile}.info";
        // 调用pdftk导出
        var ret = ExportPdfInfo(pdfFile, infoPath);
        if (ret != 0 || File.Exists(infoPath) is false)
        {
            Console.WriteLine("PDF信息导出失败！");
            return 10;
        }
        // 提取并写入mark
        var infoText = File.ReadAllText(infoPath);
        var marksText = Bookmark.ExtractTkMark(infoText);
        var marks = Bookmark.ParseTkMark(marksText);
        using (var writer = new StreamWriter(markFile, false, System.Text.Encoding.UTF8))
        {
            foreach (var mark in marks)
                writer.WriteLine(mark.ToSimpleMark());
        }
        File.Delete(infoPath);
        Console.WriteLine($"已导出{marks.Count}条书签到: {markFile}");
        return 0;
    }

    // Step 2: 从Mark文件写回书签到PDF
    public static int ImportSimpleMarkText(string pdfFile, string markFile)
    {
        if (!File.Exists(markFile))
        {
            Console.WriteLine("书签文件未找到！");
            return 11;
        }
        var markText = File.ReadAllText(markFile);
        var marks = Bookmark.ParseSimpleMark(markText);
        // pdftk导出原info
        var infoPath = $"{pdfFile}.info";
        var ret = ExportPdfInfo(pdfFile, infoPath);
        if (ret != 0 || !File.Exists(infoPath))
        {
            Console.WriteLine("PDF信息导出失败！");
            return 12;
        }
        // 清理并插入书签
        var infoText = File.ReadAllText(infoPath);
        var cleaned = Bookmark.RemoveTkMarkFromPdfInfo(infoText);
        cleaned += Bookmark.ToTkMark(marks);
        File.WriteAllText(infoPath, cleaned);

        var outPdfPath = Path.Combine(Path.GetDirectoryName(pdfFile) ?? ".",
            Path.GetFileNameWithoutExtension(pdfFile) + "_new.pdf");
        var ret2 = UpdatePdfFromInfo(pdfFile, infoPath, outPdfPath);
        File.Delete(infoPath);

        if (ret2 != 0 || File.Exists(outPdfPath) is false)
        {
            Console.WriteLine("生成新PDF失败！");
            return 13;
        }
        Console.WriteLine($"已写入书签并生成: {outPdfPath}");
        return 0;
    }

    public static int ExportPdfInfo(string pdf, string infoPath)
    {
        var p = new Process();
        p.StartInfo.FileName = "pdftk";
        p.StartInfo.Arguments = $"\"{pdf}\" dump_data_utf8 output \"{infoPath}\"";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.Start();
        p.WaitForExit();
        return p.ExitCode;
    }

    public static int UpdatePdfFromInfo(string pdf, string infoPath, string outPdf)
    {
        var p = new Process();
        p.StartInfo.FileName = "pdftk";
        p.StartInfo.Arguments = $"\"{pdf}\" update_info_utf8 \"{infoPath}\" output \"{outPdf}\"";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.Start();
        p.WaitForExit();
        return p.ExitCode;
    }
}