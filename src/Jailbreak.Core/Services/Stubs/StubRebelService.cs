using CounterStrikeSharp.API.Core;

namespace Jailbreak.Core.Services.Stubs;

/// <summary>
/// No-op implementation of IRebelService. Will be replaced when Rebel is migrated (Task 9).
/// </summary>
public class StubRebelService : IRebelService {
  public ISet<CCSPlayerController> GetActiveRebels() => new HashSet<CCSPlayerController>();
  public long GetRebelTimeLeft(CCSPlayerController player) => 0;
  public bool MarkRebel(CCSPlayerController player, long time = -1) => false;
  public void UnmarkRebel(CCSPlayerController player) { }
  public void DisableRebelForRound() { }
}
