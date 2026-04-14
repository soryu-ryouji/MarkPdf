# MarkPdf

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey.svg)]()

一个轻量级、跨平台的命令行工具，用于编辑 PDF 书签（目录），无需外部依赖。

[English](README.md) | 中文

---

## 功能特性

- 🚀 **快速轻量** - 优化后仅 ~15MB，无外部依赖
- 📝 **交互式编辑** - 在喜爱的编辑器中直接编辑书签
- 👁️ **监听模式** - 保存书签文件时自动更新 PDF（无需重启程序）
- 🔧 **简单格式** - 类 Markdown 的书签语法
- 🖥️ **跨平台** - 支持 Windows、macOS 和 Linux
- 🎯 **智能编辑器检测** - 自动检测 VS Code、Sublime、Vim 等
- ⚙️ **可配置** - 通过配置文件自定义默认设置
- 🔄 **增量更新** - 导入新书签时自动替换旧书签

## 安装

### 方式 1：预编译二进制文件（推荐）

从 [Releases](../../releases) 下载最新版本并解压：

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
# 解压 MarkPdf-trimmed-win-x64.zip 并添加到 PATH
```

### 方式 2：从源码构建

**构建要求：**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
# 克隆仓库
git clone https://github.com/yourusername/MarkPdf.git
cd MarkPdf

# 为当前平台构建
./publish-optimized.sh

# 或手动构建
dotnet publish -c Release -o ./output
```

## 快速开始

```bash
# 初始化配置（创建默认配置文件）
MarkPdf init

# 导出已有书签到文本文件
MarkPdf export --pdf book.pdf --mark bookmarks.txt

# 交互式编辑书签（打开默认编辑器）
MarkPdf edit --pdf book.pdf

# 从文件导入书签并替换原 PDF
MarkPdf import --pdf book.pdf --mark bookmarks.txt --replace
```

## 书签格式

书签使用简单的文本格式存储：

```text
# 第1章 引言 1
## 1.1 概述 2
## 1.2 背景 3
# 第2章 方法 5
## 2.1 设计 6
### 2.1.1 阶段1 7
### 2.1.2 阶段2 8
## 2.2 实现 10
# 第3章 结论 15
```

**语法说明：**
- `#` = 一级标题（章节）
- `##` = 二级标题（节）
- `###` = 三级标题（小节）
- `####` = 四级标题（依此类推）
- 末尾数字 = 页码

**注意事项：**
- 每行一个书签
- 页码必须是正整数
- 标题和页码之间用空格分隔

## 命令详解

### `init` - 初始化配置

创建带智能默认值的配置文件：

```bash
MarkPdf init
```

配置文件位置：`~/.config/MarkPdf/config.json`

### `config` - 编辑配置

在默认编辑器中打开配置文件：

```bash
MarkPdf config
```

### `export` - 导出书签

将 PDF 中的所有书签导出到文本文件：

```bash
MarkPdf export --pdf <PDF路径> --mark <书签文件路径>

# 示例
MarkPdf export --pdf book.pdf --mark book-marks.txt
```

### `import` - 导入书签

将文本文件中的书签导入到 PDF：

```bash
MarkPdf import --pdf <PDF路径> --mark <书签文件路径> [--replace]

# 示例：创建新文件（默认）
MarkPdf import --pdf book.pdf --mark new-marks.txt
# 输出：book_new.pdf

# 示例：替换原文件
MarkPdf import --pdf book.pdf --mark new-marks.txt --replace
```

**选项：**
- `--replace` (-r): 替换原始 PDF 而不是创建新文件

### `edit` - 交互式编辑

在默认编辑器中打开当前书签进行编辑：

```bash
MarkPdf edit --pdf <PDF路径> [--editor <编辑器命令>] [--watch]

# 使用默认编辑器
MarkPdf edit --pdf book.pdf

# 指定编辑器
MarkPdf edit --pdf book.pdf --editor "vim"
MarkPdf edit --pdf book.pdf --editor "code --wait"
MarkPdf edit --pdf book.pdf --editor "subl -w"

# 监听模式：保存书签文件时自动更新 PDF
MarkPdf edit --pdf book.pdf --watch
MarkPdf edit --pdf book.pdf --editor "code --wait" --watch
```

**标准工作流程：**
1. 运行命令，程序提取当前书签（如果没有则显示模板）
2. 在编辑器中修改书签
3. 保存并关闭编辑器
4. 程序自动更新 PDF 书签

**监听模式工作流程：**
1. 使用 `--watch` 参数运行命令
2. 编辑器打开书签文件
3. 编辑并保存文件 → PDF 自动更新
4. 继续编辑和保存 → PDF 每次都会更新
5. 在终端按 Enter 键停止监听并退出

**监听模式优点：**
- 无需为每次更改重新启动 MarkPdf
- 即时反馈：立即看到 PDF 更新
- 非常适合微调书签结构
- 智能保存：仅当书签内容真正改变时才更新 PDF

**监听模式说明：**
- GUI 编辑器正常打开
- 终端编辑器（vim、nvim、nano）自动在新终端窗口中打开

## 配置文件

配置文件：`~/.config/MarkPdf/config.json`

### 配置位置

| 平台 | 路径 |
|------|------|
| Windows | `%USERPROFILE%\.config\MarkPdf\config.json` |
| macOS | `~/.config/MarkPdf/config.json` |
| Linux | `~/.config/MarkPdf/config.json` |

### 配置项

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

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `editor` | 默认编辑器命令 | 自动检测 |
| `editReplace` | `edit` 命令是否自动替换原 PDF | `true` |
| `importReplace` | `import` 命令默认是否替换 | `false` |
| `encoding` | 导出文件编码 | `utf-8` |
| `bom` | 导出文件是否包含 BOM | `false` |
| `exportSuffix` | 导出文件后缀 | `null` |

## 编辑器支持

### 智能检测优先级

程序按以下顺序自动检测可用编辑器：

| 优先级 | 编辑器 | 命令 | 类型 |
|--------|--------|------|------|
| 1 | VS Code | `code --wait` | GUI |
| 2 | Sublime Text | `subl -w` | GUI |
| 3 | Cursor | `cursor --wait` | GUI |
| 4 | Zed | `zed --wait` | GUI |
| 5 | Fleet | `fleet --wait` | GUI |
| 6 | TextMate | `mate -w` | GUI |
| 7 | BBEdit | `bbedit` | GUI |
| 8 | Notepad++ | `notepad++` | GUI |
| 9 | Notepad2 | `notepad2` | GUI |
| 10 | Neovim | `nvim` | 终端 |
| 11 | Vim | `vim` | 终端 |
| 12 | GNU Nano | `nano` | 终端 |
| 13 | Vi | `vi` | 终端 |
| 14 | Notepad | `notepad` | GUI (Windows) |
| 15 | TextEdit | `open -t -W -n` | GUI (macOS) |

### 编辑器选择优先级

1. 命令行 `--editor` 参数（最高优先级）
2. 配置文件中的 `editor` 设置
3. `$EDITOR` 环境变量
4. 自动检测（上表顺序）

### GUI 编辑器注意事项

GUI 编辑器（如 VS Code、Sublime Text）通常需要 `--wait` 或 `-w` 参数，让命令等待编辑器关闭后再返回。

```bash
# 正确
MarkPdf edit --pdf book.pdf --editor "code --wait"
MarkPdf edit --pdf book.pdf --editor "subl -w"

# 错误（不会等待编辑器关闭）
MarkPdf edit --pdf book.pdf --editor "code"
```

## 构建与发布

### 发布脚本

```bash
# 优化构建（~15MB，推荐）
./publish-optimized.sh

# 最小构建（~10MB，激进裁剪）
./publish-minimal.sh

# 原始构建（~35MB，无裁剪）
./publish.sh
```

### 体积对比

| 方案 | 压缩包大小 | 说明 |
|------|-----------|------|
| **Minimal** | ~10 MB | 激进裁剪，体积最小 |
| **Optimized** | ~15 MB | 平衡方案，推荐 |
| **Original** | ~35 MB | 兼容性最好 |

### 手动构建

```bash
# 单文件发布
dotnet publish -c Release \
    -r osx-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:EnableCompressionInSingleFile=true \
    -o ./output
```

支持的运行时标识符（RID）：
- `osx-arm64` - macOS Apple Silicon
- `osx-x64` - macOS Intel
- `linux-x64` - Linux x64
- `linux-arm64` - Linux ARM64
- `win-x64` - Windows x64
- `win-arm64` - Windows ARM64

详细发布说明见 [PUBLISH.md](PUBLISH.md)。

## 常见问题

### Q: 编辑器无法打开或立即退出

**A:** GUI 编辑器需要 `--wait` 或 `-w` 参数：

```bash
# 正确
MarkPdf edit --pdf book.pdf --editor "code --wait"

# 错误
MarkPdf edit --pdf book.pdf --editor "code"
```

检查编辑器是否在 PATH 中：
```bash
which code
```

### Q: 更改未被检测到

**A:** 
- 确保在关闭编辑器前保存文件
- 检查书签格式是否正确（参考"书签格式"部分）

### Q: 导入新书签后，旧书签仍然存在

**A:** 确保使用最新版本（v1.0.0+），旧版本可能存在此问题。新版会自动替换旧书签。

### Q: macOS 预览应用不自动刷新

**A:** macOS 预览（Preview.app）不会监视文件变化。建议使用 [Skim](https://skim-app.sourceforge.io/)：

```bash
# 用 Skim 打开 PDF
open -a Skim book.pdf

# 编辑书签后，Skim 会自动刷新
MarkPdf edit --pdf book.pdf
```

### Q: 如何批量处理多个 PDF？

**A:** 使用 shell 循环：

```bash
for pdf in *.pdf; do
    MarkPdf export --pdf "$pdf" --mark "${pdf%.pdf}.txt"
done
```

## 技术栈

- [.NET 10](https://dotnet.microsoft.com/)
- [iText](https://itextpdf.com/) - PDF 处理库
- [System.CommandLine](https://github.com/dotnet/command-line-api) - 命令行解析

## 许可证

MIT 许可证 - 详情见 [LICENSE](LICENSE) 文件。

---

## 更新日志

### v1.0.0
- ✨ 初始版本发布
- 📝 支持书签导入/导出/编辑
- 🎯 智能编辑器检测
- ⚙️ 配置文件支持
- 🚀 优化构建（~15MB）
