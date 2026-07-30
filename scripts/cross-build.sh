#!/usr/bin/env bash
# Cross-build Tonome Desktop for Linux from any platform
# Prerequisites: .NET SDK 9.0, Docker (for Linux build)

set -euo pipefail
cd "$(dirname "$0")/.."

BUILD_CONFIG="${BUILD_CONFIG:-Release}"
OUTPUT="${OUTPUT:-./build}"

echo "=== Cross-building Tonome Desktop for Linux ==="

# Use Docker to build for Linux if not on Linux
if [[ "$(uname)" != "Linux" ]]; then
    echo "Not on Linux - using Docker for cross-build..."

    docker build -t tonome-build -f - . <<'DOCKERFILE'
FROM mcr.microsoft.com/dotnet/sdk:9.0

RUN apt-get update && apt-get install -y \
    libwayland-dev \
    libgl1-mesa-dev \
    libxkbcommon-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /build
COPY . .

RUN dotnet publish src/Tonome.Framework \
    -c Release -r linux-x64 --self-contained true \
    -o /output/framework && \
    dotnet publish src/Tonome.Compositor \
    -c Release -r linux-x64 --self-contained true \
    -o /output/compositor && \
    dotnet publish src/Tonome.Shell \
    -c Release -r linux-x64 --self-contained true \
    -o /output/shell && \
    dotnet publish src/Tonome.Settings \
    -c Release -r linux-x64 --self-contained true \
    -o /output/settings && \
    dotnet publish src/Tonome.Session \
    -c Release -r linux-x64 --self-contained true \
    -o /output/session && \
    dotnet publish src/Tonome.Boot \
    -c Release -r linux-x64 --self-contained true \
    -o /output/boot

RUN echo "Build complete"
DOCKERFILE

    # Extract output from Docker image
    docker create --name tonome-extract tonome-build
    docker cp tonome-extract:/output/. "$OUTPUT"
    docker rm tonome-extract

    echo "Cross-build complete. Output in: $OUTPUT"
else
    # Native Linux build
    echo "Linux detected - building natively..."
    mkdir -p "$OUTPUT"

    dotnet publish src/Tonome.Framework \
        -c "$BUILD_CONFIG" -r linux-x64 --self-contained true \
        -o "$OUTPUT/framework"

    dotnet publish src/Tonome.Compositor \
        -c "$BUILD_CONFIG" -r linux-x64 --self-contained true \
        -o "$OUTPUT/compositor"

    dotnet publish src/Tonome.Shell \
        -c "$BUILD_CONFIG" -r linux-x64 --self-contained true \
        -o "$OUTPUT/shell"

    dotnet publish src/Tonome.Settings \
        -c "$BUILD_CONFIG" -r linux-x64 --self-contained true \
        -o "$OUTPUT/settings"

    dotnet publish src/Tonome.Session \
        -c "$BUILD_CONFIG" -r linux-x64 --self-contained true \
        -o "$OUTPUT/session"

    dotnet publish src/Tonome.Boot \
        -c "$BUILD_CONFIG" -r linux-x64 --self-contained true \
        -o "$OUTPUT/boot"

    echo "Native build complete. Output in: $OUTPUT"
fi
