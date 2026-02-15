namespace Jailbreak.Core.Services.Stubs;

/// <summary>
/// Stub interface for mute service. Will be replaced when Mute is migrated (Task 9).
/// </summary>
public interface IMuteService {
  void PeaceMute(MuteReason reason);
  void UnPeaceMute();
  bool IsPeaceEnabled();
  DateTime GetLastPeace();
}

public enum MuteReason {
  INITIAL_WARDEN,
  WARDEN_TAKEN,
  WARDEN_INVOKED,
  ADMIN
}
