using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Contracts.Extensions;

public static class ServerExtensions {
  /// <summary>
  ///   Get the current CCSGameRules for the server.
  /// </summary>
  public static CCSGameRules? GetGameRules() {
    return Utilities
     .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
     .First()
     .GameRules;
  }

  public static CCSGameRulesProxy? GetGameRulesProxy() {
    return Utilities
     .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
     .FirstOrDefault();
  }
}
