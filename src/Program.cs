
using System.CommandLine;

namespace MarkPdf;

class Program
{
    private static AppConfig Config = AppConfig.Load();

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

        var replaceOption = new Option<bool>("--replace", "-r")
        {
            Description = "Replace the original PDF file instead of creating a new one",
            Required = false,
        };

        var editorOption = new Option<string>("--editor", "-e")
        {
            Description = "Editor to use (can be set in config)",
            Required = false,
        };

        var watchOption = new Option<bool>("--watch", "-w")
        {
            Description = "Watch mode: auto-save PDF when bookmark file changes",
            Required = false,
        };

        // Init config command
        var initCommand = new Command("init", "Initialize configuration file");

        // Config command
        var configCommand = new Command("config", "Edit configuration file");

        var importCommand = new Command("import", "Import bookmarks from a mark file to a PDF")
        {
            pdfOption,
            markOption,
            replaceOption,
        };

        var exportCommand = new Command("export", "Export bookmarks from a PDF to a mark file")
        {
            pdfOption,
            markOption,
        };

        var editCommand = new Command("edit", "Edit PDF bookmarks interactively")
        {
            pdfOption,
            editorOption,
            watchOption,
        };

        initCommand.SetAction((_) =>
        {
            var config = AppConfig.Init();
            Console.WriteLine($"Config location: {AppConfig.GetConfigPath()}");
            Console.WriteLine("Current settings:");
            Console.WriteLine($"  Default editor: {config.DefaultEditor ?? "(auto-detect)"}");
            Console.WriteLine($"  Edit replace: {config.EditReplace}");
            Console.WriteLine($"  Import replace: {config.ImportReplace}");
            Console.WriteLine($"  Encoding: {config.Encoding}");
            Console.WriteLine($"  Include BOM: {config.IncludeBom}");
            if (!string.IsNullOrEmpty(config.ExportSuffix))
            {
                Console.WriteLine($"  Export suffix: {config.ExportSuffix}");
            }
        });

        configCommand.SetAction((_) =>
        {
            AppConfig.EditConfig();
        });

        importCommand.SetAction((importArgs) =>
        {
            var pdfPath = importArgs.GetValue<string>(pdfOption);
            var markPath = importArgs.GetValue<string>(markOption);
            var replace = importArgs.GetValue<bool>(replaceOption);

            // 如果命令行没有指定 --replace，使用配置文件的默认值
            if (!replace && !args.Contains("--replace") && !args.Contains("-r"))
            {
                replace = Config.ImportReplace;
            }

            Pdf.ImportSimpleMarkText(pdfPath!, markPath!, replace);
        });

        exportCommand.SetAction((exportArgs) =>
        {
            var pdfPath = exportArgs.GetValue<string>(pdfOption);
            var markPath = exportArgs.GetValue<string>(markOption);
            Pdf.ExportMarks(pdfPath!, markPath!);
        });

        editCommand.SetAction((editArgs) =>
        {
            var pdfPath = editArgs.GetValue<string>(pdfOption);
            var editor = editArgs.GetValue<string>(editorOption);
            var watch = editArgs.GetValue<bool>(watchOption);

            // 优先使用命令行指定的编辑器，其次使用配置文件
            editor ??= Config.DefaultEditor;

            if (watch)
            {
                // 监听模式
                Pdf.EditBookmarksWatchMode(pdfPath!, editor);
            }
            else
            {
                // 普通模式
                Pdf.EditBookmarks(pdfPath!, editor);
            }
        });

        rootCommand.Add(initCommand);
        rootCommand.Add(configCommand);
        rootCommand.Add(importCommand);
        rootCommand.Add(exportCommand);
        rootCommand.Add(editCommand);

        rootCommand.Parse(args).Invoke();
    }
}
