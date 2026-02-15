using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace Jailbreak.Core.Services.State;

public class AliveStateTracker : BaseStateTracker {
  [GameEventHandler]
  public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info) {
    if (ev.Userid != null) Reset(ev.Userid);
    return HookResult.Continue;
  }
}
