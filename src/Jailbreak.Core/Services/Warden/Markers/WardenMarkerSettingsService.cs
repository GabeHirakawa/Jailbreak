using System.Collections.Concurrent;
using System.Drawing;
using Jailbreak.Core.Services.Stubs;

namespace Jailbreak.Core.Services.Warden.Markers;

/// <summary>
/// Manages per-player warden marker settings (type and color).
/// Migrated from Jailbreak.Warden.Markers.WardenMarkerSettings.
/// Cookie/Actain integration is stubbed out until migrated.
/// </summary>
public class WardenMarkerSettingsService : IWardenMarkerSettings {
  private readonly IBeamShapeRegistry registry;
  private readonly ConcurrentDictionary<ulong, Stubs.MarkerSettings> cache = new();

  public WardenMarkerSettingsService(IBeamShapeRegistry registry) {
    this.registry = registry;
  }

  public Stubs.MarkerSettings? GetCachedSettings(ulong steamId) {
    return cache.TryGetValue(steamId, out var cached) ? cached : null;
  }

  public async Task EnsureCachedAsync(ulong steamId) {
    if (cache.ContainsKey(steamId)) return;
    // TODO: Re-enable when Actain/cookie service is migrated
    // For now, just store defaults
    cache[steamId] = new Stubs.MarkerSettings(BeamShapeType.CIRCLE, Color.White);
    await Task.CompletedTask;
  }

  public async Task SetTypeAsync(ulong steamId, BeamShapeType type) {
    // TODO: Re-enable when Actain/cookie service is migrated
    var current = cache.GetValueOrDefault(steamId,
      new Stubs.MarkerSettings(BeamShapeType.CIRCLE, Color.White));
    cache[steamId] = new Stubs.MarkerSettings(type, current.color);
    await Task.CompletedTask;
  }

  public async Task SetColorAsync(ulong steamId, string colorKey) {
    // TODO: Re-enable when Actain/cookie service is migrated
    var color = Color.White;
    var colors = registry.GetAllColors();
    if (colors.TryGetValue(colorKey, out var c)) color = c;

    var current = cache.GetValueOrDefault(steamId,
      new Stubs.MarkerSettings(BeamShapeType.CIRCLE, Color.White));
    cache[steamId] = new Stubs.MarkerSettings(current.Type, color);
    await Task.CompletedTask;
  }

  public void Invalidate(ulong steamId) => cache.TryRemove(steamId, out _);
}
