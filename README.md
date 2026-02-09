# Mark Pdf

A simple PDF table of contents editing tool, based on pdftk.

## Usage

### Install pdftk

```
# macOS - brew
brew install pdftk-java

# debian - apt
apt install pdftk

# windows - scoop
scoop install pdftk
```

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
# Chapter 04 Environment Variables and Build Tools 51
# Chapter 05 Code Quality 64
# Chapter 06 Triggering Pipeline Execution 91
# Chapter 07 Multi-branch Build 107
# Chapter 08 Parameterized Pipeline 115
# Chapter 09 Credential Management 126
# Chapter 10 Artifact Management 137
# Chapter 11 Visual Build and Views 154
# Chapter 12 Automated Deployment 161
# Chapter 13 Notification 174
# Chapter 14 Distributed and Parallel Build 183
# Chapter 15 Extending Pipeline 200
# Chapter 16 Jenkins Operations and Maintenance 214
# Chapter 17 Automated Operations Experience 234
# Chapter 18 How to Design a Pipeline 246
# Postscript 254
```

### How to Build

Run the dotnet build command.

The executable will be found in the `bin/Release/net10.0/{platform}/publish/` folder.

```shell
dotnet publish
```

## LICENSE

This project is open source under the MIT License.
