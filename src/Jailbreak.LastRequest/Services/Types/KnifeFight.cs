using CounterStrikeSharp.API.Core;
using Jailbreak.LastRequest.Enums;
using Jailbreak.LastRequest.Locale;

namespace Jailbreak.LastRequest.Services.Types;

public class KnifeFight(BasePlugin plugin, ILastRequestManager manager,
  ILastRequestLocale messages,
  CCSPlayerController prisoner, CCSPlayerController guard)
  : WeaponizedRequest(plugin, manager, messages, prisoner, guard) {
  public override LRType Type => LRType.KNIFE_FIGHT;

  public override void Execute() {
    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();
    Prisoner.GiveNamedItem("weapon_knife");
    Guard.GiveNamedItem("weapon_knife");
    State = LRState.ACTIVE;
  }

  public override void OnEnd(LRResult result) { State = LRState.COMPLETED; }
}
