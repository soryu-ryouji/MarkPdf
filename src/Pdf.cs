using System.Diagnostics;
using System.Text;

namespace MarkPdf;

class Pdf
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    public static void ExportMarks(string pdfFile, string markFile)
    {
        var infoText = RunPdftkDump(pdfFile);
        var marksText = Bookmark.ExtractTkMark(infoText);
        var marks = Bookmark.ParseTkMark(marksText);

        File.WriteAllLines(markFile, marks.Select(m => m.ToSimpleMark()), Utf8NoBom);
        Console.WriteLine($"已导出{marks.Count}条书签到: {markFile}");
    }

    public static void ImportSimpleMarkText(string pdfFile, string markFile, bool replace = false)
    {
        if (!File.Exists(markFile))
        {
            throw new FileNotFoundException("mark file not found", markFile);
        }

        var markText = File.ReadAllText(markFile, Encoding.UTF8);
        var marks = Bookmark.ParseSimpleMark(markText);
        Console.WriteLine($"解析到 {marks.Count} 条书签");

        var infoText = RunPdftkDump(pdfFile);
        var cleaned = Bookmark.RemoveTkMarkFromPdfInfo(infoText);
        var newInfo = cleaned + Bookmark.ToTkMark(marks);

        string outPdfPath;
        if (replace)
        {
            // 先输出到临时文件，再替换原文件
            outPdfPath = Path.Combine(Path.GetTempPath(), $"pdftk_replace_{Guid.NewGuid()}.pdf");
        }
        else
        {
            outPdfPath = GetOutputPath(pdfFile);
        }

        RunPdftkUpdate(pdfFile, newInfo, outPdfPath);

        if (!File.Exists(outPdfPath))
        {
            throw new IOException("Failed to generate new PDF.");
        }

        if (replace)
        {
            File.Copy(outPdfPath, pdfFile, true);
            File.Delete(outPdfPath);
            Console.WriteLine($"已写入书签并替换: {pdfFile}");
        }
        else
        {
            Console.WriteLine($"已写入书签并生成: {outPdfPath}");
        }
    }

    private static string RunPdftkDump(string pdfFile)
    {
        return RunPdftkWithTempFiles(pdfFile, (tempPdf, tempOut) =>
        {
            RunPdftk($"\"{tempPdf}\" dump_data_utf8 output \"{tempOut}\"");
            return File.Exists(tempOut) ? File.ReadAllText(tempOut, Encoding.UTF8) : 
                throw new IOException("Failed to export PDF info.");
        });
    }

    private static void RunPdftkUpdate(string pdfFile, string infoContent, string outPdf)
    {
        RunPdftkWithTempFiles(pdfFile, (tempPdf, tempOut) =>
        {
            var tempInfo = Path.Combine(Path.GetTempPath(), $"pdftk_info_{Guid.NewGuid()}.info");
            try
            {
                File.WriteAllText(tempInfo, infoContent, Utf8NoBom);
                RunPdftk($"\"{tempPdf}\" update_info_utf8 \"{tempInfo}\" output \"{tempOut}\"");
                
                if (File.Exists(tempOut))
                {
                    File.Copy(tempOut, outPdf, true);
                }
                return true;
            }
            finally
            {
                TryDelete(tempInfo);
            }
        });
    }

    private static T RunPdftkWithTempFiles<T>(string pdfFile, Func<string, string, T> action)
    {
        var tempPdf = Path.Combine(Path.GetTempPath(), $"pdftk_in_{Guid.NewGuid()}.pdf");
        var tempOut = Path.Combine(Path.GetTempPath(), $"pdftk_out_{Guid.NewGuid()}");

        try
        {
            File.Copy(pdfFile, tempPdf, true);
            return action(tempPdf, tempOut);
        }
        finally
        {
            TryDelete(tempPdf);
            TryDelete(tempOut);
        }
    }

    private static void RunPdftk(string arguments)
    {
        using var p = new Process();
        p.StartInfo.FileName = "pdftk";
        p.StartInfo.Arguments = arguments;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.Start();
        
        var error = p.StandardError.ReadToEnd();
        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            throw new IOException($"pdftk failed (exit code {p.ExitCode}): {error}");
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
}