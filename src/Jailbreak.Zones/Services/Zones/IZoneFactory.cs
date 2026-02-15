using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Zones.Services.Zones;

public interface IZoneFactory {
  IZone CreateZone(IEnumerable<Vector> origins);
}
