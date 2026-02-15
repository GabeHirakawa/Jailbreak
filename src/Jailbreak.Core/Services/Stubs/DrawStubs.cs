using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Core.Services.Stubs;

// TODO: Re-enable when Draw system is migrated
// These stubs represent the draw/beam interfaces from Jailbreak.Public.Mod.Draw

public enum BeamShapeType {
  CIRCLE,
  SQUARE,
  TRIANGLE,
  STAR,
  HEART,
  DIAMOND
}

public static class BeamShapeTypeExtensions {
  public static string ToFriendlyString(this BeamShapeType type) {
    return type switch {
      BeamShapeType.CIRCLE   => "Circle",
      BeamShapeType.SQUARE   => "Square",
      BeamShapeType.TRIANGLE => "Triangle",
      BeamShapeType.STAR     => "Star",
      BeamShapeType.HEART    => "Heart",
      BeamShapeType.DIAMOND  => "Diamond",
      _                      => type.ToString()
    };
  }
}

public readonly record struct MarkerSettings(BeamShapeType Type, Color color);

public interface IBeamShapeRegistry {
  IEnumerable<BeamShapeType> GetAllTypes();
  Dictionary<string, Color> GetAllColors();
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
  public virtual void Remove() { }
}

public interface ITextSpawner {
  IEnumerable<CPointWorldText> CreateTextHat(TextSetting setting,
    CCSPlayerController player);
}

public class TextSetting {
  public string msg { get; set; } = "";
  public Color color { get; set; } = Color.White;
}

public interface IRainbowColorizer {
  Color GetRainbow();
}

/// <summary>
/// Stub for BeamLine. Real implementation lives in old Draw module.
/// </summary>
public class BeamLine {
  private readonly BasePlugin plugin;
  private readonly Vector start;
  private readonly Vector end;

  public BeamLine(BasePlugin plugin, Vector start, Vector end) {
    this.plugin = plugin;
    this.start = start;
    this.end = end;
  }

  public void SetColor(Color color) { }
  public void SetWidth(float width) { }
  public void Draw(float duration) { }
}
