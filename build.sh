#!/usr/bin/env bash

# Path to your Unity executable
UNITY_PATH="/opt/Unity/Editor/Unity"

# Project path
PROJECT_PATH="$(cd "$(dirname "$0")" && pwd)"

# Build method
BUILD_METHOD="BuildScript.BuildLinux"

"$UNITY_PATH" \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -executeMethod $BUILD_METHOD \
  -logFile "$PROJECT_PATH/build.log"

echo "Build finished. See build.log for details."
