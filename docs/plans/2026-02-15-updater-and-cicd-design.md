# Jailbreak Updater Plugin & CI/CD Migration Design

## Overview

Two related changes to the Jailbreak project:

1. **CI/CD migration** from manual git tagging to net-changesets for version management and changelog generation
2. **Jailbreak.Updater plugin** that automatically updates all plugins on game servers by polling GitHub Releases

## CI/CD: net-changesets with Lock-Step Versioning

### Why net-changesets

Replace the current manual tag-and-release workflow with PR-driven version management. net-changesets provides:

- Changeset files in PRs that describe what changed and the semver bump type
- Automated version bumping across all `.csproj` files
- Auto-generated changelogs with per-plugin granularity
- A "Version Packages" PR that stages the next release

### Lock-Step Versioning

All plugins share a single version number via a `fixed` group in `.changeset/config.json`:

```json
{
  "fixed": [
    [
      "Jailbreak.Contracts",
      "Jailbreak.Core",
      "Jailbreak.LastRequest",
      "Jailbreak.Fun",
      "Jailbreak.Zones",
      "Jailbreak.Gangs",
      "Jailbreak.Tools",
      "Jailbreak.Updater"
    ]
  ],
  "changelog": true,
  "access": "public",
  "baseBranch": "main"
}
```

A change to any plugin bumps the entire suite. Changelogs remain granular per-plugin.

### Release Flow

```
Developer pushes PR with .changeset/cool-feature.md
  -> PR merges to main
  -> CI runs `changesets version` -> opens/updates Version PR
  -> Version PR triggers pre-release build -> v2.3.0-beta.1 (GitHub pre-release)
  -> Dev servers (updateChannel: "prerelease") pick it up
  -> Version PR merges to main
  -> CI builds stable release -> v2.3.0 (GitHub release)
  -> Production servers (updateChannel: "stable") pick it up
```

### Changelog Format

```md
## 2.3.0

### Jailbreak.Core
- Added new warden marker type

### Jailbreak.Fun
- Fixed RTD crash on round end
```

## Jailbreak.Updater Plugin

### Architecture

```
Jailbreak.Updater/
├── UpdaterPlugin.cs              # Entry point, registers events + commands
├── Services/
│   ├── IUpdateService.cs         # Interface for update operations
│   ├── UpdateService.cs          # Core logic: check, download, stage, apply
│   ├── GitHubReleaseProvider.cs  # GitHub Releases API client
│   └── UpdateState.cs            # State model (serialized to JSON)
├── Commands/
│   └── UpdateCommands.cs         # Admin commands (css_update, css_version)
├── Models/
│   └── UpdaterConfig.cs          # Config model
└── Jailbreak.Updater.csproj
```

### Dependencies

```xml
<PackageReference Include="CounterStrikeSharp.API" Version="1.0.342" />
<ProjectReference Include="..\Jailbreak.Contracts\Jailbreak.Contracts.csproj" />
```

No additional NuGet packages. Uses .NET 8 stdlib:

- `System.Net.Http.HttpClient` for HTTP
- `System.Text.Json` for JSON parsing (GitHub API, config, state)
- `System.IO.Compression.ZipFile` for zip extraction
- `System.Version` for semver comparison

### Configuration

`config.json` in the plugin directory:

```json
{
  "updateChannel": "stable",
  "checkOnMapChange": true,
  "autoApply": true,
  "repository": "edgegamers/Jailbreak"
}
```

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `updateChannel` | string | `"stable"` | `"stable"` for tagged releases only, `"prerelease"` to include beta builds |
| `checkOnMapChange` | bool | `true` | Poll GitHub Releases API on each map change |
| `autoApply` | bool | `true` | Auto-apply staged updates on `OnMapEnd`. If `false`, admins must run `css_update apply` |
| `repository` | string | `"edgegamers/Jailbreak"` | GitHub `owner/repo` to check for releases |

### State File

`data/update-state.json` in the plugin directory:

```json
{
  "installedVersion": "2.1.0",
  "stagedVersion": null,
  "lastCheck": "2026-02-15T12:00:00Z",
  "status": "idle"
}
```

Status transitions: `idle -> downloading -> staged -> applying -> idle`

### Update Lifecycle

**1. Check** (server start, map change, or `css_update check`):

- `GET /repos/{owner}/{repo}/releases` from GitHub API
- Filter by `updateChannel` (skip pre-releases if `"stable"`)
- Compare latest release tag against `installedVersion`
- No update available: done
- New version found: begin download

**2. Download:**

- Fetch `Jailbreak.zip` asset from the release
- Extract to `data/staging/`
- Set state to `"staged"`, record `stagedVersion`
- Announce to admins: "Jailbreak v2.3.0 staged. Will apply on map end."

**3. Apply** (`OnMapEnd` if `autoApply`, or `css_update apply`):

- Set state to `"applying"`
- Copy each plugin folder from `staging/` to sibling plugin directories
- Copy `Jailbreak.Updater` last (self-update)
- CSSharp hot-reloads all plugins on next map load

**4. Resume** (after updater reloads from self-update):

- Read state file on plugin load
- If `"applying"`: update succeeded, set `installedVersion = stagedVersion`, clear staging, set `"idle"`
- If `"staged"`: update not yet applied, wait for next `OnMapEnd`

### Directory Layout

```
csgo/addons/counterstrikesharp/plugins/
├── Jailbreak.Core/
├── Jailbreak.LastRequest/
├── Jailbreak.Fun/
├── Jailbreak.Zones/
├── Jailbreak.Gangs/
├── Jailbreak.Tools/
└── Jailbreak.Updater/
    ├── Jailbreak.Updater.dll
    ├── config.json
    └── data/
        ├── update-state.json
        └── staging/
            ├── Jailbreak.Core/
            ├── Jailbreak.Fun/
            ├── ...
            └── Jailbreak.Updater/
```

### Admin Commands

| Command | Description |
|---------|-------------|
| `css_update check` | Force a version check now |
| `css_update apply` | Apply staged update on next map end |
| `css_update status` | Show current/staged versions and state |
| `css_version` | Print installed version of all plugins |

### Edge Cases

| Scenario | Behavior |
|----------|----------|
| Server reboots mid-download | State is `"downloading"`, re-checks on next start |
| Updater restarts mid-apply (self-update) | State is `"applying"`, new updater finalizes on load |
| No network | Check fails silently, logs warning, retries next map change |
| Same version re-released | Compares tags, no-op if version matches |
| Downgrade attempt | Only updates if remote version > installed version |

## Decisions Made

- **Approach**: GitHub Releases polling (over NuGet feed or webhook-push)
- **Versioning**: Lock-step via changesets `fixed` group (over per-plugin independent versions)
- **Update granularity**: Whole bundle (over per-plugin updates)
- **Staging**: Inside updater's own data folder (self-contained)
- **Apply timing**: `OnMapEnd` (safe point between gameplay sessions)
- **Self-update**: Included in bundle, applied last, state file enables resume
