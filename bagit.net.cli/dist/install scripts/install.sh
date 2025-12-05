#!/bin/sh
# Simple installer for BagIt.NET CLI
# Usage: sudo ./install.sh

BINARY_NAME="bagit.net"
TARGET_DIR="$HOME/bin"
TARGET_PATH="$TARGET_DIR/bagit.net"

# Check if the binary exists in the current directory
if [ ! -f "$BINARY_NAME" ]; then
    echo "Error: $BINARY_NAME not found in current directory."
    exit 1
fi

# Copy to target directory
echo "Installing $BINARY_NAME to $TARGET_PATH..."
cp "$BINARY_NAME" "$TARGET_PATH" || { echo "Failed to copy binary. Try running with sudo."; exit 1; }

# Make it executable
chmod +x "$TARGET_PATH" || { echo "Failed to set executable permission."; exit 1; }

echo "Installation complete. You can now run 'bagit.net --help'."
