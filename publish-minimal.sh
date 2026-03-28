#!/bin/bash
# MarkPdf 最小体积发布脚本（使用更激进的裁剪）

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
OUTPUT_DIR="artifacts/MarkPdf-minimal-${RUNTIME}"

echo "=================================="
echo "Publishing MarkPdf (Minimal Size)"
echo "Runtime: $RUNTIME"
echo "TrimMode: full (aggressive)"
echo "=================================="

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# 使用 full 裁剪模式（更激进）
dotnet publish \
    -c Release \
    -r "$RUNTIME" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=full \
    -p:EnableCompressionInSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:InvariantGlobalization=true \
    -p:UseSystemResourceKeys=true \
    -o "$OUTPUT_DIR" 2>&1 || {
        echo "Full trim failed, falling back to partial trim..."
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
    }

rm -f "$OUTPUT_DIR"/*.pdb
rm -f "$OUTPUT_DIR"/*.xml
rm -f "$OUTPUT_DIR"/*.txt

echo ""
echo "=================================="
echo "Results:"
echo "=================================="

ls -lh "$OUTPUT_DIR"/MarkPdf* | awk '{print "  File: " $9 " Size: " $5}'

cd artifacts
tar -czf "MarkPdf-minimal-${RUNTIME}.tar.gz" "MarkPdf-minimal-${RUNTIME}"
echo ""
echo "  Archive: MarkPdf-minimal-${RUNTIME}.tar.gz"
ls -lh "MarkPdf-minimal-${RUNTIME}.tar.gz" | awk '{print "  Size: " $5}'
cd ..

echo ""
echo "Testing..."
"$OUTPUT_DIR"/MarkPdf --help 2>&1 | head -3

echo ""
echo "Comparison:"
echo "  Original (no trim):  ~93 MB"
echo "  Optimized (partial): ~21 MB"
ls -lh "$OUTPUT_DIR"/MarkPdf* | awk '{print "  Minimal (full):      " $5}'
