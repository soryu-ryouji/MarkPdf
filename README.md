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

# Edit bookmarks interactively (auto-detects best available editor)
MarkPdf edit --pdf your_pdf_path

# Edit with a specific editor
MarkPdf edit --pdf your_pdf_path --editor vim
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

### Configuration File

MarkPdf supports a configuration file for setting default values:

```bash
# Initialize config file with smart defaults
MarkPdf init

# Edit config file
MarkPdf config
```

**Config File Location:** `~/.config/MarkPdf/config.json`
- Windows: `%USERPROFILE%\.config\MarkPdf\config.json`
- macOS: `~/.config/MarkPdf/config.json`
- Linux: `~/.config/MarkPdf/config.json`

### Smart Editor Detection

When running `MarkPdf init` or using auto-detection, editors are checked in this priority order:

| Priority | Editor | Command | Detection |
|----------|--------|---------|-----------|
| 1 | VS Code | `code --wait` | Command available |
| 2 | Sublime Text | `subl -w` | Command available |
| 3 | Cursor | `cursor --wait` | Command available |
| 4 | Zed | `zed --wait` | Command available |
| 5 | Fleet | `fleet --wait` | Command available |
| 6 | TextMate | `mate -w` | Command available |
| 7 | BBEdit | `bbedit` | Command available |
| 8 | Notepad++ | `notepad++` | Windows only, path check |
| 9 | Notepad2 | `notepad2` | Windows only, path check |
| 10 | Neovim | `nvim` | Command available |
| 11 | Vim | `vim` | Command available |
| 12 | GNU Nano | `nano` | Command available |
| 13 | Vi | `vi` | Command available |
| 14 | Notepad | `notepad` | Windows default |
| 15 | TextEdit | `open -t -W -n` | macOS default |

**Priority (high to low):**
1. Command line `--editor` argument
2. Configuration file `editor` setting
3. `$EDITOR` environment variable
4. Auto-detected from priority list above

### Interactive Edit Mode

The `edit` command opens your preferred editor:

1. Run `MarkPdf edit --pdf your.pdf`
2. The best available editor opens with current bookmarks
3. Edit using the format shown above
4. Save and close the editor
5. PDF is automatically updated

### Other Smart Defaults

| Setting | Windows | macOS | Linux |
|---------|---------|-------|-------|
| **BOM** | `true` (Notepad compatible) | `false` | `false` |
| **Encoding** | `utf-8` | `utf-8` | `utf-8` |
| **Edit Replace** | `true` | `true` | `true` |
| **Import Replace** | `false` | `false` | `false` |

### How to Build & Publish

**Quick Start:**

```shell
# Recommended: Build optimized version (~15MB)
./publish-optimized.sh

# Minimal size (~10MB)
./publish-minimal.sh

# Or manually
dotnet publish -c Release -o ./output
```

**Download Size Comparison:**

| Version | Size | Command |
|---------|------|---------|
| **Minimal** | ~10 MB | `./publish-minimal.sh` |
| **Optimized** | ~15 MB | `./publish-optimized.sh` (Recommended) |
| **Original** | ~35 MB | `./publish.sh` |

**Find the executable:**

After publishing:
```
artifacts/MarkPdf-trimmed-osx-arm64/MarkPdf      # macOS ARM64
artifacts/MarkPdf-trimmed-linux-x64/MarkPdf      # Linux x64
artifacts/MarkPdf-trimmed-win-x64/MarkPdf.exe    # Windows x64
```

**Quick Install:**

```bash
# macOS/Linux
tar -xzf MarkPdf-trimmed-osx-arm64.tar.gz
sudo cp MarkPdf-trimmed-osx-arm64/MarkPdf /usr/local/bin/
MarkPdf --help
```

See [PUBLISH.md](PUBLISH.md) for detailed instructions.

## Dependencies

- [.NET 10](https://dotnet.microsoft.com/)
- [iText](https://itextpdf.com/) - A powerful PDF library for Java and .NET

## LICENSE

This project is open source under the MIT License.
