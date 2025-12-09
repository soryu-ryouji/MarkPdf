
using System.CommandLine;
using System.Diagnostics;

namespace MarkPdf;

class Program
{
    public static int Main(string[] args)
    {
        PraseArgs(args);
        return 0;
    }

    private static void PraseArgs(string[] args)
    {
        var rootCommand = new RootCommand("MarkPdf - A simple PDF bookmark manager");

        var pdfOption = new Option<string>("--pdf", "-p")
        {
            Description = "Path to the PDF file",
            Required = true,
        };

        var markOption = new Option<string>("--mark", "-m")
        {
            Description = "Path to the mark file",
            Required = true,
        };

        var importCommand = new Command("import", "Import bookmarks from a mark file to a PDF")
        {
            pdfOption,
            markOption
        };

        var exportCommand = new Command("export", "Export bookmarks from a PDF to a mark file")
        {
            pdfOption,
            markOption
        };

        importCommand.SetAction((importArgs) =>
        {
            var pdfPath = importArgs.GetValue<string>(pdfOption);
            var markPath = importArgs.GetValue<string>(markOption);
            Pdf.ImportSimpleMarkText(pdfPath!, markPath!);
        });

        exportCommand.SetAction((exportArgs) =>
        {
            var pdfPath = exportArgs.GetValue<string>(pdfOption);
            var markPath = exportArgs.GetValue<string>(markOption);
            Pdf.ExportMarks(pdfPath!, markPath!);
        });

        rootCommand.Add(importCommand);
        rootCommand.Add(exportCommand);

        rootCommand.Parse(args).Invoke();
    }
}