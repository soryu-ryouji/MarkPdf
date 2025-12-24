using System.Text.RegularExpressions;

namespace MarkPdf;

public static class Bookmark
{
    public static List<PdfMark> ParseTkMark(string text)
    {
        // 统一换行符
        text = NormalizeLineEndings(text);
        var pattern = @"^BookmarkBegin\nBookmarkTitle: (.+)\nBookmarkLevel: (\d+)\nBookmarkPageNumber: (\d+)$";
        var matches = Regex.Matches(text, pattern, RegexOptions.Multiline);
        var marks = new List<PdfMark>();

        foreach (Match match in matches)
        {
            var title = match.Groups[1].Value.Trim();
            var level = int.Parse(match.Groups[2].Value);
            var page = int.Parse(match.Groups[3].Value);
            marks.Add(new PdfMark(title, level, page));
        }
        return marks;
    }

    public static List<PdfMark> ParseSimpleMark(string text)
    {
        // 统一换行符
        text = NormalizeLineEndings(text);
        var pattern = @"^(#+)\s+\[(.+?)\]\((\d+)\)$";
        var matches = Regex.Matches(text, pattern, RegexOptions.Multiline);
        var marks = new List<PdfMark>();

        foreach (Match match in matches)
        {
            var level = match.Groups[1].Value.Length;
            var title = match.Groups[2].Value.Trim();
            var page = int.Parse(match.Groups[3].Value);
            marks.Add(new PdfMark(title, level, page));
        }
        return marks;
    }

    public static string ToTkMark(List<PdfMark> marks)
    {
        return string.Join("\n", marks.ConvertAll(mark => mark.ToTkMark()));
    }

    public static string ExtractTkMark(string infoText)
    {
        var lines = infoText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        var pattern = @"^(BookmarkBegin|BookmarkTitle: .+|BookmarkLevel: \d+|BookmarkPageNumber: \d+)$";
        var resultLines = new List<string>();
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (Regex.IsMatch(trimmed, pattern))
                resultLines.Add(trimmed);
        }
        return string.Join("\n", resultLines);
    }

    public static string RemoveTkMarkFromPdfInfo(string text)
    {
        // 统一换行符
        text = NormalizeLineEndings(text);
        // 移除所有书签块
        var cleaned = Regex.Replace(text, @"BookmarkBegin\nBookmarkTitle: .+\nBookmarkLevel: \d+\nBookmarkPageNumber: \d+\n?", "", RegexOptions.Multiline);
        // 移除多余空行
        cleaned = Regex.Replace(cleaned, @"\n{2,}", "\n");
        return cleaned.TrimEnd() + "\n";
    }

    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }
}