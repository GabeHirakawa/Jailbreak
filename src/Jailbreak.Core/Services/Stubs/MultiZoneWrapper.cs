using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Core.Services.Stubs;

/// <summary>
/// Wraps multiple zones into a single zone check.
/// </summary>
public class MultiZoneWrapper : IZone {
  private readonly IList<IZone> zones;

  public MultiZoneWrapper(IList<IZone> zones) {
    this.zones = zones;
  }

  public bool IsInsideZone(Vector? position) {
    return zones.Any(z => z.IsInsideZone(position));
  }
}
