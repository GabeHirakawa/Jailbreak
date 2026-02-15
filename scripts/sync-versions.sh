#!/usr/bin/env bash
set -euo pipefail

# Find the highest <Version> across all .csproj files in src/
# and apply it to all of them (lock-step versioning).

SRC_DIR="src"
MAX_VERSION="0.0.0"

# Find highest version
for csproj in "$SRC_DIR"/*/Jailbreak.*.csproj; do
  version=$(grep -oP '<Version>\K[^<]+' "$csproj" 2>/dev/null || echo "")
  if [ -n "$version" ]; then
    if printf '%s\n%s' "$MAX_VERSION" "$version" | sort -V | tail -1 | grep -qx "$version"; then
      MAX_VERSION="$version"
    fi
  fi
done

if [ "$MAX_VERSION" = "0.0.0" ]; then
  echo "No version found in .csproj files — nothing to sync."
  exit 0
fi

echo "Syncing all plugins to version: $MAX_VERSION"

# Apply to all .csproj files
for csproj in "$SRC_DIR"/*/Jailbreak.*.csproj; do
  if grep -q '<Version>' "$csproj"; then
    sed -i "s|<Version>[^<]*</Version>|<Version>$MAX_VERSION</Version>|" "$csproj"
  else
    # Insert Version into first PropertyGroup
    sed -i "/<PropertyGroup>/a\\    <Version>$MAX_VERSION</Version>" "$csproj"
  fi
  echo "  Updated: $csproj"
done
