using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Models;

namespace Jailbreak.Core.Services.Rebel;

/// <summary>
/// Listens for player damage/death events and marks attackers as rebels.
/// Migrated from Jailbreak.Rebel.RebelListener.
/// </summary>
public class RebelListener {
  private readonly IRebelService rebelService;
  // TODO: Re-enable LastRequest integration when LastRequest plugin is migrated (Task 11)
  // private readonly ILastRequestManager lastRequestManager;
  // TODO: Re-enable LastGuard integration when available
  // private readonly ILastGuardService lastGuard;

  private readonly Dictionary<int, int> weaponScores = [];

  public RebelListener(IRebelService rebelService) {
    this.rebelService = rebelService;
  }

  public void Initialize(BasePlugin basePlugin) {
    basePlugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
    basePlugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
    // TODO: Re-enable OnDeath for rebel kill credits when Gangs is available (Task 15)
    // basePlugin.RegisterEventHandler<EventPlayerDeath>(OnDeath, HookMode.Pre);
  }

  public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal()) return HookResult.Continue;
    if (player.Team != CsTeam.CounterTerrorist) return HookResult.Continue;

    var attacker = @event.Attacker;
    if (attacker == null || !attacker.IsReal()) return HookResult.Continue;

    if (attacker.Team != CsTeam.Terrorist) return HookResult.Continue;

    // TODO: Re-enable LastRequest check when LastRequest plugin is migrated
    // if (lastRequestManager.IsInLR(attacker)
    //   || lastRequestManager.IsInLR(player))
    //   return HookResult.Continue;

    var weapon = "weapon_" + @event.Weapon;
    if (!weaponScores.TryGetValue(attacker.Slot, out var old)) old = 0;

    if (WeaponTag.SNIPERS.Contains(weapon) && weapon != "weapon_ssg08")
      weaponScores[attacker.Slot] = Math.Max(30, old);
    else if (WeaponTag.RIFLES.Contains(weapon))
      weaponScores[attacker.Slot] = Math.Max(25, old);
    else if (WeaponTag.GUNS.Contains(weapon))
      weaponScores[attacker.Slot] = Math.Max(15, old);
    else
      weaponScores[attacker.Slot] = Math.Max(10, old);

    rebelService.MarkRebel(attacker);
    return HookResult.Continue;
  }

  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    weaponScores.Clear();
    return HookResult.Continue;
  }

  // TODO: Re-enable rebel kill credits when Gangs integration is available (Task 15)
  // public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info) { ... }
}
