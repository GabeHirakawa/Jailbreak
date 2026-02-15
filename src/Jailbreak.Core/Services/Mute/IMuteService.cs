namespace Jailbreak.Core.Services.Mute;

/// <summary>
/// Service for managing voice muting during peace periods.
/// Migrated from Jailbreak.Public.Mod.Mute.IMuteService.
/// </summary>
public interface IMuteService {
  void PeaceMute(MuteReason reason);
  void UnPeaceMute();
  bool IsPeaceEnabled();
  DateTime GetLastPeace();
}

/// <summary>
/// Reasons for invoking a peace mute.
/// Migrated from Jailbreak.Public.Mod.Mute.MuteReason.
/// </summary>
public enum MuteReason {
  /// <summary>
  ///   An admin invoked the peace
  /// </summary>
  ADMIN,

  /// <summary>
  ///   The warden invoked the peace
  /// </summary>
  WARDEN_INVOKED,

  /// <summary>
  ///   The first warden of the round has been assigned
  /// </summary>
  INITIAL_WARDEN,

  /// <summary>
  ///   A new warden has been assigned
  /// </summary>
  WARDEN_TAKEN
}
