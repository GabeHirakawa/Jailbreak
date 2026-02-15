using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace Jailbreak.Core.Services.State;

public class RoundStateTracker : BaseStateTracker {
  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info) {
    ResetAll();

    return HookResult.Continue;
  }
}
