using System.Diagnostics;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Models;

namespace Jailbreak.Fun.Services.RTD.Rewards;

public class AmmoWeaponReward : WeaponReward {
  private readonly int primary, secondary;

  public AmmoWeaponReward(string weapon, int primary, int secondary,
    CsTeam requiredTeam = CsTeam.Terrorist) : base(weapon, requiredTeam) {
    Trace.Assert(WeaponTag.GUNS.Contains(weapon));
    this.primary   = primary;
    this.secondary = secondary;
  }

  public override string Name
    => primary + secondary == 0 ?
      $"Toy {weapon.GetFriendlyWeaponName()}" :
      $"{weapon.GetFriendlyWeaponName()} ({primary}/{secondary})";

  public override bool GrantReward(CCSPlayerController player) {
    player.GiveNamedItem(weapon);
    player.GetWeaponBase(weapon)?.SetAmmo(primary, secondary);
    return true;
  }
}
