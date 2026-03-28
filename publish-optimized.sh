#!/bin/bash
# MarkPdf 优化发布脚本（启用裁剪）
# 使用方法: ./publish-optimized.sh [os] [arch]

set -e

OS=${1:-$(uname -s | tr '[:upper:]' '[:lower:]')}
ARCH=${2:-$(uname -m)}

case "$OS" in
    linux*) OS="linux" ;;
    darwin* | macos*) OS="osx" ;;
    win* | msys* | cygwin*) OS="win" ;;
    *) echo "Unknown OS: $OS"; exit 1 ;;
esac

case "$ARCH" in
    x86_64 | amd64) ARCH="x64" ;;
    aarch64 | arm64) ARCH="arm64" ;;
    x86 | i386 | i686) ARCH="x86" ;;
esac

RUNTIME="$OS-$ARCH"
PROJECT_NAME="MarkPdf"
OUTPUT_DIR="artifacts/${PROJECT_NAME}-trimmed-${RUNTIME}"

echo "=================================="
echo "Publishing $PROJECT_NAME (Optimized)"
echo "Runtime: $RUNTIME"
echo "TrimMode: partial"
echo "Compression: enabled"
echo "=================================="

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

dotnet publish \
    -c Release \
    -r "$RUNTIME" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=partial \
    -p:EnableCompressionInSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$OUTPUT_DIR"

# 删除不需要的文件
rm -f "$OUTPUT_DIR"/*.pdb
rm -f "$OUTPUT_DIR"/*.xml
rm -f "$OUTPUT_DIR"/*.txt
rm -f "$OUTPUT_DIR"/*.deps.json

echo ""
echo "=================================="
echo "Publish completed!"
echo "=================================="

echo ""
echo "Size comparison:"
echo "  Original (no trim): ~93 MB"
echo -n "  Optimized:          "
ls -lh "$OUTPUT_DIR"/MarkPdf* | awk '{print $5}'

cd artifacts
tar -czf "${PROJECT_NAME}-trimmed-${RUNTIME}.tar.gz" "${PROJECT_NAME}-trimmed-${RUNTIME}"
echo ""
echo "Archive: artifacts/${PROJECT_NAME}-trimmed-${RUNTIME}.tar.gz"
ls -lh "${PROJECT_NAME}-trimmed-${RUNTIME}.tar.gz" | awk '{print "Archive size:", $5}'
cd ..

echo ""
echo "Testing..."
"$OUTPUT_DIR"/MarkPdf --help 2>&1 | head -5
