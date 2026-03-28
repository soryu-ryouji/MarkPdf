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

# 交互式编辑书签（自动检测最佳可用编辑器）
MarkPdf edit --pdf your_pdf_path

# 使用指定编辑器编辑
MarkPdf edit --pdf your_pdf_path --editor vim
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

### 配置文件

MarkPdf 支持配置文件来设置默认值：

```bash
# 使用智能默认值初始化配置文件
MarkPdf init

# 编辑配置文件
MarkPdf config
```

**配置文件位置：** `~/.config/MarkPdf/config.json`
- Windows: `%USERPROFILE%\.config\MarkPdf\config.json`
- macOS: `~/.config/MarkPdf/config.json`
- Linux: `~/.config/MarkPdf\config.json`

### 智能编辑器检测

运行 `MarkPdf init` 或使用自动检测时，按以下优先级检查编辑器：

| 优先级 | 编辑器 | 命令 | 检测方式 |
|--------|--------|------|----------|
| 1 | VS Code | `code --wait` | 命令可用 |
| 2 | Sublime Text | `subl -w` | 命令可用 |
| 3 | Cursor | `cursor --wait` | 命令可用 |
| 4 | Zed | `zed --wait` | 命令可用 |
| 5 | Fleet | `fleet --wait` | 命令可用 |
| 6 | TextMate | `mate -w` | 命令可用 |
| 7 | BBEdit | `bbedit` | 命令可用 |
| 8 | Notepad++ | `notepad++` | Windows 专用，路径检查 |
| 9 | Notepad2 | `notepad2` | Windows 专用，路径检查 |
| 10 | Neovim | `nvim` | 命令可用 |
| 11 | Vim | `vim` | 命令可用 |
| 12 | GNU Nano | `nano` | 命令可用 |
| 13 | Vi | `vi` | 命令可用 |
| 14 | Notepad | `notepad` | Windows 默认 |
| 15 | TextEdit | `open -t -W -n` | macOS 默认 |

**优先级（从高到低）：**
1. 命令行 `--editor` 参数
2. 配置文件 `editor` 设置
3. `$EDITOR` 环境变量
4. 从上述列表自动检测

### 交互式编辑模式

`edit` 命令打开您首选的编辑器：

1. 运行 `MarkPdf edit --pdf your.pdf`
2. 最佳可用编辑器打开，显示当前书签
3. 按照上述格式编辑书签
4. 保存并关闭编辑器
5. PDF 自动更新

### 其他智能默认值

| 设置 | Windows | macOS | Linux |
|------|---------|-------|-------|
| **BOM** | `true`（记事本兼容） | `false` | `false` |
| **编码** | `utf-8` | `utf-8` | `utf-8` |
| **编辑替换** | `true` | `true` | `true` |
| **导入替换** | `false` | `false` | `false` |

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
