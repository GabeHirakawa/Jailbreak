using CounterStrikeSharp.API.Core;

namespace Jailbreak.Core.Services.Stubs;

/// <summary>
/// Stub interface for rebel service. Will be replaced when Rebel is migrated (Task 9).
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
