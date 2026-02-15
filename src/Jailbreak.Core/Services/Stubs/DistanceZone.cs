using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Core.Services.Stubs;

/// <summary>
/// A zone defined by proximity to a set of points.
/// </summary>
public class DistanceZone : IZone {
  public const float WIDTH_CELL = 300f;

  private readonly IList<Vector> points;
  private readonly float width;

  public DistanceZone(IList<Vector> points, float width) {
    this.points = points;
    this.width = width;
  }

  public bool IsInsideZone(Vector? position) {
    if (position == null) return false;
    return points.Any(p => p.Distance(position) <= width);
  }
}
