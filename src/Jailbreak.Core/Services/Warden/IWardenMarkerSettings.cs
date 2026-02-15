using Jailbreak.Core.Services.Stubs;

namespace Jailbreak.Core.Services.Warden;

/// <summary>
/// Service for managing per-player warden marker settings.
/// Co-located from Jailbreak.Public.Mod.Warden.IWardenMarkerSettings.
/// </summary>
public interface IWardenMarkerSettings {
  MarkerSettings? GetCachedSettings(ulong steamId);
  Task EnsureCachedAsync(ulong steamId);
  Task SetTypeAsync(ulong steamId, BeamShapeType type);
  Task SetColorAsync(ulong steamId, string colorKey);
  void Invalidate(ulong steamId);
}
