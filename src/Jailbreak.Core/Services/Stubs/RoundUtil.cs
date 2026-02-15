using CounterStrikeSharp.API;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Core.Services.Stubs;

// TODO: Replace with proper RoundUtil from Contracts when available
// Mirrors Jailbreak.Public.Utils.RoundUtil

public static class RoundUtil {
  public static int GetTimeElapsed() {
    var gamerules = ServerExtensions.GetGameRules();
    if (gamerules == null) return 0;
    var freezeTime = gamerules.FreezeTime;
    return (int)(Server.CurrentTime - gamerules.RoundStartTime - freezeTime);
  }

  public static bool IsWarmup() {
    var rules = ServerExtensions.GetGameRules();
    return rules == null || rules.WarmupPeriod;
  }
}
