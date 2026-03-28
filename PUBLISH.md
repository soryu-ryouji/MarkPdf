# MarkPdf 发布指南

## 快速开始

### 推荐方式：优化发布（体积小，兼容性好）

```bash
# 发布当前平台（约 21MB，压缩后 15MB）
./publish-optimized.sh

# 发布指定平台
./publish-optimized.sh linux x64
./publish-optimized.sh win x64
```

### 最小体积发布（激进裁剪，约 16MB）

```bash
# 使用更激进的裁剪设置（约 16MB，压缩后 9.7MB）
./publish-minimal.sh
```

### 原始发布（兼容性最好，体积大）

```bash
# 不启用裁剪（约 93MB）
./publish.sh
```

## 体积对比

| 方案 | 未压缩 | 压缩后 | 说明 |
|------|--------|--------|------|
| **minimal** (full trim) | 16 MB | **9.7 MB** | 激进裁剪，体积最小 |
| **optimized** (partial) | 21 MB | **15 MB** | 平衡方案，推荐 |
| **original** (no trim) | 93 MB | 35 MB | 兼容性最好 |

**推荐**：使用 `optimized` 方案，体积小且兼容性好。

## 发布产物位置

```
# 优化版本
artifacts/MarkPdf-trimmed-osx-arm64/
artifacts/MarkPdf-trimmed-osx-arm64.tar.gz

# 最小版本
artifacts/MarkPdf-minimal-osx-arm64/
artifacts/MarkPdf-minimal-osx-arm64.tar.gz

# 原始版本
artifacts/MarkPdf-osx-arm64/
artifacts/MarkPdf-osx-arm64.tar.gz
```

## 技术细节

### 优化设置（publish-optimized.sh）

```xml
<!-- 项目文件配置 -->
<PublishTrimmed>true</PublishTrimmed>
<TrimMode>partial</TrimMode>
<EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
```

**Partial Trim 特点：**
- 裁剪未使用的代码，但保留反射所需的元数据
- 与 iText 等使用反射的库兼容
- 使用 JSON Source Generator 避免序列化问题

### 最小体积设置（publish-minimal.sh）

```xml
<TrimMode>full</TrimMode>
<InvariantGlobalization>true</InvariantGlobalization>
```

**Full Trim 特点：**
- 更激进的裁剪，移除更多未使用代码
- 可能与不常用功能有兼容性问题
- 体积更小

## 手动发布

```bash
# 单文件发布（推荐）
dotnet publish -c Release \
    -r osx-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=partial \
    -p:EnableCompressionInSingleFile=true \
    -o ./output

# 支持的运行时标识符（RID）
# Linux:   linux-x64, linux-arm64
# macOS:   osx-x64, osx-arm64
# Windows: win-x64, win-arm64, win-x86
```

## 使用方法

### 直接运行

```bash
# macOS / Linux
./MarkPdf --help

# Windows
MarkPdf.exe --help
```

### 添加到 PATH

**macOS/Linux:**
```bash
# 复制到 /usr/local/bin
sudo cp MarkPdf /usr/local/bin/

# 或者添加到用户 bin
mkdir -p ~/bin
cp MarkPdf ~/bin/
echo 'export PATH="$HOME/bin:$PATH"' >> ~/.zshrc
```

**Windows:**
```powershell
# 将目录添加到系统 PATH
# 设置 → 系统 → 关于 → 高级系统设置 → 环境变量
```

## 分发方式

### 1. 压缩包分发（推荐）

使用脚本生成的 `.tar.gz` 文件：

```bash
# 解压即用
tar -xzf MarkPdf-trimmed-osx-arm64.tar.gz
cd MarkPdf-trimmed-osx-arm64
./MarkPdf --help
```

### 2. 安装脚本

```bash
#!/bin/bash
INSTALL_DIR="${INSTALL_DIR:-/usr/local/bin}"
cp MarkPdf "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/MarkPdf"
echo "MarkPdf installed to $INSTALL_DIR"
```

### 3. 包管理器（高级）

- **Homebrew** (macOS/Linux)
- **Scoop** (Windows)
- **AUR** (Arch Linux)

## 常见问题

### Q: 为什么体积还是很大？

A: 包含完整 .NET 运行时是主要原因。优化后的 15MB 已经是合理的体积。

### Q: 最小版本 (minimal) 有什么风险？

A: Full trim 模式可能裁剪掉某些反射所需的代码。如果遇到运行时错误，请使用 optimized 版本。

### Q: 裁剪版本兼容性如何？

A: Optimized 版本经过测试可以正常使用，因为：
- 使用 `partial` 裁剪模式，保留反射元数据
- iText 相关程序集已标记为不裁剪 (`TrimmerRootAssembly`)
- 使用 JSON Source Generator 替代反射序列化

### Q: 找不到可执行文件？
A: 发布后的可执行文件在 `artifacts/MarkPdf-{type}-{runtime}/` 目录

### Q: 提示权限不足？
A: 添加执行权限：`chmod +x MarkPdf`

### Q: 在其他机器上运行报错？
A: 确保使用 `--self-contained true` 发布，包含所有依赖

## 测试发布

```bash
# 测试优化版本
./publish-optimized.sh
cd artifacts/MarkPdf-trimmed-osx-arm64
./MarkPdf init
./MarkPdf --help
```

## 减小体积的技巧

如需进一步减小体积：

1. **使用 AOT 编译** - 目前与 iText 不兼容，需要库支持
2. **移除未使用的 iText 功能** - 目前引用了完整 iText 包
3. **使用更小的 PDF 库** - 如 iText 的精简版或其他库

当前优化版本（15MB）已经是较好的平衡点。
