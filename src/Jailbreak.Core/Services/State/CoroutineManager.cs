using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Core.Services.State;

public class CoroutineManager : ICoroutines {
  private readonly List<Timer> destroyOnRoundEnd = [];

  public void Round(Action callback, float time = 10) {
    var timer = create(callback, time);
    destroyOnRoundEnd.Add(timer);
  }

  private Timer create(Action callback, float time = 10) {
    return new Timer(time, callback, TimerFlags.STOP_ON_MAPCHANGE);
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info) {
    destroyOnRoundEnd.ForEach(timer => timer.Kill());

    return HookResult.Continue;
  }
}
