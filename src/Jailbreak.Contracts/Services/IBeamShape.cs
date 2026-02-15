namespace Jailbreak.Contracts.Services;

/// <summary>
/// Defines a 2D shape in unit space (roughly normalized to [-1, 1])
/// for rendering as 3D beams. Each shape is represented as a polyline
/// with 2D points that can be scaled and projected into world space.
/// </summary>
public interface IBeamShape {
    /// <summary>
    /// Display name for this shape (e.g., "Circle", "Star", "Heart").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The unit 2D points that define the shape in local space.
    /// These points should be roughly normalized to the [-1, 1] range.
    /// </summary>
    IReadOnlyList<(float x, float y)> UnitPoints { get; }

    /// <summary>
    /// Whether the shape is closed (the last point connects back to the first).
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Default radius/scale for this shape when rendered in world space.
    /// </summary>
    float DefaultRadius { get; }

    /// <summary>
    /// Default line width for rendering this shape.
    /// </summary>
    float DefaultWidth { get; }
}
