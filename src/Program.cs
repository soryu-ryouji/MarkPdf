
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
            return Pdf.ExportMarks(args[1], args[2]);
        }
        else if (cmd == "import" && args.Length == 3)
        {
            return Pdf.ImportSimpleMarkText(args[1], args[2]);
        }
        else
        {
            Console.WriteLine("参数有误！");
            return 2;
        }
    }
}