using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Models;
using Jailbreak.Contracts.Services;

namespace Jailbreak.Contracts;

/// <summary>
/// The main API surface exposed by the Jailbreak Core plugin.
/// Satellite plugins consume this via PluginCapability.
/// </summary>
public interface IJailbreakCore {
    // State
    RoundState RoundState { get; }
    void SetRoundState(RoundState state);

    // Players
    IEnumerable<CCSPlayerController> GetAlivePlayers();
    IEnumerable<CCSPlayerController> GetAlivePlayers(CsTeam team);
    bool IsRebel(CCSPlayerController player);
    void MarkRebel(CCSPlayerController player);

    // Sub-APIs (accessed as properties)
    IWardenService Warden { get; }
    IWeaponService Weapons { get; }

    // Communication
    void Announce(string message);
    void AnnounceCenter(string message);
    void Message(CCSPlayerController player, string message);
}
