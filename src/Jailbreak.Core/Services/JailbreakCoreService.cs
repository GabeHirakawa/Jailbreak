using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts;
using Jailbreak.Contracts.Models;
using Jailbreak.Contracts.Services;

namespace Jailbreak.Core.Services;

public class JailbreakCoreService : IJailbreakCore {
    public RoundState RoundState { get; private set; } = RoundState.Normal;

    // These will be injected once the services are migrated
    public IWardenService Warden { get; }
    public IWeaponService Weapons { get; }

    public JailbreakCoreService(IWardenService warden, IWeaponService weapons) {
        Warden = warden;
        Weapons = weapons;
    }

    public void SetRoundState(RoundState state) {
        RoundState = state;
    }

    public IEnumerable<CCSPlayerController> GetAlivePlayers() {
        return Utilities.GetPlayers()
            .Where(p => p.IsValid && p.PawnIsAlive);
    }

    public IEnumerable<CCSPlayerController> GetAlivePlayers(CsTeam team) {
        return GetAlivePlayers().Where(p => p.Team == team);
    }

    public bool IsRebel(CCSPlayerController player) {
        // Will be implemented when Rebel service is migrated
        return false;
    }

    public void MarkRebel(CCSPlayerController player) {
        // Will be implemented when Rebel service is migrated
    }

    public void Announce(string message) {
        foreach (var player in Utilities.GetPlayers())
            player.PrintToChat(message);
    }

    public void AnnounceCenter(string message) {
        foreach (var player in Utilities.GetPlayers())
            player.PrintToCenter(message);
    }

    public void Message(CCSPlayerController player, string message) {
        player.PrintToChat(message);
    }
}
