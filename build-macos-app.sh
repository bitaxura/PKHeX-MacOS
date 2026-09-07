#!/usr/bin/env bash
set -e

# PKHeX macOS Application Bundle (.app) Builder
# Usage: ./build-macos-app.sh [--self-contained] [--arch arm64|x64] [--config Release|Debug]

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG="Release"
SELF_CONTAINED="true"
ARCH="$(uname -m)"

# Normalize arch
if [ "$ARCH" = "x86_64" ]; then
    RID="osx-x64"
else
    RID="osx-arm64"
fi

# Parse optional arguments
while [[ "$#" -gt 0 ]]; do
    case $1 in
        --config) CONFIG="$2"; shift ;;
        --arch)
            if [ "$2" = "x64" ] || [ "$2" = "x86_64" ]; then
                RID="osx-x64"
            else
                RID="osx-arm64"
            fi
            shift
            ;;
        --no-self-contained) SELF_CONTAINED="false" ;;
        --self-contained) SELF_CONTAINED="true" ;;
        *) echo "Unknown parameter: $1"; exit 1 ;;
    esac
    shift
done

echo "==================================================="
echo "Building PKHeX for macOS"
echo "  Configuration : $CONFIG"
echo "  Architecture  : $RID"
echo "  Self-Contained: $SELF_CONTAINED"
echo "==================================================="

APP_NAME="PKHeX.app"
OUTPUT_DIR="$ROOT_DIR/dist"
APP_DIR="$OUTPUT_DIR/$APP_NAME"
CONTENTS_DIR="$APP_DIR/Contents"
MACOS_DIR="$CONTENTS_DIR/MacOS"
RESOURCES_DIR="$CONTENTS_DIR/Resources"

# 1. Clean previous build
rm -rf "$OUTPUT_DIR"
mkdir -p "$MACOS_DIR"
mkdir -p "$RESOURCES_DIR"

# 2. Compile & Publish
echo "==> Publishing with dotnet..."
dotnet publish "$ROOT_DIR/PKHeX.Avalonia/PKHeX.Avalonia.csproj" \
    -c "$CONFIG" \
    -r "$RID" \
    --self-contained "$SELF_CONTAINED" \
    -o "$MACOS_DIR" \
    -p:PublishSingleFile=false

# 3. Copy App Metadata & Icons
echo "==> Setting up macOS bundle structure..."
cp "$ROOT_DIR/PKHeX.Avalonia/Info.plist" "$CONTENTS_DIR/Info.plist"

if [ -f "$ROOT_DIR/PKHeX.Avalonia/PKHeX.icns" ]; then
    cp "$ROOT_DIR/PKHeX.Avalonia/PKHeX.icns" "$RESOURCES_DIR/PKHeX.icns"
fi

echo "APPL????" > "$CONTENTS_DIR/PkgInfo"

# 4. Permissions & Codesign
echo "==> Applying permissions and ad-hoc code signing..."
chmod +x "$MACOS_DIR/PKHeX.Avalonia"

if command -v codesign &> /dev/null; then
    codesign --force --deep -s - "$APP_DIR" 2>/dev/null || true
fi

echo "==================================================="
echo "✅ macOS App Bundle successfully built!"
echo "   Path: $APP_DIR"
echo "   To launch: open \"$APP_DIR\""
echo "==================================================="
