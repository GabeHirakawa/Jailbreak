using CounterStrikeSharp.API;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Tools.Utils;

/// <summary>
/// Utility for round time manipulation.
/// </summary>
public static class RoundUtil {
  public static void SetTimeRemaining(int seconds) {
    var rules = ServerExtensions.GetGameRules();
    if (rules == null) return;
    var freezeTime = rules.FreezeTime;
    var elapsed = Server.CurrentTime - rules.RoundStartTime - freezeTime;
    rules.RoundTime = (int)elapsed + seconds;
  }
}
