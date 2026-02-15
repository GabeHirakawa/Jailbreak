# ![EdgeGamers](assets/ego_text.webp) Jailbreak

[![Discord](https://img.shields.io/discord/161245089774043136?style=for-the-badge&logo=discord&logoColor=%23ffffff&label=Discord&color=%235865F2
)](https://edgm.rs/discord)

The classic Jail gamemode, ported to Counter-Strike 2.

## Downloads

[![Release](https://img.shields.io/badge/Release-mediumseagreen?style=for-the-badge&logo=onlyoffice
)](https://github.com/edgegamers/Jailbreak/releases/)⠀⠀
[![Stable](https://img.shields.io/badge/Stable-orangered?style=for-the-badge&logo=onlyoffice)](https://nightly.link/edgegamers/Jailbreak/workflows/nightly/main/jailbreak-nightly)
⠀⠀
[![Dev](https://img.shields.io/badge/Nightly-slateblue?style=for-the-badge&logo=onlyoffice
)](https://nightly.link/edgegamers/Jailbreak/workflows/nightly/dev/jailbreak-nightly)

**Release** builds are our full releases. We try to keep these high-quality and bug-free, when we can.
Our **Stable** builds run on EdgeGamers' own Jailbreak servers.
Our **Nightly** builds are used exclusively for development and staging, and are likely to have problems.

## Versioning

Our release tags starting from 'v2.0.0' follow the [Semantic Versioning 2.0.0](https://semver.org/) standard,
where `MAJOR.MINOR.PATCH` are incremented based on the following:

- `MAJOR` when we make incompatible API changes,
- `MINOR` when we add functionality in a backwards-compatible manner.
- `PATCH` when we make backwards-compatible bug fixes.

## Status

- **⚙️ Server**
  - [x] Stats/Analytics Sinks
  - [x] Error reporting
  - [x] Logging
  - [x] Zones
- **👮 Guards**
  - [x] Warden Selection
  - [x] Warden Laser and Paint
  - [x] Special Days
- **🎃 Prisoners**
  - [x] Last Request
  - [x] Rebel System
- **🛕 Maps**
  - [x] Automagic Cell Opening
  - [ ] Custom Entities
  - [ ] Custom I/O
  - [ ] Warden/Guard/Prisoner Filters

## Configuration

Configuration is done through CS#'s [FakeConVars](https://docs.cssharp.dev/examples/WithFakeConvars.html?q=fakeconvar).

You can search for the list of configurable
convars [like so](https://github.com/search?q=repo%3Aedgegamers%2FJailbreak%20fakeconvar&type=code).

## Architecture

Jailbreak is split into independent plugins that communicate via `PluginCapability`:

| Plugin | Description |
|--------|-------------|
| **Jailbreak.Contracts** | Shared interfaces, extensions, and models |
| **Jailbreak.Core** | Warden, rebel, mute, logs, last guard, teams |
| **Jailbreak.LastRequest** | Last request system (LR types, menus, management) |
| **Jailbreak.Fun** | Special days, roll-the-dice, rainbow effects |
| **Jailbreak.Zones** | Zone management, draw/beam shapes, SQL persistence |
| **Jailbreak.Gangs** | Gang perks (icons, colors, stats) via GangsAPI |
| **Jailbreak.Tools** | Debug/operator commands (`css_debug`) |

## Building

```shell
dotnet build JailbreakNew.sln
```

To publish all plugins:

```shell
dotnet publish src/Jailbreak.Core/Jailbreak.Core.csproj -o build/Jailbreak.Core
dotnet publish src/Jailbreak.LastRequest/Jailbreak.LastRequest.csproj -o build/Jailbreak.LastRequest
dotnet publish src/Jailbreak.Fun/Jailbreak.Fun.csproj -o build/Jailbreak.Fun
dotnet publish src/Jailbreak.Zones/Jailbreak.Zones.csproj -o build/Jailbreak.Zones
dotnet publish src/Jailbreak.Gangs/Jailbreak.Gangs.csproj -o build/Jailbreak.Gangs
dotnet publish src/Jailbreak.Tools/Jailbreak.Tools.csproj -o build/Jailbreak.Tools
```

Please use [SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or higher.

## Using

Jailbreak requires Counter Strike Sharp. If you don't have that installed, [follow the
install instructions here](https://docs.cssharp.dev/docs/guides/getting-started.html).

Install each plugin like any other Counter Strike Sharp plugin: drop each plugin folder into
`game/csgo/addons/counterstrikesharp/plugins`.
