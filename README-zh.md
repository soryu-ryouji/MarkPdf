# Mark Pdf

一个简单的 PDF 目录编辑工具，基于 iText 实现。

## 使用方法

### MarkPdf 使用说明

```
# 导出 PDF 书签文件
MarkPdf export --pdf your_pdf_path --mark your_mark_path

# 导入修改后的 PDF 书签文件
MarkPdf import --pdf your_pdf_path --mark your_mark_path

# 导入修改后的 PDF 书签文件并替换原文件
MarkPdf import --pdf your_pdf_path --mark your_mark_path --replace
```

**目录文件结构**

```
# 前言 5
# 目录 6
# 第01章 关于软件工程生产力 15
## 1.1 换个角度看"软件工程生产力提升" 15
## 1.2 Jenkins 简介 18
## 1.3 Jenkins 与 DevOps 18
# 第02章 Pipeline 入门 20
# 第03章 Pipeline 语法讲解 30
```

- 使用 `#` 表示标题层级（一个 `#` 表示一级标题，两个 `#` 表示二级标题，以此类推）
- 末尾的数字表示页码

### 如何构建

运行 dotnet build 命令。

可执行文件将位于 `bin/Release/net10.0/{platform}/publish/` 文件夹中。

```shell
dotnet publish
```

## 依赖项

- [.NET 10](https://dotnet.microsoft.com/)
- [iText](https://itextpdf.com/) - 用于 Java 和 .NET 的强大 PDF 库

## LICENSE

本项目基于 MIT 许可证开源。
