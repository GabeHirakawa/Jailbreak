using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Core.Services.Stubs;

/// <summary>
/// Stub interface for zone manager. Will be replaced when Zones is migrated (Task 14).
/// </summary>
public interface IZoneManager {
  Task<IList<IZone>> GetZones(string map, ZoneType type);
}

public interface IZone {
  bool IsInsideZone(Vector? position);
}

public enum ZoneType {
  CELL,
  ARMORY,
  CUSTOM
}
