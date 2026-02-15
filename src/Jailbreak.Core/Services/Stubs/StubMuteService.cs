namespace Jailbreak.Core.Services.Stubs;

/// <summary>
/// No-op implementation of IMuteService. Will be replaced when Mute is migrated (Task 9).
/// </summary>
public class StubMuteService : IMuteService {
  private DateTime lastPeace = DateTime.MinValue;

  public void PeaceMute(MuteReason reason) { lastPeace = DateTime.Now; }
  public void UnPeaceMute() { }
  public bool IsPeaceEnabled() { return false; }
  public DateTime GetLastPeace() { return lastPeace; }
}
