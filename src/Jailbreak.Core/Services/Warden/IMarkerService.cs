using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Core.Services.Warden;

/// <summary>
/// Service for managing the warden marker.
/// Co-located from Jailbreak.Public.Mod.Warden.IMarkerService.
/// </summary>
public interface IMarkerService {
  Vector? MarkerPosition { get; }
  float Radius { get; }

  bool InMarker(Vector pos) {
    if (MarkerPosition == null) return false;
    var widenedRadius = Radius + 32;
    return MarkerPosition.DistanceSquared(pos) <= widenedRadius * widenedRadius;
  }

  bool InMarker(CCSPlayerController player) {
    if (MarkerPosition == null) return false;
    var pos = player.PlayerPawn.Value?.AbsOrigin;
    return pos != null && InMarker(pos);
  }
}
