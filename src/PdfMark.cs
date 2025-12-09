namespace MarkPdf;

public class PdfMark
{
    public string Title { get; }
    public int Level { get; }
    public int Page { get; }

    public PdfMark(string title, int level, int page)
    {
        Title = title;
        Level = level;
        Page = page;
    }

    public string ToTkMark()
    {
        return $"BookmarkBegin\nBookmarkTitle: {Title}\nBookmarkLevel: {Level}\nBookmarkPageNumber: {Page}";
    }

    public string ToSimpleMark()
    {
        return $"{new string('#', Level)} [{Title}]({Page})";
    }
}
