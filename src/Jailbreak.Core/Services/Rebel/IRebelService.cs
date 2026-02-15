using CounterStrikeSharp.API.Core;

namespace Jailbreak.Core.Services.Rebel;

/// <summary>
/// Service for tracking and managing rebel prisoners.
/// Migrated from Jailbreak.Public.Mod.Rebel.IRebelService.
/// </summary>
public interface IRebelService {
  ISet<CCSPlayerController> GetActiveRebels();

  bool IsRebel(CCSPlayerController player) {
    return GetRebelTimeLeft(player) > 0;
  }

  long GetRebelTimeLeft(CCSPlayerController player);

  bool MarkRebel(CCSPlayerController player, long time = -1);

  void UnmarkRebel(CCSPlayerController player);

  void DisableRebelForRound();
}
