#!/bin/bash
# MarkPdf 发布脚本
# 使用方法: ./publish.sh [mode] [os] [arch]
#
# 模式 (mode):
#   standard   - 标准发布（默认，无裁剪，~93MB）
#   optimized  - 优化发布（部分裁剪，~21MB）
#   minimal    - 最小体积（激进裁剪，更小体积）
#
# 示例:
#   ./publish.sh                    # 使用默认配置发布当前平台
#   ./publish.sh standard           # 标准模式发布当前平台
#   ./publish.sh optimized linux x64 # 优化模式发布 Linux x64
#   ./publish.sh minimal osx arm64  # 最小模式发布 macOS ARM64

set -e

# 解析参数
MODE="${1:-standard}"
OS="${2:-}"
ARCH="${3:-}"

# 如果第一个参数是 OS 名称（没有指定 mode），则调整参数
# 支持的模式名称
case "$MODE" in
    linux*|darwin*|osx*|win*|msys*|cygwin*)
        # 第一个参数是 OS，没有指定 mode，使用默认值
        ARCH="$OS"
        OS="$MODE"
        MODE="standard"
        ;;
esac

# 如果没有指定 OS，使用当前系统
if [ -z "$OS" ]; then
    OS=$(uname -s | tr '[:upper:]' '[:lower:]')
fi

# 如果没有指定 ARCH，使用当前架构
if [ -z "$ARCH" ]; then
    ARCH=$(uname -m)
fi

# 规范化 OS 名称
case "$OS" in
    linux*)
        OS="linux"
        ;;
    darwin* | macos*)
        OS="osx"
        ;;
    win* | msys* | cygwin*)
        OS="win"
        ;;
    *)
        echo "Unknown OS: $OS"
        exit 1
        ;;
esac

# 规范化架构名称
case "$ARCH" in
    x86_64 | amd64)
        ARCH="x64"
        ;;
    aarch64 | arm64)
        ARCH="arm64"
        ;;
    x86 | i386 | i686)
        ARCH="x86"
        ;;
esac

RUNTIME="$OS-$ARCH"
PROJECT_NAME="MarkPdf"

# 根据模式设置输出目录和发布参数
case "$MODE" in
    standard|std)
        MODE="standard"
        OUTPUT_DIR="artifacts/$PROJECT_NAME-$RUNTIME"
        EXTRA_PROPS=""
        ;;
    optimized|opt)
        MODE="optimized"
        OUTPUT_DIR="artifacts/${PROJECT_NAME}-trimmed-${RUNTIME}"
        EXTRA_PROPS="-p:PublishTrimmed=true -p:TrimMode=partial -p:EnableCompressionInSingleFile=true"
        ;;
    minimal|min)
        MODE="minimal"
        OUTPUT_DIR="artifacts/${PROJECT_NAME}-minimal-${RUNTIME}"
        EXTRA_PROPS="-p:PublishTrimmed=true -p:TrimMode=full -p:EnableCompressionInSingleFile=true -p:InvariantGlobalization=true -p:UseSystemResourceKeys=true"
        ;;
    *)
        echo "Unknown mode: $MODE"
        echo "Available modes: standard, optimized, minimal"
        exit 1
        ;;
esac

# 显示发布信息
echo "=================================="
echo "Publishing $PROJECT_NAME"
echo "Mode: $MODE"
echo "Runtime: $RUNTIME"
case "$MODE" in
    standard)
        echo "TrimMode: none"
        echo "Compression: disabled"
        echo "Expected size: ~93 MB"
        ;;
    optimized)
        echo "TrimMode: partial"
        echo "Compression: enabled"
        echo "Expected size: ~21 MB"
        ;;
    minimal)
        echo "TrimMode: full (aggressive)"
        echo "Compression: enabled"
        echo "Expected size: < 21 MB"
        ;;
esac
echo "Output: $OUTPUT_DIR"
echo "=================================="

# 清理并创建输出目录
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# 构建 dotnet publish 命令
PUBLISH_CMD="dotnet publish \
    -c Release \
    -r $RUNTIME \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    $EXTRA_PROPS \
    -o $OUTPUT_DIR"

# 执行发布
echo ""
echo "Running: $PUBLISH_CMD"
echo ""

if [ "$MODE" = "minimal" ]; then
    # minimal 模式可能失败（full trim），需要回退到 partial
    eval $PUBLISH_CMD 2>&1 || {
        echo ""
        echo "Full trim failed, falling back to partial trim..."
        EXTRA_PROPS="-p:PublishTrimmed=true -p:TrimMode=partial -p:EnableCompressionInSingleFile=true -p:InvariantGlobalization=true -p:UseSystemResourceKeys=true"
        PUBLISH_CMD="dotnet publish \
            -c Release \
            -r $RUNTIME \
            --self-contained true \
            -p:PublishSingleFile=true \
            -p:IncludeNativeLibrariesForSelfExtract=true \
            $EXTRA_PROPS \
            -o $OUTPUT_DIR"
        eval $PUBLISH_CMD
    }
else
    eval $PUBLISH_CMD
fi

# 删除调试文件和临时文件
rm -f "$OUTPUT_DIR"/*.pdb
rm -f "$OUTPUT_DIR"/*.xml
rm -f "$OUTPUT_DIR"/*.txt
rm -f "$OUTPUT_DIR"/*.deps.json 2>/dev/null || true

echo ""
echo "=================================="
echo "Publish completed!"
echo "=================================="

# 显示文件大小
echo ""
echo "File size:"
ls -lh "$OUTPUT_DIR"/MarkPdf* 2>/dev/null | awk '{print "  " $9 ": " $5}'

# 创建压缩包
echo ""
echo "Creating archive..."
cd artifacts

# 根据模式确定压缩包名称
case "$MODE" in
    standard)
        ARCHIVE_NAME="$PROJECT_NAME-$RUNTIME.tar.gz"
        ;;
    optimized)
        ARCHIVE_NAME="${PROJECT_NAME}-trimmed-${RUNTIME}.tar.gz"
        ;;
    minimal)
        ARCHIVE_NAME="${PROJECT_NAME}-minimal-${RUNTIME}.tar.gz"
        ;;
esac

tar -czf "$ARCHIVE_NAME" "$(basename "$OUTPUT_DIR")"
echo "Archive created: artifacts/$ARCHIVE_NAME"
ls -lh "$ARCHIVE_NAME" | awk '{print "Archive size: " $5}'
cd ..

# 测试发布结果
echo ""
echo "Testing..."
"$OUTPUT_DIR"/MarkPdf --help 2>&1 | head -5

echo ""
echo "Done! You can distribute: artifacts/$ARCHIVE_NAME"
