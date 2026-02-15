using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Zones.Services.Zones;

public class BasicZoneFactory : IZoneFactory {
  public IZone CreateZone(IEnumerable<Vector> origins) {
    return new DistanceZone(origins.ToList(), 0);
  }
}
