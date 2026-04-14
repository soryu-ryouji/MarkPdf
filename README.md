# MarkPdf

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey.svg)]()

A lightweight, cross-platform CLI tool for editing PDF bookmarks (table of contents) without external dependencies.

[English](#markpdf) | [中文](README-zh.md)

---

## MarkPdf (English)

### Features

- 🚀 **Fast & Lightweight** - Optimized binary ~15MB, no external dependencies
- 📝 **Interactive Editing** - Edit bookmarks directly in your favorite editor
- 👁️ **Watch Mode** - Auto-save PDF when bookmark file changes (no need to restart)
- 🔧 **Simple Format** - Markdown-like syntax for bookmarks
- 🖥️ **Cross-Platform** - Windows, macOS, and Linux support
- 🎯 **Smart Editor Detection** - Auto-detects VS Code, Sublime, Vim, etc.
- ⚙️ **Configurable** - Customizable defaults via config file
- 🔄 **Incremental Update** - Automatically replaces old bookmarks when importing new ones

### Installation

#### Option 1: Pre-built Binaries

Download the latest release from [Releases](../../releases) and extract:

```bash
# macOS (Apple Silicon)
tar -xzf MarkPdf-trimmed-osx-arm64.tar.gz
sudo cp MarkPdf-trimmed-osx-arm64/MarkPdf /usr/local/bin/

# macOS (Intel)
tar -xzf MarkPdf-trimmed-osx-x64.tar.gz
sudo cp MarkPdf-trimmed-osx-x64/MarkPdf /usr/local/bin/

# Linux
tar -xzf MarkPdf-trimmed-linux-x64.tar.gz
sudo cp MarkPdf-trimmed-linux-x64/MarkPdf /usr/local/bin/

# Windows
# Extract MarkPdf-trimmed-win-x64.zip and add to PATH
```

#### Option 2: Build from Source

**Requirements:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
# Clone repository
git clone https://github.com/yourusername/MarkPdf.git
cd MarkPdf

# Build for current platform
./publish-optimized.sh

# Or build manually
dotnet publish -c Release -o ./output
```

### Quick Start

```bash
# Initialize configuration
MarkPdf init

# Export existing bookmarks
MarkPdf export --pdf book.pdf --mark bookmarks.txt

# Edit bookmarks interactively (opens your default editor)
MarkPdf edit --pdf book.pdf

# Import bookmarks from file
MarkPdf import --pdf book.pdf --mark bookmarks.txt --replace
```

### Bookmark Format

Bookmarks are stored in a simple text format:

```text
# Chapter 1 Introduction 1
## 1.1 Overview 2
## 1.2 Background 3
# Chapter 2 Method 5
## 2.1 Design 6
### 2.1.1 Phase 1 7
### 2.1.2 Phase 2 8
## 2.2 Implementation 10
# Chapter 3 Conclusion 15
```

**Syntax:**
- `#` = Level 1 heading (chapter)
- `##` = Level 2 heading (section)
- `###` = Level 3 heading (subsection)
- `####` = Level 4 heading (and so on)
- Number at the end = Page number

**Notes:**
- One bookmark per line
- Page number must be a positive integer
- Space between title and page number

### Commands

#### `init` - Initialize Configuration

```bash
MarkPdf init
```

Creates a configuration file at `~/.config/MarkPdf/config.json` with smart defaults.

#### `config` - Edit Configuration

```bash
MarkPdf config
```

Opens the configuration file in your default editor.

#### `export` - Export Bookmarks

```bash
MarkPdf export --pdf <pdf_path> --mark <mark_path>
```

Exports all bookmarks from a PDF to a text file.

**Example:**
```bash
MarkPdf export --pdf book.pdf --mark book-marks.txt
```

#### `import` - Import Bookmarks

```bash
MarkPdf import --pdf <pdf_path> --mark <mark_path> [--replace]
```

Imports bookmarks from a text file into a PDF.

**Examples:**
```bash
# Create new file (default)
MarkPdf import --pdf book.pdf --mark new-marks.txt
# Output: book_new.pdf

# Replace original file
MarkPdf import --pdf book.pdf --mark new-marks.txt --replace
```

Options:
- `--replace` (-r): Replace the original PDF instead of creating a new one

#### `edit` - Interactive Editing

```bash
MarkPdf edit --pdf <pdf_path> [--editor <editor>] [--watch]
```

Opens the current bookmarks (or a template) in your default editor. Save and close to apply changes.

**Examples:**
```bash
# Use default editor
MarkPdf edit --pdf book.pdf

# Specify editor
MarkPdf edit --pdf book.pdf --editor "vim"
MarkPdf edit --pdf book.pdf --editor "code --wait"
MarkPdf edit --pdf book.pdf --editor "subl -w"

# Watch mode: auto-save PDF when bookmark file changes
MarkPdf edit --pdf book.pdf --watch
MarkPdf edit --pdf book.pdf --editor "code --wait" --watch
```

**Standard Workflow:**
1. Run command, program extracts current bookmarks (or shows template if none)
2. Modify bookmarks in editor
3. Save and close editor
4. Program automatically updates PDF bookmarks

**Watch Mode Workflow:**
1. Run command with `--watch` flag
2. Editor opens with bookmark file
3. Edit and save the file → PDF updates automatically
4. Continue editing and saving → PDF updates each time
5. Press Enter in terminal to stop watching and exit

**Watch Mode Benefits:**
- No need to restart MarkPdf for each change
- Instant feedback: see PDF updates immediately
- Perfect for fine-tuning bookmark structure
- Smart save: PDF is only updated when bookmark content actually changes

**Watch Mode Notes:**
- GUI editors open normally
- Terminal editors (vim, nvim, nano) automatically open in a new terminal window

### Configuration

#### Config File Location

| Platform | Path |
|----------|------|
| Windows | `%USERPROFILE%\.config\MarkPdf\config.json` |
| macOS | `~/.config/MarkPdf/config.json` |
| Linux | `~/.config/MarkPdf/config.json` |

#### Config Options

```json
{
  "editor": "code --wait",
  "editReplace": true,
  "importReplace": false,
  "encoding": "utf-8",
  "bom": false,
  "exportSuffix": null
}
```

| Option | Description | Default |
|--------|-------------|---------|
| `editor` | Default editor command | Auto-detect |
| `editReplace` | Auto-replace PDF when editing | `true` |
| `importReplace` | Default replace behavior for import | `false` |
| `encoding` | Text file encoding | `utf-8` |
| `bom` | Include BOM in exported files | `false` |
| `exportSuffix` | Suffix for exported bookmark files | `null` |

### Editor Support

#### Smart Detection Priority

The program automatically detects available editors in this order:

| Priority | Editor | Command | Type |
|----------|--------|---------|------|
| 1 | VS Code | `code --wait` | GUI |
| 2 | Sublime Text | `subl -w` | GUI |
| 3 | Cursor | `cursor --wait` | GUI |
| 4 | Zed | `zed --wait` | GUI |
| 5 | Fleet | `fleet --wait` | GUI |
| 6 | TextMate | `mate -w` | GUI |
| 7 | BBEdit | `bbedit` | GUI |
| 8 | Notepad++ | `notepad++` | GUI |
| 9 | Notepad2 | `notepad2` | GUI |
| 10 | Neovim | `nvim` | Terminal |
| 11 | Vim | `vim` | Terminal |
| 12 | GNU Nano | `nano` | Terminal |
| 13 | Vi | `vi` | Terminal |
| 14 | Notepad | `notepad` | GUI (Windows) |
| 15 | TextEdit | `open -t -W -n` | GUI (macOS) |

#### Editor Selection Priority

1. Command line `--editor` argument (highest priority)
2. Configuration file `editor` setting
3. `$EDITOR` environment variable
4. Auto-detection (order in table above)

#### GUI Editor Notes

GUI editors (like VS Code, Sublime Text) usually need `--wait` or `-w` flag to wait for the editor to close before returning.

```bash
# Correct
MarkPdf edit --pdf book.pdf --editor "code --wait"
MarkPdf edit --pdf book.pdf --editor "subl -w"

# Incorrect (won't wait for editor to close)
MarkPdf edit --pdf book.pdf --editor "code"
```

### Building & Publishing

#### Publish Scripts

```bash
# Optimized build (~15MB, recommended)
./publish-optimized.sh

# Minimal build (~10MB, aggressive trimming)
./publish-minimal.sh

# Original build (~35MB, no trimming)
./publish.sh
```

#### Size Comparison

| Option | Package Size | Description |
|--------|--------------|-------------|
| **Minimal** | ~10 MB | Aggressive trimming, smallest size |
| **Optimized** | ~15 MB | Balanced, recommended |
| **Original** | ~35 MB | Best compatibility |

#### Manual Build

```bash
# Single file publish
dotnet publish -c Release \
    -r osx-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:EnableCompressionInSingleFile=true \
    -o ./output
```

Supported Runtime Identifiers (RID):
- `osx-arm64` - macOS Apple Silicon
- `osx-x64` - macOS Intel
- `linux-x64` - Linux x64
- `linux-arm64` - Linux ARM64
- `win-x64` - Windows x64
- `win-arm64` - Windows ARM64

See [PUBLISH.md](PUBLISH.md) for detailed distribution instructions.

### Troubleshooting

**Q: Editor doesn't open or exits immediately**

A: GUI editors need `--wait` or `-w` flag:

```bash
# Correct
MarkPdf edit --pdf book.pdf --editor "code --wait"

# Incorrect
MarkPdf edit --pdf book.pdf --editor "code"
```

Check editor is in PATH:
```bash
which code
```

**Q: Changes not detected**

A: 
- Ensure you save the file before closing the editor
- Check that bookmarks format is correct (see "Bookmark Format" section)

**Q: Old bookmarks still exist after importing new ones**

A: Ensure you're using the latest version (v1.0.0+). Older versions may have this issue. New versions automatically replace old bookmarks.

**Q: PDF preview doesn't refresh**

A: macOS Preview doesn't auto-refresh. Use [Skim](https://skim-app.sourceforge.io/) for auto-refresh support:

```bash
# Open PDF with Skim
open -a Skim book.pdf

# Edit bookmarks, Skim will auto-refresh
MarkPdf edit --pdf book.pdf
```

**Q: How to batch process multiple PDFs?**

A: Use shell loop:

```bash
for pdf in *.pdf; do
    MarkPdf export --pdf "$pdf" --mark "${pdf%.pdf}.txt"
done
```

### Tech Stack

- [.NET 10](https://dotnet.microsoft.com/)
- [iText](https://itextpdf.com/) - PDF processing library
- [System.CommandLine](https://github.com/dotnet/command-line-api) - Command line parsing

### License

MIT License - see [LICENSE](LICENSE) file for details.

---

## Changelog

### v1.0.0
- ✨ Initial release
- 📝 Support bookmark import/export/edit
- 🎯 Smart editor detection
- ⚙️ Configuration file support
- 🚀 Optimized build (~15MB)
