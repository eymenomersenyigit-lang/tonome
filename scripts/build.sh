#!/usr/bin/env bash
# Build script for to[no]ME! Linux
# Usage: ./scripts/build.sh [target]
#   target: dev | publish | packages | iso | all

set -euo pipefail
cd "$(dirname "$0")/.."

RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

log()   { echo -e "${BLUE}[tonome]${NC} $1"; }
ok()    { echo -e "${GREEN}[OK]${NC} $1"; }
err()   { echo -e "${RED}[ERROR]${NC} $1"; }

BUILD_CONFIG="${BUILD_CONFIG:-Release}"
RID="${RID:-linux-x64}"
OUTPUT="${OUTPUT:-./build}"

dev_build() {
    log "Building Tonome Desktop (Debug)..."
    dotnet build Tonome.sln
    ok "Development build complete"
}

publish_build() {
    log "Publishing Tonome Desktop ($BUILD_CONFIG / $RID)..."

    mkdir -p "$OUTPUT/framework" "$OUTPUT/compositor" "$OUTPUT/shell" "$OUTPUT/settings" "$OUTPUT/session" "$OUTPUT/boot"

    dotnet publish src/Tonome.Framework \
        -c "$BUILD_CONFIG" -r "$RID" \
        --self-contained true \
        -o "$OUTPUT/framework"

    dotnet publish src/Tonome.Compositor \
        -c "$BUILD_CONFIG" -r "$RID" \
        --self-contained true \
        -o "$OUTPUT/compositor"

    dotnet publish src/Tonome.Shell \
        -c "$BUILD_CONFIG" -r "$RID" \
        --self-contained true \
        -o "$OUTPUT/shell"

    dotnet publish src/Tonome.Settings \
        -c "$BUILD_CONFIG" -r "$RID" \
        --self-contained true \
        -o "$OUTPUT/settings"

    dotnet publish src/Tonome.Session \
        -c "$BUILD_CONFIG" -r "$RID" \
        --self-contained true \
        -o "$OUTPUT/session"

    dotnet publish src/Tonome.Boot \
        -c "$BUILD_CONFIG" -r "$RID" \
        --self-contained true \
        -o "$OUTPUT/boot"

    ok "Published to $OUTPUT"
}

build_packages() {
    log "Building Arch Linux packages..."
    for pkgdir in packages/*/; do
        pkgname=$(basename "$pkgdir")
        log "Building $pkgname..."
        (cd "$pkgdir" && makepkg -si --noconfirm)
        ok "$pkgname built"
    done
}

build_iso() {
    log "Building to[no]ME! Linux ISO..."
    if ! command -v mkarchiso &> /dev/null; then
        err "mkarchiso not found. Install archiso: sudo pacman -S archiso"
        exit 1
    fi

    mkdir -p "$OUTPUT/iso"

    # Copy archiso configuration
    local iso_dir="$OUTPUT/iso-build"
    mkdir -p "$iso_dir"
    cp -r iso/archiso/* "$iso_dir/"
    cp -r iso/grub "$iso_dir/"

    # Build the ISO
    mkarchiso -v -w "$iso_dir/work" -o "$OUTPUT/iso" "$iso_dir"

    ok "ISO built: $(ls -1 $OUTPUT/iso/*.iso 2>/dev/null || echo 'check output directory')"
}

case "${1:-dev}" in
    dev)        dev_build ;;
    publish)    publish_build ;;
    packages)   build_packages ;;
    iso)        publish_build && build_packages && build_iso ;;
    all)        dev_build && publish_build && build_packages && build_iso ;;
    *)
        echo "Usage: $0 [dev|publish|packages|iso|all]"
        exit 1
        ;;
esac
