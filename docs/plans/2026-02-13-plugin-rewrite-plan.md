# Jailbreak Plugin Rewrite Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Restructure the Jailbreak CSSharp plugin from a 30-project monolith into 7 independent CSSharp plugins with shared contracts.

**Architecture:** Each plugin is a standalone `BasePlugin` with its own `.csproj`. Cross-plugin communication via `PluginCapability<IJailbreakCore>`. Shared interfaces and formatting live in `Jailbreak.Contracts`. See `docs/plans/2026-02-13-plugin-rewrite-design.md` for full design.

**Tech Stack:** .NET 8.0, CounterStrikeSharp.API 1.0.342+, C# 12

**Verification:** This is a CSSharp game server plugin with no unit test framework. Verification at each step is "does it compile" (`dotnet build`). Full functional testing happens on a live CS2 server.

---

## Phase 0: Setup

### Task 1: Create rewrite branch and fix .gitignore

**Files:**
- Modify: `.gitignore`

**Step 1: Create branch**

```bash
git checkout -b rewrite/multi-plugin
```

**Step 2: Fix the corrupted .gitignore**

Line 13 has `*.user` encoded in UTF-16 with null bytes, causing git to interpret `*` as matching everything. Replace the file with a clean version preserving the same rules:

```gitignore
# Repository build files
build/
bin/
obj/

Debug/
Release/

# IDE files
.idea/
.vs/
*.user
```

**Step 3: Commit**

```bash
git add .gitignore
git commit -m "fix: repair corrupted .gitignore (UTF-16 null bytes in *.user pattern)"
```

### Task 2: Create new solution structure

**Files:**
- Create: `src/Jailbreak.Contracts/Jailbreak.Contracts.csproj`
- Create: `src/Jailbreak.Core/Jailbreak.Core.csproj`
- Create: `src/Jailbreak.LastRequest/Jailbreak.LastRequest.csproj`
- Create: `src/Jailbreak.Fun/Jailbreak.Fun.csproj`
- Create: `src/Jailbreak.Zones/Jailbreak.Zones.csproj`
- Create: `src/Jailbreak.Gangs/Jailbreak.Gangs.csproj`
- Create: `src/Jailbreak.Tools/Jailbreak.Tools.csproj`

**Step 1: Create directory scaffolding**

For each plugin, create the standard folder structure:
```
src/Jailbreak.<Name>/
  Services/
  Commands/
  Events/
  Locale/
  Models/
```

**Step 2: Create Jailbreak.Contracts.csproj**

This is a class library (not a plugin), no BasePlugin:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CounterStrikeSharp.API" Version="1.0.342" />
  </ItemGroup>
</Project>
```

**Step 3: Create each plugin .csproj**

Each plugin project references Contracts. Example for Core:

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

Repeat for LastRequest, Fun, Zones, Gangs, Tools — each referencing only Contracts.

Core additionally needs: `CS2ScreenMenuAPI`, `CS2TraceRay`, and the Mixin DLLs (`GangsAPI.dll`, `MAULActainShared.dll`, `MStatsShared.dll`). Copy the `Mixin/` directory to `src/Jailbreak.Contracts/Mixin/` so all plugins can reference it.

**Step 4: Create new Jailbreak.sln**

Replace the existing solution file. Add all 8 projects under a flat `src/` solution folder.

```bash
dotnet new sln --name Jailbreak --force
dotnet sln add src/Jailbreak.Contracts/Jailbreak.Contracts.csproj
dotnet sln add src/Jailbreak.Core/Jailbreak.Core.csproj
dotnet sln add src/Jailbreak.LastRequest/Jailbreak.LastRequest.csproj
dotnet sln add src/Jailbreak.Fun/Jailbreak.Fun.csproj
dotnet sln add src/Jailbreak.Zones/Jailbreak.Zones.csproj
dotnet sln add src/Jailbreak.Gangs/Jailbreak.Gangs.csproj
dotnet sln add src/Jailbreak.Tools/Jailbreak.Tools.csproj
```

**Step 5: Verify empty solution builds**

```bash
dotnet build
```

Expected: all 8 projects build with 0 errors (they're empty).

**Step 6: Commit**

```bash
git add -A
git commit -m "scaffold: create new multi-plugin solution structure"
```

---

## Phase 1: Jailbreak.Contracts

The foundation. Every other plugin depends on this. Migrate shared interfaces, formatting core, and define the cross-plugin API.

### Task 3: Migrate formatting core

**Files:**
- Create: `src/Jailbreak.Contracts/Formatting/IView.cs`
- Create: `src/Jailbreak.Contracts/Formatting/FormatObject.cs`
- Create: `src/Jailbreak.Contracts/Formatting/FormatWriter.cs`
- Create: `src/Jailbreak.Contracts/Formatting/SimpleView.cs`
- Create: `src/Jailbreak.Contracts/Formatting/ViewExtensions.cs`
- Create: `src/Jailbreak.Contracts/Formatting/Objects/*.cs`

**Step 1: Copy formatting files**

Copy from `public/Jailbreak.Formatting/` into `src/Jailbreak.Contracts/Formatting/`:
- `Base/IView.cs` → `Formatting/IView.cs`
- `Core/FormatObject.cs` → `Formatting/FormatObject.cs`
- `Core/FormatWriter.cs` → `Formatting/FormatWriter.cs`
- `Base/SimpleView.cs` → `Formatting/SimpleView.cs`
- `Extensions/ViewExtensions.cs` → `Formatting/ViewExtensions.cs`
- `Objects/*.cs` → `Formatting/Objects/*.cs`
- `Logistics/IDialect.cs`, `ILanguage.cs`, `Languages/English.cs` → `Formatting/Languages/`

**Step 2: Update namespaces**

All files: change namespace from `Jailbreak.Formatting.*` to `Jailbreak.Contracts.Formatting.*`

**Step 3: Verify build**

```bash
dotnet build src/Jailbreak.Contracts/Jailbreak.Contracts.csproj
```

**Step 4: Commit**

```bash
git add src/Jailbreak.Contracts/Formatting/
git commit -m "feat(contracts): migrate formatting core from Jailbreak.Formatting"
```

### Task 4: Create IJailbreakCore and shared contracts

**Files:**
- Create: `src/Jailbreak.Contracts/IJailbreakCore.cs`
- Create: `src/Jailbreak.Contracts/JailbreakApi.cs`
- Create: `src/Jailbreak.Contracts/Models/RoundState.cs`
- Create: `src/Jailbreak.Contracts/Services/IWardenService.cs`
- Create: `src/Jailbreak.Contracts/Services/IWeaponService.cs`

**Step 1: Create RoundState enum**

```csharp
namespace Jailbreak.Contracts.Models;

public enum RoundState {
    Normal,
    SpecialDay,
    LastRequest
}
```

**Step 2: Create cross-plugin service interfaces**

Only interfaces that satellite plugins need to consume. Migrate from `public/Jailbreak.Public/Mod/Warden/IWardenService.cs` — keep only the methods that other plugins actually call. Refer to the existing interface to determine the surface.

Similarly for `IWeaponService` — extract from existing weapon-related extension methods in `public/Jailbreak.Public/Extensions/`.

**Step 3: Create IJailbreakCore**

```csharp
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Models;
using Jailbreak.Contracts.Services;

namespace Jailbreak.Contracts;

public interface IJailbreakCore {
    RoundState RoundState { get; }
    void SetRoundState(RoundState state);

    IEnumerable<CCSPlayerController> GetAlivePlayers();
    IEnumerable<CCSPlayerController> GetAlivePlayers(CsTeam team);
    bool IsRebel(CCSPlayerController player);
    void MarkRebel(CCSPlayerController player);

    IWardenService Warden { get; }
    IWeaponService Weapons { get; }

    void Announce(string message);
    void AnnounceCenter(string message);
    void Message(CCSPlayerController player, string message);
}
```

**Step 4: Create JailbreakApi**

```csharp
using CounterStrikeSharp.API.Core.Capabilities;

namespace Jailbreak.Contracts;

public static class JailbreakApi {
    public static PluginCapability<IJailbreakCore> Core { get; } =
        new("jailbreak:core");
}
```

**Step 5: Verify build**

```bash
dotnet build src/Jailbreak.Contracts/Jailbreak.Contracts.csproj
```

**Step 6: Commit**

```bash
git add src/Jailbreak.Contracts/
git commit -m "feat(contracts): add IJailbreakCore, JailbreakApi, and shared service interfaces"
```

### Task 5: Migrate cross-plugin extensible type interfaces

**Files:**
- Create: `src/Jailbreak.Contracts/Services/ILastRequest.cs`
- Create: `src/Jailbreak.Contracts/Services/ISpecialDay.cs`
- Create: `src/Jailbreak.Contracts/Services/IRTDReward.cs`
- Create: `src/Jailbreak.Contracts/Services/IGangPerk.cs`
- Create: `src/Jailbreak.Contracts/Services/IBeamShape.cs`

**Step 1: Define each extensible type interface**

These are the contracts that implementations in satellite plugins will fulfill. Refer to the existing abstract classes in `public/Jailbreak.Public/Mod/` for method signatures, but simplify.

Each interface should accept `IJailbreakCore` in its methods so implementations can interact with core state.

**Step 2: Migrate shared extension methods**

Copy useful extension methods from `public/Jailbreak.Public/Extensions/` to `src/Jailbreak.Contracts/Extensions/`. Only the ones that are genuinely shared across plugins (player queries, entity helpers).

**Step 3: Migrate Tag system**

Copy `public/Jailbreak.Tag/` (2 files) into `src/Jailbreak.Contracts/` if needed by other plugins, or into Core if only Core uses it.

**Step 4: Migrate Validator**

Copy `public/Jailbreak.Validator/` (3 files) into `src/Jailbreak.Contracts/` if shared, or into the plugin that uses it (LastRequest and RTD).

**Step 5: Verify build**

```bash
dotnet build src/Jailbreak.Contracts/Jailbreak.Contracts.csproj
```

**Step 6: Commit**

```bash
git add src/Jailbreak.Contracts/
git commit -m "feat(contracts): add extensible type interfaces and shared utilities"
```

---

## Phase 2: Jailbreak.Core

The main plugin. Implements `IJailbreakCore`, contains Warden, Rebel, Mute, Logs, and state tracking.

### Task 6: Create CorePlugin entry point and IJailbreakCore implementation

**Files:**
- Create: `src/Jailbreak.Core/CorePlugin.cs`
- Create: `src/Jailbreak.Core/Services/JailbreakCoreService.cs`

**Step 1: Create CorePlugin.cs**

```csharp
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using Jailbreak.Contracts;

namespace Jailbreak.Core;

public class CorePlugin : BasePlugin {
    public override string ModuleName => "Jailbreak Core";
    public override string ModuleVersion => "2.0.0";
    public override string ModuleAuthor => "EdgeGamers Development";

    public override void Load(bool hotReload) {
        // Register IJailbreakCore capability
        // Initialize services via DI
        // Register event handlers and commands
    }

    public override void Unload(bool hotReload) {
        // Cleanup
    }
}
```

**Step 2: Create JailbreakCoreService implementing IJailbreakCore**

This is the concrete implementation that gets exposed via PluginCapability. It delegates to the sub-services (Warden, Weapons, etc.).

**Step 3: Wire up PluginCapability registration in Load()**

```csharp
Capabilities.RegisterPluginCapability(JailbreakApi.Core, () => coreService);
```

**Step 4: Verify build**

```bash
dotnet build src/Jailbreak.Core/Jailbreak.Core.csproj
```

**Step 5: Commit**

```bash
git add src/Jailbreak.Core/
git commit -m "feat(core): create CorePlugin entry point and IJailbreakCore implementation"
```

### Task 7: Migrate state tracking services

**Files:**
- Create: `src/Jailbreak.Core/Services/State/AliveStateTracker.cs`
- Create: `src/Jailbreak.Core/Services/State/GlobalStateTracker.cs`
- Create: `src/Jailbreak.Core/Services/State/RoundStateTracker.cs`

**Step 1: Migrate from `src/Jailbreak.Generic/PlayerState/`**

Copy the three state tracker files. Update namespaces from `Jailbreak.Generic.*` to `Jailbreak.Core.Services.State`.

Remove the `IPluginBehavior` interface — these are now internal services registered in Core's DI, using `[GameEventHandler]` attributes directly.

**Step 2: Migrate CoroutineManager**

Copy from `src/Jailbreak.Generic/Coroutines/CoroutineManager.cs` to `src/Jailbreak.Core/Services/State/CoroutineManager.cs`. Update namespace.

**Step 3: Register in CorePlugin's service collection**

Add DI registrations for state trackers using `IPluginServiceCollection<CorePlugin>`.

**Step 4: Verify build**

```bash
dotnet build src/Jailbreak.Core/Jailbreak.Core.csproj
```

**Step 5: Commit**

```bash
git add src/Jailbreak.Core/Services/State/
git commit -m "feat(core): migrate state tracking services"
```

### Task 8: Migrate Warden system

**Files:**
- Create: `src/Jailbreak.Core/Services/Warden/WardenService.cs`
- Create: `src/Jailbreak.Core/Services/Warden/WardenSelection.cs`
- Create: `src/Jailbreak.Core/Services/Warden/Markers/*.cs`
- Create: `src/Jailbreak.Core/Services/Warden/Paint/*.cs`
- Create: `src/Jailbreak.Core/Services/Warden/SpecialTreatment/*.cs`
- Create: `src/Jailbreak.Core/Commands/WardenCommands.cs`

**Step 1: Migrate core warden services**

From `mod/Jailbreak.Warden/`:
- `Global/WardenBehavior.cs` → `Services/Warden/WardenService.cs`
- `Selection/WardenSelectionBehavior.cs` → `Services/Warden/WardenSelection.cs`

Update namespaces. Replace `IPluginBehavior` with direct service registration. Move `[GameEventHandler]` methods into appropriate classes.

**Step 2: Migrate warden sub-features**

- `Markers/*.cs` → `Services/Warden/Markers/*.cs`
- `Paint/*.cs` → `Services/Warden/Paint/*.cs`
- `SpecialTreatment/*.cs` → `Services/Warden/SpecialTreatment/*.cs`

**Step 3: Consolidate warden commands**

From `mod/Jailbreak.Warden/Commands/`:
- `CountCommandsBehavior.cs`
- `WardenCommandsBehavior.cs`
- `ChickenCommandBehavior.cs`
- etc.

Consolidate into `Commands/WardenCommands.cs` (or a few files if it's too large). All `[ConsoleCommand]` methods go in Commands/.

**Step 4: Co-locate warden-internal interfaces**

Any interfaces from `public/Jailbreak.Public/Mod/Warden/` that are NOT in Contracts (i.e., only used within Core) move to `Services/Warden/`.

**Step 5: Verify build**

```bash
dotnet build src/Jailbreak.Core/Jailbreak.Core.csproj
```

**Step 6: Commit**

```bash
git add src/Jailbreak.Core/Services/Warden/ src/Jailbreak.Core/Commands/WardenCommands.cs
git commit -m "feat(core): migrate warden system"
```

### Task 9: Migrate Rebel, Mute, and Logs

**Files:**
- Create: `src/Jailbreak.Core/Services/Rebel/RebelService.cs`
- Create: `src/Jailbreak.Core/Services/Mute/MuteService.cs`
- Create: `src/Jailbreak.Core/Services/Logs/LogService.cs`
- Create: `src/Jailbreak.Core/Commands/MuteCommands.cs`

**Step 1: Migrate Rebel**

From `mod/Jailbreak.Rebel/` (4 files). Update namespaces, remove `IPluginBehavior`.

**Step 2: Migrate Mute**

From `mod/Jailbreak.Mute/` (2 files). Update namespaces, remove `IPluginBehavior`.

**Step 3: Migrate Logs**

From `mod/Jailbreak.Logs/` (7 files). Update namespaces, remove `IPluginBehavior`.

**Step 4: Verify build**

```bash
dotnet build src/Jailbreak.Core/Jailbreak.Core.csproj
```

**Step 5: Commit**

```bash
git add src/Jailbreak.Core/Services/Rebel/ src/Jailbreak.Core/Services/Mute/ src/Jailbreak.Core/Services/Logs/ src/Jailbreak.Core/Commands/
git commit -m "feat(core): migrate rebel, mute, and logs services"
```

### Task 10: Create Core locale

**Files:**
- Create: `src/Jailbreak.Core/Locale/ICoreLocale.cs`
- Create: `src/Jailbreak.Core/Locale/CoreLocale.cs`

**Step 1: Consolidate locale interfaces**

Merge all Core-related locale interfaces into one `ICoreLocale`:
- From `IWardenLocale` (warden messages)
- From `IWardenCmdCountLocale`, `IWardenCmdOpenLocale`, etc. (warden command messages)
- From `IWardenSTLocale` (special treatment messages)
- From `IRebelLocale` (rebel messages)
- From `IWardenPeaceLocale` (mute messages)
- From `ILogLocale` (log messages)
- From `IGenericCmdLocale` (shared command messages)
- From `ILGLocale` (last guard messages)

**Step 2: Create CoreLocale.cs implementation**

Consolidate from `lang/Jailbreak.English/Warden/*.cs`, `lang/Jailbreak.English/Rebel/*.cs`, `lang/Jailbreak.English/Mute/*.cs`, `lang/Jailbreak.English/Logs/*.cs`, `lang/Jailbreak.English/Generic/*.cs`, `lang/Jailbreak.English/LastGuard/*.cs`.

**Step 3: Register locale in Core's service collection**

```csharp
services.AddSingleton<ICoreLocale, CoreLocale>();
```

**Step 4: Update all Core services to inject `ICoreLocale` instead of individual locale interfaces**

**Step 5: Verify build**

```bash
dotnet build src/Jailbreak.Core/Jailbreak.Core.csproj
```

**Step 6: Commit**

```bash
git add src/Jailbreak.Core/Locale/
git commit -m "feat(core): create consolidated Core locale"
```

---

## Phase 3: Jailbreak.LastRequest

### Task 11: Create LastRequestPlugin and migrate LR system

**Files:**
- Create: `src/Jailbreak.LastRequest/LastRequestPlugin.cs`
- Create: `src/Jailbreak.LastRequest/Services/ILastRequestManager.cs`
- Create: `src/Jailbreak.LastRequest/Services/LastRequestManager.cs`
- Create: `src/Jailbreak.LastRequest/Services/LastRequestFactory.cs`
- Create: `src/Jailbreak.LastRequest/Services/Types/*.cs`
- Create: `src/Jailbreak.LastRequest/Commands/LastRequestCommands.cs`
- Create: `src/Jailbreak.LastRequest/Events/LastRequestEvents.cs`
- Create: `src/Jailbreak.LastRequest/Locale/ILastRequestLocale.cs`
- Create: `src/Jailbreak.LastRequest/Locale/LastRequestLocale.cs`

**Step 1: Create LastRequestPlugin.cs**

Standalone `BasePlugin`. On Load, get `IJailbreakCore` via `JailbreakApi.Core.Get()`.

**Step 2: Migrate LR manager and factory**

From `mod/Jailbreak.LastRequest/`:
- `LastRequestManager.cs` → `Services/LastRequestManager.cs`
- `LastRequestFactory.cs` → `Services/LastRequestFactory.cs`

Replace the enum + switch factory with DI-based registration: register each `ILastRequest` implementation, inject `IEnumerable<ILastRequest>` into the factory.

**Step 3: Migrate LR types**

From `mod/Jailbreak.LastRequest/LastRequests/`:
- `RockPaperScissors.cs`, `Race.cs`, `Coinflip.cs`, `BulletForBullet.cs`, `GunToss.cs`, `KnifeFight.cs`, `NoScope.cs`

Each implements `ILastRequest` from Contracts. Move to `Services/Types/`.

**Step 4: Migrate commands and events**

Consolidate command behaviors into `Commands/LastRequestCommands.cs`.

**Step 5: Create locale**

Consolidate from `lang/Jailbreak.English/LastRequest/*.cs` (7 files) into one `ILastRequestLocale` + `LastRequestLocale`.

**Step 6: Verify build**

```bash
dotnet build src/Jailbreak.LastRequest/Jailbreak.LastRequest.csproj
```

**Step 7: Commit**

```bash
git add src/Jailbreak.LastRequest/
git commit -m "feat(lr): create LastRequest plugin with all LR types"
```

---

## Phase 4: Jailbreak.Fun

### Task 12: Create FunPlugin and migrate SpecialDay

**Files:**
- Create: `src/Jailbreak.Fun/FunPlugin.cs`
- Create: `src/Jailbreak.Fun/Services/SpecialDay/*.cs`
- Create: `src/Jailbreak.Fun/Commands/SpecialDayCommands.cs`

**Step 1: Create FunPlugin.cs**

Standalone `BasePlugin`. On Load, get `IJailbreakCore` via `JailbreakApi.Core.Get()`.

**Step 2: Migrate SpecialDay manager and factory**

From `mod/Jailbreak.SpecialDay/`. Replace enum + switch with DI-based `IEnumerable<ISpecialDay>`.

**Step 3: Migrate all special day implementations**

From `mod/Jailbreak.SpecialDay/SpecialDays/`. Each implements `ISpecialDay` from Contracts.

**Step 4: Verify build**

```bash
dotnet build src/Jailbreak.Fun/Jailbreak.Fun.csproj
```

**Step 5: Commit**

```bash
git add src/Jailbreak.Fun/Services/SpecialDay/ src/Jailbreak.Fun/FunPlugin.cs src/Jailbreak.Fun/Commands/
git commit -m "feat(fun): migrate SpecialDay system"
```

### Task 13: Migrate RTD, Rainbow, and Trail

**Files:**
- Create: `src/Jailbreak.Fun/Services/RTD/*.cs`
- Create: `src/Jailbreak.Fun/Services/Rainbow/RainbowService.cs`
- Create: `src/Jailbreak.Fun/Services/Trail/TrailService.cs`
- Create: `src/Jailbreak.Fun/Commands/RTDCommands.cs`

**Step 1: Migrate RTD**

From `mod/Jailbreak.RTD/` (25 files). Migrate reward generator and all rewards. Replace manual list with DI-based `IEnumerable<IRTDReward>`. Use `[RewardWeight]` attribute or property on the interface for probability.

**Step 2: Migrate Rainbow**

From `mod/Jailbreak.Rainbow/` (2 files).

**Step 3: Migrate Trail**

From `mod/Jailbreak.Trail/` (8 files). Note: this module was NOT registered in the old service collection. Include it now if it should be active, or skip if deprecated.

**Step 4: Create Fun locale**

Consolidate from `lang/Jailbreak.English/SpecialDay/*.cs` and `lang/Jailbreak.English/RTD/*.cs` into one `IFunLocale` + `FunLocale`.

**Step 5: Verify build**

```bash
dotnet build src/Jailbreak.Fun/Jailbreak.Fun.csproj
```

**Step 6: Commit**

```bash
git add src/Jailbreak.Fun/
git commit -m "feat(fun): migrate RTD, Rainbow, and Trail"
```

---

## Phase 5: Jailbreak.Zones

### Task 14: Create ZonesPlugin and migrate Zones + Draw

**Files:**
- Create: `src/Jailbreak.Zones/ZonesPlugin.cs`
- Create: `src/Jailbreak.Zones/Services/Zones/*.cs`
- Create: `src/Jailbreak.Zones/Services/Draw/*.cs`
- Create: `src/Jailbreak.Zones/Services/Draw/Shapes/*.cs`
- Create: `src/Jailbreak.Zones/Commands/ZoneCommands.cs`
- Create: `src/Jailbreak.Zones/Locale/IZonesLocale.cs`
- Create: `src/Jailbreak.Zones/Locale/ZonesLocale.cs`

**Step 1: Create ZonesPlugin.cs**

Standalone `BasePlugin`. Consumes `IJailbreakCore`.

**Step 2: Migrate Zones**

From `mod/Jailbreak.Zones/` (7 files). Remove the dependency on `Jailbreak.Debug` (was a circular-like dependency in old code). Zone creation commands move to Commands/.

Note: Zones uses `MySqlConnector` — add this NuGet reference to the .csproj.

**Step 3: Migrate Draw/BeamShape system**

From `mod/Jailbreak.Draw/` (14 files):
- `BeamShapeRegistry.cs` → `Services/Draw/BeamShapeRegistry.cs`
- `BeamShapeFactory.cs` → `Services/Draw/BeamShapeFactory.cs`
- `Shapes/*.cs` → `Services/Draw/Shapes/*.cs`

Replace manual registry constructor with DI-based `IEnumerable<IBeamShape>`.

**Step 4: Verify build**

```bash
dotnet build src/Jailbreak.Zones/Jailbreak.Zones.csproj
```

**Step 5: Commit**

```bash
git add src/Jailbreak.Zones/
git commit -m "feat(zones): create Zones plugin with Draw/BeamShape system"
```

---

## Phase 6: Jailbreak.Gangs

### Task 15: Create GangsPlugin and migrate gang system

**Files:**
- Create: `src/Jailbreak.Gangs/GangsPlugin.cs`
- Create: `src/Jailbreak.Gangs/Services/IGangService.cs`
- Create: `src/Jailbreak.Gangs/Services/GangService.cs`
- Create: `src/Jailbreak.Gangs/Services/Perks/*.cs`
- Create: `src/Jailbreak.Gangs/Commands/GangCommands.cs`
- Create: `src/Jailbreak.Gangs/Locale/IGangsLocale.cs`
- Create: `src/Jailbreak.Gangs/Locale/GangsLocale.cs`

**Step 1: Create GangsPlugin.cs**

Standalone `BasePlugin`. Consumes `IJailbreakCore` and the external `GangsAPI.dll` capability.

**Step 2: Migrate core gang service**

From `mod/Gangs.BaseImpl/` (8 files). Consolidate `BasePerk`, `BasicPerkMenu`, `AbstractEnumCommand`, `AbstractEnumMenu`, stats, and color extensions into `Services/`.

**Step 3: Collapse 7 perk projects into individual files**

Each of these was its own 5-file project. Collapse to one class per perk:
- `mod/Gangs.BombIconPerk/` → `Services/Perks/BombIconPerk.cs`
- `mod/Gangs.WardenIconPerk/` → `Services/Perks/WardenIconPerk.cs`
- `mod/Gangs.SpecialIconPerk/` → `Services/Perks/SpecialIconPerk.cs`
- `mod/Gangs.WardenPaintColorPerk/` → `Services/Perks/WardenPaintColorPerk.cs`
- `mod/Gangs.SpecialDayColorPerk/` → `Services/Perks/SpecialDayColorPerk.cs`
- `mod/Gangs.LastRequestColorPerk/` → `Services/Perks/LastRequestColorPerk.cs`
- `mod/Gangs.CellsPerk/` → `Services/Perks/CellsPerk.cs`

Each perk implements `IGangPerk` from Contracts. Eliminate the separate Bootstrap classes — perks are registered via DI.

**Step 4: Migrate commands**

Consolidate `AbstractEnumCommand` subcommands into `Commands/GangCommands.cs`.

**Step 5: Verify build**

```bash
dotnet build src/Jailbreak.Gangs/Jailbreak.Gangs.csproj
```

**Step 6: Commit**

```bash
git add src/Jailbreak.Gangs/
git commit -m "feat(gangs): create Gangs plugin with all perks consolidated"
```

---

## Phase 7: Jailbreak.Tools

### Task 16: Create ToolsPlugin and migrate debug commands

**Files:**
- Create: `src/Jailbreak.Tools/ToolsPlugin.cs`
- Create: `src/Jailbreak.Tools/Commands/*.cs`

**Step 1: Create ToolsPlugin.cs**

Standalone `BasePlugin`. Consumes `IJailbreakCore`.

**Step 2: Migrate debug commands**

From `mod/Jailbreak.Debug/` (18 files). The current system has a `DebugCommand` with a dictionary of subcommands. Modernize to individual `[ConsoleCommand]` classes:

- `Subcommands/EndRound.cs` → `Commands/EndRoundCommand.cs`
- `Subcommands/DebugSetTime.cs` → `Commands/SetTimeCommand.cs`
- `Subcommands/DebugZone.cs` → `Commands/ZoneCreatorCommand.cs`
- etc.

Or keep the subcommand pattern if preferred — it's a clean pattern for admin tools.

**Step 3: Verify build**

```bash
dotnet build src/Jailbreak.Tools/Jailbreak.Tools.csproj
```

**Step 4: Commit**

```bash
git add src/Jailbreak.Tools/
git commit -m "feat(tools): create Tools plugin with debug/operator commands"
```

---

## Phase 8: Cleanup and CI

### Task 17: Update CI/CD pipeline

**Files:**
- Modify: `.github/workflows/nightly.yml`
- Modify: `.github/workflows/release.yml`

**Step 1: Update build commands**

The old CI builds one project: `src/Jailbreak/Jailbreak.csproj`. The new CI must build and publish all 7 plugins.

```yaml
- run: |
    dotnet restore
    dotnet build --no-restore
    dotnet publish src/Jailbreak.Core/Jailbreak.Core.csproj --no-build --no-restore
    dotnet publish src/Jailbreak.LastRequest/Jailbreak.LastRequest.csproj --no-build --no-restore
    dotnet publish src/Jailbreak.Fun/Jailbreak.Fun.csproj --no-build --no-restore
    dotnet publish src/Jailbreak.Zones/Jailbreak.Zones.csproj --no-build --no-restore
    dotnet publish src/Jailbreak.Gangs/Jailbreak.Gangs.csproj --no-build --no-restore
    dotnet publish src/Jailbreak.Tools/Jailbreak.Tools.csproj --no-build --no-restore
```

Update publish directories and artifact paths for each plugin.

**Step 2: Commit**

```bash
git add .github/workflows/
git commit -m "ci: update build pipeline for multi-plugin architecture"
```

### Task 18: Remove old directory structure

**Files:**
- Delete: `mod/` (entire directory)
- Delete: `public/` (entire directory)
- Delete: `lang/` (entire directory)
- Delete: `src/Jailbreak/` (old monolithic plugin)
- Delete: `src/Jailbreak.Generic/` (absorbed into Core)

**Step 1: Verify all functionality has been migrated**

Do a file-by-file audit:
- Every `.cs` file in `mod/`, `public/`, `lang/`, `src/Jailbreak/`, and `src/Jailbreak.Generic/` should have a corresponding file in the new `src/` structure.

**Step 2: Remove old directories**

```bash
git rm -r mod/ public/ lang/ src/Jailbreak/ src/Jailbreak.Generic/
```

**Step 3: Final build verification**

```bash
dotnet build
```

All 8 projects should build with 0 errors.

**Step 4: Commit**

```bash
git add -A
git commit -m "cleanup: remove old monolithic directory structure"
```

### Task 19: Final review and documentation

**Step 1: Update README.md**

Document the new plugin structure, how to build, how to add a new feature (e.g., adding a new LR type or Special Day).

**Step 2: Verify .gitignore works correctly**

```bash
git status
```

No build artifacts should show as untracked.

**Step 3: Commit**

```bash
git add README.md
git commit -m "docs: update README for multi-plugin architecture"
```

---

## Task Dependency Graph

```
Task 1 (branch + gitignore)
  └→ Task 2 (solution scaffolding)
       └→ Task 3 (formatting core)
            └→ Task 4 (IJailbreakCore + contracts)
                 └→ Task 5 (extensible type interfaces)
                      ├→ Task 6 (CorePlugin entry)
                      │    └→ Task 7 (state tracking)
                      │         └→ Task 8 (warden)
                      │              └→ Task 9 (rebel, mute, logs)
                      │                   └→ Task 10 (core locale)
                      ├→ Task 11 (LastRequest) — can start after Task 5
                      ├→ Task 12 (SpecialDay) — can start after Task 5
                      │    └→ Task 13 (RTD, Rainbow, Trail)
                      ├→ Task 14 (Zones + Draw) — can start after Task 5
                      ├→ Task 15 (Gangs) — can start after Task 5
                      └→ Task 16 (Tools) — can start after Task 5
                           └→ Task 17 (CI/CD)
                                └→ Task 18 (remove old code)
                                     └→ Task 19 (docs)
```

**Parallelizable:** Tasks 11-16 (satellite plugins) can all be done in parallel once Contracts (Tasks 3-5) and Core (Tasks 6-10) are complete. Task 11 specifically depends on Core being done since LR interacts heavily with Warden state.

---

## Estimated Scope

| Phase | Tasks | Estimated Files |
|-------|-------|-----------------|
| Phase 0: Setup | 2 | ~10 (scaffolding) |
| Phase 1: Contracts | 3 | ~25 |
| Phase 2: Core | 5 | ~60 |
| Phase 3: LastRequest | 1 | ~15 |
| Phase 4: Fun | 2 | ~55 |
| Phase 5: Zones | 1 | ~25 |
| Phase 6: Gangs | 1 | ~15 |
| Phase 7: Tools | 1 | ~20 |
| Phase 8: Cleanup | 3 | ~5 (mostly deletions) |
| **Total** | **19** | **~230 new files** |
