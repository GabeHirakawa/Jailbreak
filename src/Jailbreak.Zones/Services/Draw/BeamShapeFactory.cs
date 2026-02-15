using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Zones.Enums;

namespace Jailbreak.Zones.Services.Draw;

public class BeamShapeFactory(IBeamShapeRegistry registry) : IBeamShapeFactory {
  private BasePlugin plugin = null!;
  public void Start(BasePlugin basePlugin) { plugin = basePlugin; }

  public BeamedPolylineShape CreateShape(Vector position,
    BeamShapeType shapeType, float? radius = null, float? width = null) {
    var def = registry.Get(shapeType);

    return new BeamedPolylineShape(plugin, position, def, radius, width);
  }
}
