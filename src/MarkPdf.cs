using System.Text.RegularExpressions;

namespace MarkPdf;

public class Mark
{
    public string Title { get; }
    public int Level { get; }
    public int Page { get; }

    public Mark(string title, int level, int page)
    {
        Title = title;
        Level = level;
        Page = page;
    }

    public string ToNormalMark()
    {
        return $"BookmarkBegin\nBookmarkTitle: {Title}\nBookmarkLevel: {Level}\nBookmarkPageNumber: {Page}";
    }

    public string ToSimpleMark()
    {
        return $"{new string('#', Level)} [{Title}]({Page})";
    }

    public override string ToString()
    {
        return ToNormalMark();
    }
}

public static class Bookmark
{
    public static List<Mark> NormalBookmarkToMarks(string text)
    {
        var pattern = @"^BookmarkBegin\nBookmarkTitle: (.+)\nBookmarkLevel: (\d+)\nBookmarkPageNumber: (\d+)$";
        var matches = Regex.Matches(text, pattern, RegexOptions.Multiline);
        var marks = new List<Mark>();

        foreach (Match match in matches)
        {
            var title = match.Groups[1].Value;
            var level = int.Parse(match.Groups[2].Value);
            var page = int.Parse(match.Groups[3].Value);
            marks.Add(new Mark(title, level, page));
        }
        return marks;
    }

    public static List<Mark> SimpleBookmarkToMarks(string text)
    {
        var pattern = @"^([#]+) \[(.*)\]\((\d+)\)$";
        var matches = Regex.Matches(text, pattern, RegexOptions.Multiline);
        var marks = new List<Mark>();

        foreach (Match match in matches)
        {
            var level = match.Groups[1].Value.Length;
            var title = match.Groups[2].Value;
            var page = int.Parse(match.Groups[3].Value);
            marks.Add(new Mark(title, level, page));
        }
        return marks;
    }

    public static string MarksToNormalBookmark(List<Mark> marks)
    {
        return string.Join("\n\n", marks.ConvertAll(mark => mark.ToNormalMark()));
    }

    public static string ExtractPdfNormalBookmarks(string infoText)
    {
        var lines = infoText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var pattern = @"^BookmarkBegin|BookmarkTitle: (.+)|BookmarkLevel: (\d+)|BookmarkPageNumber: (\d+)$";
        var resultLines = new List<string>();
        foreach (var line in lines)
        {
            if (Regex.IsMatch(line, pattern))
                resultLines.Add(line);
        }
        return string.Join("\n", resultLines);
    }

    public static string RemovePdfInfoBookmark(string text)
    {
        var cleaned = Regex.Replace(text, @"BookmarkBegin(.*?)BookmarkPageNumber: \d+", "", RegexOptions.Singleline);
        cleaned = Regex.Replace(cleaned, @"\n\s*\n", "\n");
        return cleaned;
    }
}
