using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Zones.Enums;

namespace Jailbreak.Zones.Services.Draw;

public interface IBeamShapeFactory {
  BeamedPolylineShape CreateShape(Vector position,
    BeamShapeType shapeType, float? radius = null, float? width = null);
}
