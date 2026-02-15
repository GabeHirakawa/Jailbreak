using CounterStrikeSharp.API;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Fun.Utils;

/// <summary>
/// Utility for round time manipulation.
/// </summary>
public static class RoundUtil {
  public static bool IsWarmup() {
    var rules = ServerExtensions.GetGameRules();
    return rules == null || rules.WarmupPeriod;
  }

  public static int GetTimeRemaining() {
    var rules = ServerExtensions.GetGameRules();
    if (rules == null) return 0;
    var freezeTime = rules.FreezeTime;
    var elapsed = Server.CurrentTime - rules.RoundStartTime - freezeTime;
    var roundTime = rules.RoundTime;
    return (int)(roundTime - elapsed);
  }

  public static int GetTimeElapsed() {
    var gamerules = ServerExtensions.GetGameRules();
    if (gamerules == null) return 0;
    var freezeTime = gamerules.FreezeTime;
    return (int)(Server.CurrentTime - gamerules.RoundStartTime - freezeTime);
  }

  public static void AddTimeRemaining(int seconds) {
    var proxy = ServerExtensions.GetGameRulesProxy();
    if (proxy?.GameRules == null) return;
    proxy.GameRules.RoundTime += seconds;
  }

  public static void SetTimeRemaining(int seconds) {
    var rules = ServerExtensions.GetGameRules();
    if (rules == null) return;
    var freezeTime = rules.FreezeTime;
    var elapsed = Server.CurrentTime - rules.RoundStartTime - freezeTime;
    rules.RoundTime = (int)elapsed + seconds;
  }
}
