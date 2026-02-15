using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.LastRequest.Enums;
using Jailbreak.LastRequest.Locale;

namespace Jailbreak.LastRequest.Services.Types;

/// <summary>
///   Represents a Last Request that involves direct PvP combat.
///   Automatically strips weapons, counts down, and calls Execute after 5 seconds.
/// </summary>
public abstract class WeaponizedRequest(BasePlugin plugin,
  ILastRequestManager manager, ILastRequestLocale messages,
  CCSPlayerController prisoner,
  CCSPlayerController guard)
  : TeleportingRequest(plugin, manager, prisoner, guard) {
  public override void Setup() {
    base.Setup();

    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();
    for (var i = 5; i >= 1; i--) {
      var copy = i;
      Plugin.AddTimer(5 - i,
        () => { messages.LastRequestCountdown(copy).ToChat(Prisoner, Guard); });
    }

    Plugin.AddTimer(5, () => {
      if (State != LRState.PENDING) return;
      Execute();
    });
  }

  public override void OnEnd(LRResult result) {
    switch (result) {
      case LRResult.GUARD_WIN:
        Prisoner.Pawn.Value?.CommitSuicide(false, true);
        break;
      case LRResult.PRISONER_WIN:
        Guard.Pawn.Value?.CommitSuicide(false, true);
        break;
    }

    State = LRState.COMPLETED;
  }
}
