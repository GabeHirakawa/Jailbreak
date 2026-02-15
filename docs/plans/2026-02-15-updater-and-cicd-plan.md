# Updater Plugin & CI/CD Migration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a Jailbreak.Updater plugin that auto-updates all plugins from GitHub Releases, and migrate CI/CD to net-changesets for version management with lock-step versioning.

**Architecture:** GitHub Releases poller plugin using .NET 8 stdlib (HttpClient, System.Text.Json, ZipFile). CI uses net-changesets for changeset discipline and changelog generation, with a sync script for lock-step versioning since net-changesets lacks native `fixed` group support. Pre-releases from Version PR, stable releases on merge to main.

**Tech Stack:** C# 12 / .NET 8.0, CounterStrikeSharp.API 1.0.342, net-changesets CLI, GitHub Actions, GitHub Releases API

**Note on testing:** This is a CounterStrikeSharp plugin — there's no unit test infrastructure in the existing project. Testing is done via `dotnet build` compilation checks and manual in-game verification. The plan uses build verification instead of TDD.

---

### Task 1: Scaffold Jailbreak.Updater project

**Files:**
- Create: `src/Jailbreak.Updater/Jailbreak.Updater.csproj`
- Modify: `JailbreakNew.sln` (add project reference)

**Step 1: Create the .csproj file**

Create `src/Jailbreak.Updater/Jailbreak.Updater.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CounterStrikeSharp.API" Version="1.0.342" />
    <ProjectReference Include="..\Jailbreak.Contracts\Jailbreak.Contracts.csproj" />
  </ItemGroup>
</Project>
```

**Step 2: Add project to solution**

Run: `dotnet sln JailbreakNew.sln add src/Jailbreak.Updater/Jailbreak.Updater.csproj`

Then manually edit `JailbreakNew.sln` to assign GUID `{A1B2C3D4-0008-4000-8000-000000000008}` for consistency with existing project GUIDs (0001-0007). The `dotnet sln add` command will auto-generate a GUID — replace it.

**Step 3: Verify it builds**

Run: `dotnet build JailbreakNew.sln`
Expected: Build succeeded with 0 errors (will warn about empty project until we add code)

**Step 4: Commit**

```bash
git add src/Jailbreak.Updater/Jailbreak.Updater.csproj JailbreakNew.sln
git commit -m "feat(updater): scaffold Jailbreak.Updater project"
```

---

### Task 2: Create UpdaterConfig and UpdateState models

**Files:**
- Create: `src/Jailbreak.Updater/Models/UpdaterConfig.cs`
- Create: `src/Jailbreak.Updater/Models/UpdateState.cs`

**Step 1: Create UpdaterConfig model**

Create `src/Jailbreak.Updater/Models/UpdaterConfig.cs`:

```csharp
using System.Text.Json.Serialization;

namespace Jailbreak.Updater.Models;

public class UpdaterConfig {
  [JsonPropertyName("updateChannel")]
  public string UpdateChannel { get; set; } = "stable";

  [JsonPropertyName("checkOnMapChange")]
  public bool CheckOnMapChange { get; set; } = true;

  [JsonPropertyName("autoApply")]
  public bool AutoApply { get; set; } = true;

  [JsonPropertyName("repository")]
  public string Repository { get; set; } = "edgegamers/Jailbreak";
}
```

**Step 2: Create UpdateState model**

Create `src/Jailbreak.Updater/Models/UpdateState.cs`:

```csharp
using System.Text.Json.Serialization;

namespace Jailbreak.Updater.Models;

public class UpdateState {
  [JsonPropertyName("installedVersion")]
  public string InstalledVersion { get; set; } = "0.0.0";

  [JsonPropertyName("stagedVersion")]
  public string? StagedVersion { get; set; }

  [JsonPropertyName("lastCheck")]
  public DateTime? LastCheck { get; set; }

  [JsonPropertyName("status")]
  public string Status { get; set; } = "idle";
}
```

**Step 3: Verify build**

Run: `dotnet build src/Jailbreak.Updater/Jailbreak.Updater.csproj`
Expected: Build succeeded

**Step 4: Commit**

```bash
git add src/Jailbreak.Updater/Models/
git commit -m "feat(updater): add UpdaterConfig and UpdateState models"
```

---

### Task 3: Create GitHubReleaseProvider service

**Files:**
- Create: `src/Jailbreak.Updater/Services/GitHubReleaseProvider.cs`

This service handles all GitHub API interaction — fetching releases, filtering by channel, downloading assets.

**Step 1: Create the provider**

Create `src/Jailbreak.Updater/Services/GitHubReleaseProvider.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jailbreak.Updater.Services;

public record GitHubRelease(
  [property: JsonPropertyName("tag_name")] string TagName,
  [property: JsonPropertyName("prerelease")] bool Prerelease,
  [property: JsonPropertyName("assets")] GitHubAsset[] Assets,
  [property: JsonPropertyName("published_at")] DateTime PublishedAt
);

public record GitHubAsset(
  [property: JsonPropertyName("name")] string Name,
  [property: JsonPropertyName("browser_download_url")] string DownloadUrl
);

public class GitHubReleaseProvider {
  private static readonly HttpClient Http = new() {
    DefaultRequestHeaders = {
      { "User-Agent", "Jailbreak-Updater" },
      { "Accept", "application/vnd.github+json" }
    }
  };

  private readonly string _repository;

  public GitHubReleaseProvider(string repository) {
    _repository = repository;
  }

  public async Task<GitHubRelease?> GetLatestRelease(bool includePreRelease) {
    var url = $"https://api.github.com/repos/{_repository}/releases";
    var response = await Http.GetAsync(url);

    if (!response.IsSuccessStatusCode) return null;

    var json = await response.Content.ReadAsStringAsync();
    var releases = JsonSerializer.Deserialize<GitHubRelease[]>(json);

    if (releases is null || releases.Length == 0) return null;

    return includePreRelease
      ? releases[0]
      : releases.FirstOrDefault(r => !r.Prerelease);
  }

  public async Task<Stream?> DownloadAsset(GitHubRelease release, string assetName) {
    var asset = release.Assets.FirstOrDefault(a => a.Name == assetName);
    if (asset is null) return null;

    var response = await Http.GetAsync(asset.DownloadUrl);
    if (!response.IsSuccessStatusCode) return null;

    return await response.Content.ReadAsStreamAsync();
  }

  public static string ParseVersion(string tagName) {
    return tagName.TrimStart('v');
  }
}
```

**Step 2: Verify build**

Run: `dotnet build src/Jailbreak.Updater/Jailbreak.Updater.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/Jailbreak.Updater/Services/GitHubReleaseProvider.cs
git commit -m "feat(updater): add GitHub Releases API provider"
```

---

### Task 4: Create UpdateService (core update logic)

**Files:**
- Create: `src/Jailbreak.Updater/Services/IUpdateService.cs`
- Create: `src/Jailbreak.Updater/Services/UpdateService.cs`

This is the core orchestrator — check, download, stage, apply.

**Step 1: Create the interface**

Create `src/Jailbreak.Updater/Services/IUpdateService.cs`:

```csharp
using Jailbreak.Updater.Models;

namespace Jailbreak.Updater.Services;

public interface IUpdateService {
  UpdateState State { get; }
  Task<bool> CheckForUpdate();
  Task ApplyUpdate(string pluginsDirectory);
  void FinalizeIfApplying();
}
```

**Step 2: Create the implementation**

Create `src/Jailbreak.Updater/Services/UpdateService.cs`:

```csharp
using System.IO.Compression;
using System.Text.Json;
using Jailbreak.Updater.Models;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Updater.Services;

public class UpdateService : IUpdateService {
  private readonly GitHubReleaseProvider _github;
  private readonly UpdaterConfig _config;
  private readonly string _dataDir;
  private readonly string _stateFilePath;
  private readonly ILogger _logger;

  public UpdateState State { get; private set; }

  public UpdateService(UpdaterConfig config, string dataDir, ILogger logger) {
    _config = config;
    _dataDir = dataDir;
    _stateFilePath = Path.Combine(dataDir, "update-state.json");
    _logger = logger;
    _github = new GitHubReleaseProvider(config.Repository);

    Directory.CreateDirectory(dataDir);
    State = LoadState();
  }

  public void FinalizeIfApplying() {
    if (State.Status != "applying") return;

    _logger.LogInformation(
      "[Updater] Finalizing update to {Version}", State.StagedVersion);
    State.InstalledVersion = State.StagedVersion!;
    State.StagedVersion = null;
    State.Status = "idle";
    CleanStaging();
    SaveState();
  }

  public async Task<bool> CheckForUpdate() {
    try {
      var includePreRelease = _config.UpdateChannel == "prerelease";
      var release = await _github.GetLatestRelease(includePreRelease);

      if (release is null) {
        _logger.LogWarning("[Updater] Failed to fetch releases");
        return false;
      }

      State.LastCheck = DateTime.UtcNow;

      var remoteVersion = GitHubReleaseProvider.ParseVersion(release.TagName);

      if (!IsNewerVersion(remoteVersion, State.InstalledVersion)) {
        SaveState();
        return false;
      }

      _logger.LogInformation(
        "[Updater] New version available: {Version}", remoteVersion);

      State.Status = "downloading";
      SaveState();

      var stagingDir = Path.Combine(_dataDir, "staging");
      CleanStaging();
      Directory.CreateDirectory(stagingDir);

      var stream = await _github.DownloadAsset(release, "Jailbreak.zip");
      if (stream is null) {
        _logger.LogWarning("[Updater] Failed to download Jailbreak.zip");
        State.Status = "idle";
        SaveState();
        return false;
      }

      var zipPath = Path.Combine(_dataDir, "Jailbreak.zip");
      await using (var fs = File.Create(zipPath)) {
        await stream.CopyToAsync(fs);
      }

      ZipFile.ExtractToDirectory(zipPath, stagingDir, overwriteFiles: true);
      File.Delete(zipPath);

      State.StagedVersion = remoteVersion;
      State.Status = "staged";
      SaveState();

      _logger.LogInformation(
        "[Updater] Version {Version} staged and ready", remoteVersion);
      return true;
    } catch (Exception ex) {
      _logger.LogError(ex, "[Updater] Error checking for updates");
      State.Status = "idle";
      SaveState();
      return false;
    }
  }

  public async Task ApplyUpdate(string pluginsDirectory) {
    if (State.Status != "staged") return;

    State.Status = "applying";
    SaveState();

    var stagingDir = Path.Combine(_dataDir, "staging");

    if (!Directory.Exists(stagingDir)) {
      _logger.LogWarning("[Updater] Staging directory not found");
      State.Status = "idle";
      SaveState();
      return;
    }

    _logger.LogInformation(
      "[Updater] Applying update to {Version}", State.StagedVersion);

    var updaterDir = "Jailbreak.Updater";

    foreach (var pluginDir in Directory.GetDirectories(stagingDir)) {
      var dirName = Path.GetFileName(pluginDir);

      // Skip the updater — copy it last
      if (dirName == updaterDir) continue;

      var targetDir = Path.Combine(pluginsDirectory, dirName);
      CopyDirectory(pluginDir, targetDir);
    }

    // Copy updater last (self-update)
    var stagedUpdater = Path.Combine(stagingDir, updaterDir);
    if (Directory.Exists(stagedUpdater)) {
      var targetUpdater = Path.Combine(pluginsDirectory, updaterDir);
      CopyDirectory(stagedUpdater, targetUpdater);
    }

    SaveState();

    _logger.LogInformation(
      "[Updater] Update applied. Plugins will reload on next map.");
  }

  private static bool IsNewerVersion(string remote, string installed) {
    if (Version.TryParse(NormalizeSemver(remote), out var r)
        && Version.TryParse(NormalizeSemver(installed), out var i))
      return r > i;
    return string.Compare(remote, installed, StringComparison.Ordinal) > 0;
  }

  private static string NormalizeSemver(string version) {
    // Strip pre-release suffix for comparison (e.g., "2.3.0-beta.1" -> "2.3.0")
    // For prerelease channel, "2.3.0-beta.2" > "2.3.0-beta.1" via string compare
    var dash = version.IndexOf('-');
    return dash >= 0 ? version[..dash] : version;
  }

  private void CleanStaging() {
    var stagingDir = Path.Combine(_dataDir, "staging");
    if (Directory.Exists(stagingDir))
      Directory.Delete(stagingDir, recursive: true);
  }

  private static void CopyDirectory(string source, string target) {
    Directory.CreateDirectory(target);

    foreach (var file in Directory.GetFiles(source)) {
      var dest = Path.Combine(target, Path.GetFileName(file));
      File.Copy(file, dest, overwrite: true);
    }

    foreach (var dir in Directory.GetDirectories(source)) {
      var dest = Path.Combine(target, Path.GetFileName(dir));
      CopyDirectory(dir, dest);
    }
  }

  private UpdateState LoadState() {
    if (!File.Exists(_stateFilePath)) return new UpdateState();

    try {
      var json = File.ReadAllText(_stateFilePath);
      return JsonSerializer.Deserialize<UpdateState>(json) ?? new UpdateState();
    } catch {
      return new UpdateState();
    }
  }

  private void SaveState() {
    var json = JsonSerializer.Serialize(State, new JsonSerializerOptions {
      WriteIndented = true
    });
    File.WriteAllText(_stateFilePath, json);
  }
}
```

**Step 3: Verify build**

Run: `dotnet build src/Jailbreak.Updater/Jailbreak.Updater.csproj`
Expected: Build succeeded

**Step 4: Commit**

```bash
git add src/Jailbreak.Updater/Services/IUpdateService.cs src/Jailbreak.Updater/Services/UpdateService.cs
git commit -m "feat(updater): add UpdateService with check/download/stage/apply lifecycle"
```

---

### Task 5: Create UpdateCommands (admin commands)

**Files:**
- Create: `src/Jailbreak.Updater/Commands/UpdateCommands.cs`

**Step 1: Create the commands**

Create `src/Jailbreak.Updater/Commands/UpdateCommands.cs`:

```csharp
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Updater.Services;

namespace Jailbreak.Updater.Commands;

public class UpdateCommands {
  private readonly IUpdateService _updateService;
  private readonly string _pluginsDirectory;

  public UpdateCommands(IUpdateService updateService, string pluginsDirectory) {
    _updateService = updateService;
    _pluginsDirectory = pluginsDirectory;
  }

  public void OnUpdateCommand(CCSPlayerController? executor, CommandInfo info) {
    if (info.ArgCount < 2) {
      info.ReplyToCommand("[Updater] Usage: css_update <check|apply|status>");
      return;
    }

    var subcommand = info.GetArg(1).ToLower();

    switch (subcommand) {
      case "check":
        _ = Task.Run(async () => {
          var found = await _updateService.CheckForUpdate();
          // CSSharp will log the result via ILogger in UpdateService
        });
        info.ReplyToCommand("[Updater] Checking for updates...");
        break;

      case "apply":
        if (_updateService.State.Status != "staged") {
          info.ReplyToCommand("[Updater] No staged update to apply.");
          return;
        }
        info.ReplyToCommand(
          $"[Updater] Will apply v{_updateService.State.StagedVersion} on map end.");
        break;

      case "status":
        var state = _updateService.State;
        info.ReplyToCommand($"[Updater] Installed: v{state.InstalledVersion}");
        if (state.StagedVersion != null)
          info.ReplyToCommand($"[Updater] Staged: v{state.StagedVersion}");
        info.ReplyToCommand($"[Updater] Status: {state.Status}");
        if (state.LastCheck.HasValue)
          info.ReplyToCommand($"[Updater] Last check: {state.LastCheck.Value:u}");
        break;

      default:
        info.ReplyToCommand("[Updater] Unknown subcommand. Use: check, apply, status");
        break;
    }
  }

  public void OnVersionCommand(CCSPlayerController? executor, CommandInfo info) {
    info.ReplyToCommand(
      $"[Jailbreak] Installed version: v{_updateService.State.InstalledVersion}");
  }
}
```

**Step 2: Verify build**

Run: `dotnet build src/Jailbreak.Updater/Jailbreak.Updater.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/Jailbreak.Updater/Commands/UpdateCommands.cs
git commit -m "feat(updater): add css_update and css_version admin commands"
```

---

### Task 6: Create UpdaterPlugin entry point

**Files:**
- Create: `src/Jailbreak.Updater/UpdaterPlugin.cs`

This wires everything together — loads config, creates services, registers events and commands.

**Step 1: Create the plugin entry point**

Create `src/Jailbreak.Updater/UpdaterPlugin.cs`:

```csharp
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Updater.Commands;
using Jailbreak.Updater.Models;
using Jailbreak.Updater.Services;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Updater;

public class UpdaterPlugin : BasePlugin {
  public override string ModuleName => "Jailbreak Updater";
  public override string ModuleVersion => "2.0.0";
  public override string ModuleAuthor => "EdgeGamers Development";

  private UpdateService? _updateService;
  private UpdateCommands? _commands;
  private UpdaterConfig _config = new();

  public override void Load(bool hotReload) {
    var pluginDir = Path.GetDirectoryName(ModulePath)!;
    var dataDir = Path.Combine(pluginDir, "data");
    var configPath = Path.Combine(pluginDir, "config.json");

    _config = LoadOrCreateConfig(configPath);
    _updateService = new UpdateService(_config, dataDir, Logger);

    // Resume from self-update: if state is "applying", finalize it
    _updateService.FinalizeIfApplying();

    // Set installed version from plugin metadata on first run
    if (_updateService.State.InstalledVersion == "0.0.0") {
      _updateService.State.InstalledVersion = ModuleVersion;
    }

    var pluginsDirectory = Path.GetDirectoryName(pluginDir)!;
    _commands = new UpdateCommands(_updateService, pluginsDirectory);

    AddCommand("css_update", "Manage Jailbreak updates", OnUpdateCommand);
    AddCommand("css_version", "Show Jailbreak version", OnVersionCommand);

    RegisterEventHandler<EventRoundEnd>(OnRoundEnd);

    // Check for updates on load
    _ = Task.Run(async () => {
      await _updateService.CheckForUpdate();
      if (_updateService.State.Status == "staged") {
        Logger.LogInformation(
          "[Updater] v{Version} is staged and will apply on map end.",
          _updateService.State.StagedVersion);
      }
    });

    if (_config.CheckOnMapChange) {
      RegisterListener<Listeners.OnMapEnd>(() => {
        // Apply staged update before map fully ends
        if (_updateService.State.Status == "staged" && _config.AutoApply) {
          _updateService.ApplyUpdate(pluginsDirectory).GetAwaiter().GetResult();
        }
      });

      RegisterListener<Listeners.OnMapStart>(mapName => {
        // Check for updates on each map start
        _ = Task.Run(async () => await _updateService.CheckForUpdate());
      });
    }
  }

  [RequiresPermissions("@css/root")]
  private void OnUpdateCommand(CCSPlayerController? executor, CommandInfo info) {
    _commands?.OnUpdateCommand(executor, info);
  }

  [RequiresPermissions("@css/root")]
  private void OnVersionCommand(CCSPlayerController? executor, CommandInfo info) {
    _commands?.OnVersionCommand(executor, info);
  }

  private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    return HookResult.Continue;
  }

  public override void Unload(bool hotReload) { }

  private static UpdaterConfig LoadOrCreateConfig(string path) {
    if (File.Exists(path)) {
      try {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<UpdaterConfig>(json) ?? new UpdaterConfig();
      } catch {
        return new UpdaterConfig();
      }
    }

    var config = new UpdaterConfig();
    var defaultJson = JsonSerializer.Serialize(config, new JsonSerializerOptions {
      WriteIndented = true
    });
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, defaultJson);
    return config;
  }
}
```

**Step 2: Verify full solution builds**

Run: `dotnet build JailbreakNew.sln`
Expected: Build succeeded with 0 errors

**Step 3: Commit**

```bash
git add src/Jailbreak.Updater/UpdaterPlugin.cs
git commit -m "feat(updater): add UpdaterPlugin entry point with event registration"
```

---

### Task 7: Update CI workflows to include Jailbreak.Updater

**Files:**
- Modify: `.github/workflows/nightly.yml` (add Updater publish step)
- Modify: `.github/workflows/release.yml` (add Updater publish step)
- Modify: `README.md` (add Updater to architecture table and build instructions)

**Step 1: Add Updater to nightly.yml**

In `.github/workflows/nightly.yml`, in the "Publish plugins" step (line 37), add after the Tools line:

```yaml
        dotnet publish src/Jailbreak.Updater/Jailbreak.Updater.csproj --no-build --no-restore -o build/Jailbreak.Updater
```

**Step 2: Add Updater to release.yml**

In `.github/workflows/release.yml`, in the "Publish plugins" step (line 31), add after the Tools line:

```yaml
          dotnet publish src/Jailbreak.Updater/Jailbreak.Updater.csproj --no-build --no-restore -o build/Jailbreak.Updater
```

**Step 3: Update README.md**

Add Jailbreak.Updater to the architecture table (after Jailbreak.Tools row):

```markdown
| **Jailbreak.Updater** | Auto-updates plugins from GitHub Releases |
```

Add to the building section publish commands:

```shell
dotnet publish src/Jailbreak.Updater/Jailbreak.Updater.csproj -o build/Jailbreak.Updater
```

**Step 4: Verify CI files are valid YAML**

Run: `python3 -c "import yaml; yaml.safe_load(open('.github/workflows/nightly.yml')); yaml.safe_load(open('.github/workflows/release.yml')); print('YAML valid')"`
Expected: YAML valid (or install pyyaml first with `pip install pyyaml`)

**Step 5: Commit**

```bash
git add .github/workflows/nightly.yml .github/workflows/release.yml README.md
git commit -m "ci: add Jailbreak.Updater to build and publish pipelines"
```

---

### Task 8: Initialize net-changesets

**Files:**
- Create: `.changeset/config.json`

**Step 1: Install net-changesets globally**

Run: `dotnet tool install solarwinds.changesets --global`
Expected: Tool 'solarwinds.changesets' installed successfully

**Step 2: Initialize changesets**

Run: `cd /home/gkh/projects/Jailbreak && changesets init`
Expected: Creates `.changeset/` directory with default config

**Step 3: Edit config for our project**

Modify `.changeset/config.json` to:

```json
{
  "sourcePath": "src",
  "packageSource": "nuget"
}
```

Note: `sourcePath` points to our `src/` directory where all .csproj files live. `packageSource` is required but we won't use `changesets publish` — we publish DLL zips via GitHub Releases instead.

**Step 4: Commit**

```bash
git add .changeset/
git commit -m "build: initialize net-changesets for version management"
```

---

### Task 9: Add lock-step version sync script

**Files:**
- Create: `scripts/sync-versions.sh`

Since net-changesets lacks native `fixed` group support, this script ensures all plugins share the same version after `changesets version` runs.

**Step 1: Create the sync script**

Create `scripts/sync-versions.sh`:

```bash
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
```

**Step 2: Make executable**

Run: `chmod +x scripts/sync-versions.sh`

**Step 3: Commit**

```bash
git add scripts/sync-versions.sh
git commit -m "build: add lock-step version sync script for changesets"
```

---

### Task 10: Create changesets CI workflow

**Files:**
- Create: `.github/workflows/changesets.yml`

This workflow handles the Version PR and release publishing flow.

**Step 1: Create the changesets workflow**

Create `.github/workflows/changesets.yml`:

```yaml
name: Changesets

on:
  push:
    branches:
      - main

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

jobs:
  version:
    runs-on: ubuntu-latest
    permissions:
      contents: write
      pull-requests: write
    outputs:
      has_changesets: ${{ steps.check.outputs.has_changesets }}
      published: ${{ steps.release.outputs.published }}
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - name: Install changesets
        run: dotnet tool install solarwinds.changesets --global

      - name: Check for changesets
        id: check
        run: |
          if changesets status 2>/dev/null; then
            echo "has_changesets=true" >> "$GITHUB_OUTPUT"
          else
            echo "has_changesets=false" >> "$GITHUB_OUTPUT"
          fi

      - name: Run changesets version
        if: steps.check.outputs.has_changesets == 'true'
        run: changesets version

      - name: Sync lock-step versions
        if: steps.check.outputs.has_changesets == 'true'
        run: bash scripts/sync-versions.sh

      - name: Create Version PR or publish release
        if: steps.check.outputs.has_changesets == 'true'
        uses: peter-evans/create-pull-request@v6
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          commit-message: "chore: version packages"
          title: "chore: version packages"
          body: |
            This PR was auto-generated by the changesets workflow.
            Merging this PR will trigger a stable release.
          branch: changeset-release/main
          delete-branch: true

  release:
    runs-on: ubuntu-latest
    needs: version
    if: |
      !contains(github.event.head_commit.message, 'chore: version packages') == false
    permissions:
      contents: write
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - name: Build all plugins
        run: |
          dotnet restore
          dotnet build --no-restore

      - name: Publish plugins
        run: |
          dotnet publish src/Jailbreak.Core/Jailbreak.Core.csproj --no-build --no-restore -o build/Jailbreak.Core
          dotnet publish src/Jailbreak.LastRequest/Jailbreak.LastRequest.csproj --no-build --no-restore -o build/Jailbreak.LastRequest
          dotnet publish src/Jailbreak.Fun/Jailbreak.Fun.csproj --no-build --no-restore -o build/Jailbreak.Fun
          dotnet publish src/Jailbreak.Zones/Jailbreak.Zones.csproj --no-build --no-restore -o build/Jailbreak.Zones
          dotnet publish src/Jailbreak.Gangs/Jailbreak.Gangs.csproj --no-build --no-restore -o build/Jailbreak.Gangs
          dotnet publish src/Jailbreak.Tools/Jailbreak.Tools.csproj --no-build --no-restore -o build/Jailbreak.Tools
          dotnet publish src/Jailbreak.Updater/Jailbreak.Updater.csproj --no-build --no-restore -o build/Jailbreak.Updater

      - name: Get version
        id: version
        run: |
          VERSION=$(grep -oP '<Version>\K[^<]+' src/Jailbreak.Core/Jailbreak.Core.csproj || echo "0.0.0")
          echo "version=$VERSION" >> "$GITHUB_OUTPUT"

      - name: Create release zip
        run: cd build && zip -r ../Jailbreak.zip . && cd ..

      - name: Release
        uses: softprops/action-gh-release@v1
        with:
          tag_name: v${{ steps.version.outputs.version }}
          name: v${{ steps.version.outputs.version }}
          fail_on_unmatched_files: true
          files: Jailbreak.zip
          generate_release_notes: true

  prerelease:
    runs-on: ubuntu-latest
    needs: version
    if: needs.version.outputs.has_changesets == 'true'
    permissions:
      contents: write
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - name: Install changesets
        run: dotnet tool install solarwinds.changesets --global

      - name: Version (for pre-release build)
        run: |
          changesets version
          bash scripts/sync-versions.sh

      - name: Build all plugins
        run: |
          dotnet restore
          dotnet build --no-restore

      - name: Publish plugins
        run: |
          dotnet publish src/Jailbreak.Core/Jailbreak.Core.csproj --no-build --no-restore -o build/Jailbreak.Core
          dotnet publish src/Jailbreak.LastRequest/Jailbreak.LastRequest.csproj --no-build --no-restore -o build/Jailbreak.LastRequest
          dotnet publish src/Jailbreak.Fun/Jailbreak.Fun.csproj --no-build --no-restore -o build/Jailbreak.Fun
          dotnet publish src/Jailbreak.Zones/Jailbreak.Zones.csproj --no-build --no-restore -o build/Jailbreak.Zones
          dotnet publish src/Jailbreak.Gangs/Jailbreak.Gangs.csproj --no-build --no-restore -o build/Jailbreak.Gangs
          dotnet publish src/Jailbreak.Tools/Jailbreak.Tools.csproj --no-build --no-restore -o build/Jailbreak.Tools
          dotnet publish src/Jailbreak.Updater/Jailbreak.Updater.csproj --no-build --no-restore -o build/Jailbreak.Updater

      - name: Get version
        id: version
        run: |
          VERSION=$(grep -oP '<Version>\K[^<]+' src/Jailbreak.Core/Jailbreak.Core.csproj || echo "0.0.0")
          echo "version=$VERSION" >> "$GITHUB_OUTPUT"

      - name: Create release zip
        run: cd build && zip -r ../Jailbreak.zip . && cd ..

      - name: Pre-release
        uses: softprops/action-gh-release@v1
        with:
          tag_name: v${{ steps.version.outputs.version }}-beta.${{ github.run_number }}
          name: v${{ steps.version.outputs.version }}-beta.${{ github.run_number }}
          prerelease: true
          fail_on_unmatched_files: true
          files: Jailbreak.zip
          generate_release_notes: true
```

**Step 2: Verify YAML is valid**

Run: `python3 -c "import yaml; yaml.safe_load(open('.github/workflows/changesets.yml')); print('Valid')"`

**Step 3: Commit**

```bash
git add .github/workflows/changesets.yml
git commit -m "ci: add changesets workflow for version PRs and releases"
```

---

### Task 11: Update old workflows for new flow

**Files:**
- Modify: `.github/workflows/release.yml` (remove or repurpose — changesets.yml handles releases now)
- Modify: `.github/workflows/nightly.yml` (keep for PR/push CI builds, remove release concerns)

**Step 1: Remove release.yml**

The changesets workflow now handles releases. Delete the old tag-triggered release workflow:

Run: `rm .github/workflows/release.yml`

**Step 2: Simplify nightly.yml**

Keep `nightly.yml` as the CI build check (runs on push/PR), but remove the webhook job if no longer needed, and ensure it includes the Updater plugin. The nightly workflow from Task 7 already includes the Updater — verify it's correct.

**Step 3: Commit**

```bash
git rm .github/workflows/release.yml
git add .github/workflows/nightly.yml
git commit -m "ci: remove old release workflow, changesets handles releases now"
```

---

### Task 12: Add Version property to all .csproj files

**Files:**
- Modify: `src/Jailbreak.Contracts/Jailbreak.Contracts.csproj`
- Modify: `src/Jailbreak.Core/Jailbreak.Core.csproj`
- Modify: `src/Jailbreak.LastRequest/Jailbreak.LastRequest.csproj`
- Modify: `src/Jailbreak.Fun/Jailbreak.Fun.csproj`
- Modify: `src/Jailbreak.Zones/Jailbreak.Zones.csproj`
- Modify: `src/Jailbreak.Gangs/Jailbreak.Gangs.csproj`
- Modify: `src/Jailbreak.Tools/Jailbreak.Tools.csproj`
- (Jailbreak.Updater already created with no Version — will be added)

net-changesets reads and writes `<Version>` in .csproj files. Currently none of the projects have this property — version is only in `ModuleVersion` in the plugin C# code.

**Step 1: Add `<Version>2.0.0</Version>` to all .csproj PropertyGroups**

For each .csproj file, add `<Version>2.0.0</Version>` inside the `<PropertyGroup>` block, after `<Nullable>enable</Nullable>`.

Example for `src/Jailbreak.Tools/Jailbreak.Tools.csproj`:
```xml
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <Version>2.0.0</Version>
</PropertyGroup>
```

Repeat for all 8 .csproj files.

**Step 2: Verify build**

Run: `dotnet build JailbreakNew.sln`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/*/Jailbreak.*.csproj
git commit -m "build: add Version property to all .csproj files for changesets"
```

---

### Task 13: Final build verification and cleanup

**Step 1: Clean build**

Run: `dotnet clean JailbreakNew.sln && dotnet build JailbreakNew.sln`
Expected: Build succeeded with 0 errors

**Step 2: Verify all 8 projects build**

Run: `dotnet build JailbreakNew.sln -v minimal 2>&1 | grep -c "succeeded"`
Expected: 8 (one per project)

**Step 3: Verify publish works for Updater**

Run: `dotnet publish src/Jailbreak.Updater/Jailbreak.Updater.csproj -o /tmp/jb-updater-test`
Expected: Publish succeeded, `/tmp/jb-updater-test/Jailbreak.Updater.dll` exists

**Step 4: Commit any remaining changes**

```bash
git add -A
git status  # Verify nothing unexpected
git commit -m "chore: final cleanup for updater and changesets integration"
```
