
using System.Diagnostics;

namespace MarkPdf;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("用法：");
            Console.WriteLine("  导出: dotnet run export <pdf路径> <mark路径>");
            Console.WriteLine("  导入: dotnet run import <pdf路径> <mark路径>");
            return 1;
        }

        var cmd = args[0].ToLower();

        if (cmd == "export" && args.Length == 3)
        {
            return ExportMarks(args[1], args[2]);
        }
        else if (cmd == "import" && args.Length == 3)
        {
            return ImportMarks(args[1], args[2]);
        }
        else
        {
            Console.WriteLine("参数有误！");
            return 2;
        }
    }

    // Step 1: 从PDF导出Mark文本
    static int ExportMarks(string pdfFile, string markFile)
    {
        var infoPath = $"{pdfFile}.info";
        // 调用pdftk导出
        var ret = RunPdftkDump(pdfFile, infoPath);
        if (ret != 0 || File.Exists(infoPath) is false)
        {
            Console.WriteLine("PDF信息导出失败！");
            return 10;
        }
        // 提取并写入mark
        var infoText = File.ReadAllText(infoPath);
        var marksText = Bookmark.ExtractPdfNormalBookmarks(infoText);
        var marks = Bookmark.NormalBookmarkToMarks(marksText);
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
    static int ImportMarks(string pdfFile, string markFile)
    {
        if (!File.Exists(markFile))
        {
            Console.WriteLine("书签文件未找到！");
            return 11;
        }
        var markText = File.ReadAllText(markFile);
        var marks = Bookmark.SimpleBookmarkToMarks(markText);
        // pdftk导出原info
        var infoPath = $"{pdfFile}.info";
        var ret = RunPdftkDump(pdfFile, infoPath);
        if (ret != 0 || !File.Exists(infoPath))
        {
            Console.WriteLine("PDF信息导出失败！");
            return 12;
        }
        // 清理并插入书签
        var infoText = File.ReadAllText(infoPath);
        var cleaned = Bookmark.RemovePdfInfoBookmark(infoText);
        cleaned += Bookmark.MarksToNormalBookmark(marks);
        File.WriteAllText(infoPath, cleaned);

        var outPdfPath = Path.Combine(Path.GetDirectoryName(pdfFile)??".",
            Path.GetFileNameWithoutExtension(pdfFile) + "_new.pdf");
        var ret2 = RunPdftkUpdate(pdfFile, infoPath, outPdfPath);
        File.Delete(infoPath);

        if (ret2 != 0 || File.Exists(outPdfPath) is false)
        {
            Console.WriteLine("生成新PDF失败！");
            return 13;
        }
        Console.WriteLine($"已写入书签并生成: {outPdfPath}");
        return 0;
    }

    static int RunPdftkDump(string pdf, string infoPath)
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

    static int RunPdftkUpdate(string pdf, string infoPath, string outPdf)
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