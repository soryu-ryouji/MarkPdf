# Mark Pdf

A simple PDF table of contents editing tool, based on iText.

## Usage

### MarkPdf Usage Instructions

```
# Export PDF bookmarks file
MarkPdf export --pdf your_pdf_path --mark your_mark_path

# Import the modified PDF bookmarks file
MarkPdf import --pdf your_pdf_path --mark your_mark_path

# Import the modified PDF bookmarks file and replace the original file
MarkPdf import --pdf your_pdf_path --mark your_mark_path --replace
```

**Table of Contents File Structure**

```
# Preface 5
# Table of Contents 6
# Chapter 01 About Software Engineering Productivity 15
## 1.1 Another Perspective on "Improving Software Engineering Productivity" 15
## 1.2 Introduction to Jenkins 18
## 1.3 Jenkins and DevOps 18
# Chapter 02 Getting Started with Pipeline 20
# Chapter 03 Pipeline Syntax Explanation 30
```

- Use `#` to indicate the heading level (one `#` for level 1, two `#` for level 2, etc.)
- The number at the end indicates the page number

### How to Build

Run the dotnet build command.

The executable will be found in the `bin/Release/net10.0/{platform}/publish/` folder.

```shell
dotnet publish
```

## Dependencies

- [.NET 10](https://dotnet.microsoft.com/)
- [iText](https://itextpdf.com/) - A powerful PDF library for Java and .NET

## LICENSE

This project is open source under the MIT License.
