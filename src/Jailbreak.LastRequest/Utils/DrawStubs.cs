using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.LastRequest.Utils;

// TODO: Re-enable when Draw system is migrated to Contracts
// These stubs mirror the beam/shape interfaces from Jailbreak.Public.Mod.Draw

public enum BeamShapeType {
  CIRCLE,
  SQUARE,
  TRIANGLE,
  STAR,
  HEART,
  DIAMOND
}

public interface IBeamShapeFactory {
  BeamedPolylineShape CreateShape(Vector position, BeamShapeType shapeType,
    float? radius = null, float? width = null);
}

/// <summary>
/// Stub for BeamedPolylineShape. Real implementation lives in old Draw module.
/// </summary>
public class BeamedPolylineShape {
  public virtual void Move(Vector position) { }
  public virtual void SetRadius(float radius) { }
  public virtual void SetColor(Color color) { }
  public virtual void Update() { }
  public virtual void Draw() { }
  public virtual void Remove() { }
}

/// <summary>
/// No-op factory that creates stub shapes until the Draw system is migrated.
/// </summary>
public class StubBeamShapeFactory : IBeamShapeFactory {
  public BeamedPolylineShape CreateShape(Vector position,
    BeamShapeType shapeType, float? radius = null, float? width = null) {
    return new BeamedPolylineShape();
  }
}
