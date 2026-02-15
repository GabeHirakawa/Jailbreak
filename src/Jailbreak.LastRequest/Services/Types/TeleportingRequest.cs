using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Extensions;
using Jailbreak.LastRequest.Enums;

namespace Jailbreak.LastRequest.Services.Types;

public abstract class TeleportingRequest(BasePlugin plugin,
  ILastRequestManager manager, CCSPlayerController prisoner,
  CCSPlayerController guard)
  : AbstractLastRequest(plugin, manager, prisoner, guard) {
  public override void Setup() {
    State = LRState.PENDING;

    Guard.Teleport(Prisoner);

    Guard.Freeze();
    Prisoner.Freeze();
    Plugin.AddTimer(1, () => { Guard.UnFreeze(); });
    Plugin.AddTimer(2, () => { Prisoner.UnFreeze(); });
  }
}
