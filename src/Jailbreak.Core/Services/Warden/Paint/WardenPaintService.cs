using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using CS2TraceRay.Class;
using CS2TraceRay.Enum;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Services.Stubs;
using Microsoft.Extensions.DependencyInjection;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Core.Services.Warden.Paint;

/// <summary>
/// Warden paint behavior - draws lines where the warden looks.
/// Migrated from Jailbreak.Warden.Paint.WardenPaintBehavior.
/// Gangs paint color perk is disabled until Gangs is migrated.
/// </summary>
public class WardenPaintService {
  private readonly IWardenService wardenService;
  private readonly IRainbowColorizer? colorizer;

  private Vector? lastPosition;
  private BasePlugin? parent;

  public WardenPaintService(IWardenService wardenService,
    IServiceProvider provider) {
    this.wardenService = wardenService;
    colorizer = provider.GetService<IRainbowColorizer>();
  }

  /// <summary>
  /// Initialize the service with a reference to the plugin for tick listener.
  /// Called from CorePlugin.Load().
  /// </summary>
  public void Initialize(BasePlugin basePlugin) {
    parent = basePlugin;
    basePlugin.RegisterListener<Listeners.OnTick>(paint);
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    return HookResult.Continue;
  }

  private void paint() {
    if (!wardenService.HasWarden) return;

    var warden = wardenService.Warden;
    if (warden == null || !warden.IsReal()) return;

    if ((warden.Buttons & CounterStrikeSharp.API.PlayerButtons.Use) == 0) return;

    var trace =
      warden.GetGameTraceByEyePosition(TraceMask.MaskSolid, Contents.TouchAll,
        warden);
    if (trace == null) return;

    var position = trace.Value.Position.ToCsVector();

    var start = lastPosition ?? position;
    start = start.Clone();

    if (lastPosition != null
      && position.DistanceSquared(lastPosition) < 25 * 25)
      return;

    lastPosition = position;
    if (start.DistanceSquared(position) > 150 * 150) start = position;

    if (parent == null)
      throw new NullReferenceException("Parent plugin is null");

    // TODO: Re-enable when Gangs plugin is migrated (Task 15)
    // For now, always use white color
    var color = Color.White;

    var line = new BeamLine(parent, start.Clone(), position.Clone());
    line.SetColor(color);
    line.SetWidth(1.5f);
    line.Draw(30);
  }
}
