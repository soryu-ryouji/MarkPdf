#!/bin/bash
# MarkPdf 发布脚本
# 使用方法: ./publish.sh [os] [arch]
# 例如: ./publish.sh linux x64

set -e

# 默认参数
OS=${1:-$(uname -s | tr '[:upper:]' '[:lower:]')}
ARCH=${2:-$(uname -m)}

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
OUTPUT_DIR="artifacts/$PROJECT_NAME-$RUNTIME"

echo "=================================="
echo "Publishing $PROJECT_NAME"
echo "Runtime: $RUNTIME"
echo "Output: $OUTPUT_DIR"
echo "=================================="

# 清理并创建输出目录
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# 发布应用程序
dotnet publish \
    -c Release \
    -r "$RUNTIME" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$OUTPUT_DIR"

# 删除调试文件和临时文件
rm -f "$OUTPUT_DIR"/*.pdb
rm -f "$OUTPUT_DIR"/*.xml
rm -f "$OUTPUT_DIR"/*.txt

echo ""
echo "=================================="
echo "Publish completed!"
echo "Output: $OUTPUT_DIR"
echo "=================================="

# 显示文件列表
echo ""
echo "Files in output directory:"
ls -lh "$OUTPUT_DIR"

# 创建压缩包
echo ""
echo "Creating archive..."
cd artifacts
tar -czf "$PROJECT_NAME-$RUNTIME.tar.gz" "$PROJECT_NAME-$RUNTIME"
echo "Archive created: artifacts/$PROJECT_NAME-$RUNTIME.tar.gz"
cd ..

echo ""
echo "Done! You can distribute: artifacts/$PROJECT_NAME-$RUNTIME.tar.gz"
