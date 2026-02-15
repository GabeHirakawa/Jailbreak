# Jailbreak Plugin Rewrite Design

## Problem

The current codebase has 365 .cs files across 30 projects in 4 top-level directories (`src/`, `mod/`, `public/`, `lang/`). Editing a single feature (e.g., Warden) requires navigating 5+ locations:

1. `mod/Jailbreak.Warden/` — implementation
2. `public/Jailbreak.Public/Mod/Warden/` — interfaces
3. `public/Jailbreak.Formatting/Views/Warden/` — message formatting
4. `lang/Jailbreak.English/Warden/` — locale strings
5. `src/Jailbreak/JailbreakServiceCollection.cs` — DI registration

Every registration pattern (LR types, special days, RTD rewards, gang perks, beam shapes) is fully manual — enum + switch factory or manual list construction.

## Goals

- Restructure from layer-based to feature-based organization
- Break the monolith into separate CSSharp plugins (SourceMod-style naming)
- Modernize CSSharp API usage (consistent attribute-based registration, simplify DI)
- Co-locate interfaces with implementations
- Simplify locale system while preserving multi-format rendering
- Consistent internal structure across all plugins
- Feature parity with current code

## Architecture: Multi-Plugin with Shared Contracts

### Plugin Split

| Plugin | Contents | Depends On |
|--------|----------|------------|
| `Jailbreak.Contracts` | Shared interfaces, formatting core, `IJailbreakCore` | — |
| `Jailbreak.Core` | Warden, Rebel, Mute, Logs, state tracking | Contracts |
| `Jailbreak.LastRequest` | LR manager, all LR types | Contracts (consumes `IJailbreakCore` via PluginCapability) |
| `Jailbreak.Fun` | RTD, SpecialDay, Rainbow, Trail | Contracts (consumes `IJailbreakCore` via PluginCapability) |
| `Jailbreak.Zones` | Zones, Draw/markers | Contracts (consumes `IJailbreakCore` via PluginCapability) |
| `Jailbreak.Gangs` | Gang system + all perks | Contracts (consumes `IJailbreakCore` via PluginCapability) |
| `Jailbreak.Tools` | Debug/operator commands | Contracts (consumes `IJailbreakCore` via PluginCapability) |

### Dependency Direction

Dependencies are always one-way. Satellite plugins depend on Contracts and consume Core's API. Core never imports satellite plugins.

```
Jailbreak.Contracts  <── Jailbreak.Core (implements IJailbreakCore)
        ^
        ├── Jailbreak.LastRequest (consumes IJailbreakCore)
        ├── Jailbreak.Fun (consumes IJailbreakCore)
        ├── Jailbreak.Zones (consumes IJailbreakCore)
        ├── Jailbreak.Gangs (consumes IJailbreakCore)
        └── Jailbreak.Tools (consumes IJailbreakCore)
```

### Inter-Plugin Communication

Core exposes `IJailbreakCore` via CSSharp's `PluginCapability<IJailbreakCore>`. Satellite plugins retrieve it on load and use it to interact with core game state.

```csharp
// Core plugin registers
Capabilities.RegisterPluginCapability(JailbreakApi.Core, () => coreApi);

// Satellite plugin consumes
var core = JailbreakApi.Core.Get();
core.SetRoundState(RoundState.SpecialDay);
core.Announce("Gun Game has started!");
```

## IJailbreakCore API Surface

```csharp
public interface IJailbreakCore {
    // State
    RoundState RoundState { get; }
    void SetRoundState(RoundState state);

    // Players
    IEnumerable<CCSPlayerController> GetAlivePlayers();
    IEnumerable<CCSPlayerController> GetAlivePlayers(CsTeam team);
    bool IsRebel(CCSPlayerController player);
    void MarkRebel(CCSPlayerController player);

    // Sub-APIs
    IWardenService Warden { get; }
    IWeaponService Weapons { get; }

    // Communication
    void Announce(string message);
    void AnnounceCenter(string message);
    void Message(CCSPlayerController player, string message);
}
```

Sub-APIs accessed as properties: `jailbreak.Warden.IsWarden(player)`, `jailbreak.Weapons.Strip(player)`.

## Consistent Plugin Structure

Every plugin follows the same internal layout:

```
Jailbreak.<PluginName>/
├── <Name>Plugin.cs             # BasePlugin entry point
├── Services/                   # Core service implementations
│   ├── I<Thing>Service.cs      # Interface (co-located)
│   └── <Thing>Service.cs       # Implementation
├── Commands/                   # Console commands ([ConsoleCommand])
│   └── <Thing>Commands.cs
├── Events/                     # Game event handlers ([GameEventHandler])
│   └── <Thing>Events.cs
├── Locale/                     # Co-located i18n
│   ├── I<PluginName>Locale.cs
│   └── <PluginName>Locale.cs
├── Models/                     # Data types, enums, DTOs
│   └── <Thing>.cs
└── Jailbreak.<PluginName>.csproj
```

Rules:
1. Entry point is always `<Name>Plugin.cs` extending `BasePlugin`
2. `Services/` for business logic — interfaces + implementations together
3. `Commands/` for anything with `[ConsoleCommand]`
4. `Events/` for `[GameEventHandler]` classes not tied to a specific service
5. `Locale/` for i18n strings
6. `Models/` for data structures, enums, config types
7. Large sub-features get a subfolder under `Services/` (e.g., `Services/Warden/Markers/`)

## Full Directory Layout

```
Jailbreak/
├── src/
│   ├── Jailbreak.Contracts/
│   │   ├── IJailbreakCore.cs
│   │   ├── JailbreakApi.cs              # PluginCapability definitions
│   │   ├── Services/
│   │   │   ├── IWardenService.cs        # Cross-plugin contracts only
│   │   │   ├── IWeaponService.cs
│   │   │   └── ...
│   │   ├── Formatting/
│   │   │   ├── IView.cs
│   │   │   ├── FormatObject.cs
│   │   │   ├── FormatWriter.cs
│   │   │   ├── SimpleView.cs
│   │   │   ├── ViewExtensions.cs
│   │   │   └── Objects/
│   │   │       ├── PlayerFormatObject.cs
│   │   │       ├── StringFormatObject.cs
│   │   │       ├── TeamFormatObject.cs
│   │   │       └── HiddenFormatObject.cs
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── Models/
│   │   │   ├── RoundState.cs
│   │   │   └── ...
│   │   └── Jailbreak.Contracts.csproj
│   │
│   ├── Jailbreak.Core/
│   │   ├── CorePlugin.cs
│   │   ├── Services/
│   │   │   ├── Warden/
│   │   │   │   ├── WardenService.cs
│   │   │   │   ├── WardenSelection.cs
│   │   │   │   ├── Markers/
│   │   │   │   ├── Paint/
│   │   │   │   └── SpecialTreatment/
│   │   │   ├── Rebel/
│   │   │   │   └── RebelService.cs
│   │   │   ├── Mute/
│   │   │   │   └── MuteService.cs
│   │   │   ├── Logs/
│   │   │   │   └── LogService.cs
│   │   │   └── State/
│   │   │       ├── AliveStateTracker.cs
│   │   │       ├── GlobalStateTracker.cs
│   │   │       └── RoundStateTracker.cs
│   │   ├── Commands/
│   │   │   ├── WardenCommands.cs
│   │   │   └── MuteCommands.cs
│   │   ├── Events/
│   │   │   └── RoundEvents.cs
│   │   ├── Locale/
│   │   │   ├── ICoreLocale.cs
│   │   │   └── CoreLocale.cs
│   │   ├── Models/
│   │   └── Jailbreak.Core.csproj
│   │
│   ├── Jailbreak.LastRequest/
│   │   ├── LastRequestPlugin.cs
│   │   ├── Services/
│   │   │   ├── ILastRequestManager.cs
│   │   │   ├── LastRequestManager.cs
│   │   │   ├── ILastRequestFactory.cs
│   │   │   ├── LastRequestFactory.cs
│   │   │   └── Types/
│   │   │       ├── RockPaperScissors.cs
│   │   │       ├── Race.cs
│   │   │       ├── Coinflip.cs
│   │   │       ├── BulletForBullet.cs
│   │   │       ├── GunToss.cs
│   │   │       ├── KnifeFight.cs
│   │   │       └── NoScope.cs
│   │   ├── Commands/
│   │   │   └── LastRequestCommands.cs
│   │   ├── Events/
│   │   │   └── LastRequestEvents.cs
│   │   ├── Locale/
│   │   │   ├── ILastRequestLocale.cs
│   │   │   └── LastRequestLocale.cs
│   │   ├── Models/
│   │   └── Jailbreak.LastRequest.csproj
│   │
│   ├── Jailbreak.Fun/
│   │   ├── FunPlugin.cs
│   │   ├── Services/
│   │   │   ├── RTD/
│   │   │   │   ├── IRTDService.cs
│   │   │   │   ├── RTDService.cs
│   │   │   │   └── Rewards/
│   │   │   ├── SpecialDay/
│   │   │   │   ├── ISpecialDayManager.cs
│   │   │   │   ├── SpecialDayManager.cs
│   │   │   │   └── Days/
│   │   │   ├── Rainbow/
│   │   │   │   └── RainbowService.cs
│   │   │   └── Trail/
│   │   │       └── TrailService.cs
│   │   ├── Commands/
│   │   │   ├── RTDCommands.cs
│   │   │   └── SpecialDayCommands.cs
│   │   ├── Events/
│   │   │   └── SpecialDayEvents.cs
│   │   ├── Locale/
│   │   │   ├── IFunLocale.cs
│   │   │   └── FunLocale.cs
│   │   ├── Models/
│   │   └── Jailbreak.Fun.csproj
│   │
│   ├── Jailbreak.Zones/
│   │   ├── ZonesPlugin.cs
│   │   ├── Services/
│   │   │   ├── Zones/
│   │   │   │   ├── IZoneService.cs
│   │   │   │   └── ZoneService.cs
│   │   │   └── Draw/
│   │   │       ├── IDrawService.cs
│   │   │       ├── DrawService.cs
│   │   │       └── Shapes/
│   │   ├── Commands/
│   │   │   └── ZoneCommands.cs
│   │   ├── Events/
│   │   ├── Locale/
│   │   │   ├── IZonesLocale.cs
│   │   │   └── ZonesLocale.cs
│   │   ├── Models/
│   │   └── Jailbreak.Zones.csproj
│   │
│   ├── Jailbreak.Gangs/
│   │   ├── GangsPlugin.cs
│   │   ├── Services/
│   │   │   ├── IGangService.cs
│   │   │   ├── GangService.cs
│   │   │   └── Perks/
│   │   │       ├── WardenIconPerk.cs
│   │   │       ├── BombIconPerk.cs
│   │   │       ├── SpecialDayColorPerk.cs
│   │   │       ├── LastRequestColorPerk.cs
│   │   │       ├── WardenPaintColorPerk.cs
│   │   │       ├── SpecialIconPerk.cs
│   │   │       └── CellsPerk.cs
│   │   ├── Commands/
│   │   │   └── GangCommands.cs
│   │   ├── Events/
│   │   ├── Locale/
│   │   │   ├── IGangsLocale.cs
│   │   │   └── GangsLocale.cs
│   │   ├── Models/
│   │   └── Jailbreak.Gangs.csproj
│   │
│   └── Jailbreak.Tools/
│       ├── ToolsPlugin.cs
│       ├── Services/
│       ├── Commands/
│       │   ├── DebugCommands.cs
│       │   ├── EndRoundCommand.cs
│       │   ├── SetTimeCommand.cs
│       │   └── ZoneCreatorCommand.cs
│       ├── Events/
│       ├── Locale/
│       │   ├── IToolsLocale.cs
│       │   └── ToolsLocale.cs
│       ├── Models/
│       └── Jailbreak.Tools.csproj
│
├── Jailbreak.sln
└── README.md
```

## Naming Conventions

### Type Names

| Layer | Pattern | Example |
|-------|---------|---------|
| Contract interface | `I<DomainThing>` | `ISpecialDay`, `IGangPerk`, `IRTDReward` |
| Core API interface | `IJailbreakCore` | — |
| Service interface | `I<Feature>Service` | `IWardenService`, `IZoneService` |
| Implementation | `<Thing>` (no `Impl` suffix) | `WardenService`, `GunGameDay` |
| Plugin entry | `<Name>Plugin` | `CorePlugin`, `FunPlugin` |

### Domain Prefix Rule

If the base term is generic enough that reading it in isolation wouldn't tell you the system, prefix with the domain:

| Interface | Why |
|-----------|-----|
| `ILastRequest` | "Last request" is already jailbreak-specific |
| `ISpecialDay` | "Special day" is already jailbreak-specific |
| `IRTDReward` | "Reward" alone is vague — prefix with RTD |
| `IGangPerk` | "Perk" alone is generic — prefix with Gang |
| `IBeamShape` | "Shape" alone is generic — prefix with Beam |

### File & Folder Rules

1. Files match class names: `WardenService.cs` contains `WardenService`
2. Folders match domains: `Services/Warden/`, not `Services/WardenStuff/`
3. Interfaces co-locate with implementations (same folder)
4. One class per file

## Extensible Type Registration

Standard DI registration. To add a new type:

1. Create a class implementing the interface (e.g., `GunGameDay : ISpecialDay`)
2. Register it in the plugin's service collection
3. Done

```csharp
// In FunPlugin.cs or its service collection
services.AddSingleton<ISpecialDay, GunGameDay>();
services.AddSingleton<ISpecialDay, WardayDay>();
services.AddSingleton<ISpecialDay, InfectionDay>();
// ...

// Manager discovers all registered types via DI
public class SpecialDayManager(IEnumerable<ISpecialDay> days) {
    // days contains all registered ISpecialDay implementations
}
```

Applied to all extensible systems:

| Plugin | Interface | Implementations |
|--------|-----------|-----------------|
| `Jailbreak.LastRequest` | `ILastRequest` | RPS, Race, Coinflip, B4B, GunToss, KnifeFight, NoScope |
| `Jailbreak.Fun` | `ISpecialDay` | GunGame, Warday, HNS, Infection, FFA, etc. |
| `Jailbreak.Fun` | `IRTDReward` | HealthShot, WeaponReward, CreditReward, etc. |
| `Jailbreak.Gangs` | `IGangPerk` | BombIcon, WardenIcon, SpecialDayColor, etc. |
| `Jailbreak.Zones` | `IBeamShape` | Circle, Square, Diamond, Heart, etc. |

## Locale System

### What stays

- `IView` / `FormatObject` / `SimpleView` pattern — multi-format rendering (Chat, Panorama, Plain)
- Fluent API: `locale.NewWarden(player).ToAllChat().ToAllCenter()`
- Type-safe locale interfaces with compile-time checking

### What changes

- Formatting core moves into `Jailbreak.Contracts` (every plugin needs it)
- Locale implementations co-locate into each plugin's `Locale/` folder
- One locale interface + one implementation per plugin (not per sub-feature)
- `lang/` directory eliminated
- `Jailbreak.Formatting` project eliminated (absorbed into Contracts)

### Before vs. After

- **Before:** 76 files across 2 projects, 19 interfaces, 31 implementations
- **After:** ~10 formatting core files in Contracts, 2 locale files per plugin (~22 total)

### Multi-language support

Preserved. To add French: create `CoreLocaleFr.cs` implementing `ICoreLocale`, swap DI registration.

## CSSharp API Modernization

- Prefer `[GameEventHandler]` and `[ConsoleCommand]` attributes over manual `RegisterListener`/`RegisterEventHandler` where possible
- Use `IPluginServiceCollection<T>` for DI in each plugin
- Use `PluginCapability<T>` for all inter-plugin communication
- Update `CounterStrikeSharp.API` to latest stable version
- Remove custom `IPluginBehavior` pattern where CSSharp's built-in lifecycle is sufficient

## Migration Summary

| Current | New |
|---------|-----|
| 30 projects | 8 projects |
| 4 top-level directories | 1 `src/` directory |
| ~365 .cs files | Estimated ~200 .cs files (same functionality, less boilerplate) |
| Layer-based organization | Feature-based organization |
| Single monolithic plugin | 7 independent CSSharp plugins |
| Manual enum + switch factories | Standard DI registration |
| Separate interface/impl projects | Co-located interfaces |
| Separate locale project | Co-located locale per plugin |
