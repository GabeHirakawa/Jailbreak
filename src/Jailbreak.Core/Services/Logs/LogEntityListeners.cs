using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Logs entity interaction events (button presses, breakable entities).
/// Migrated from Jailbreak.Logs.Listeners.LogEntityListeners.
/// </summary>
public class LogEntityListeners {
  private readonly IRichLogService logs;

  public LogEntityListeners(IRichLogService logs) { this.logs = logs; }

  public HookResult OnButtonPressed(CEntityIOOutput output, string name,
    CEntityInstance activator, CEntityInstance caller, CVariant value,
    float delay) {
    if (!activator.TryGetController(out var player)) return HookResult.Continue;
    if (player == null || !player.IsReal()) return HookResult.Continue;

    var ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);

    logs.Append(logs.Player(player),
      $"pressed a button: {ent?.Entity?.Name ?? "Unlabeled"} -> {output.Connections?.TargetDesc ?? "None"}");
    return HookResult.Continue;
  }

  public HookResult OnBreakableBroken(CEntityIOOutput output, string name,
    CEntityInstance activator, CEntityInstance caller, CVariant value,
    float delay) {
    if (!activator.TryGetController(out var player)) return HookResult.Continue;
    if (player == null || !player.IsReal()) return HookResult.Continue;

    var ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);

    logs.Append(logs.Player(player),
      $"broke an entity: {ent?.Entity?.Name ?? "Unlabeled"} -> {output.Connections?.TargetDesc ?? "None"}");
    return HookResult.Continue;
  }
}
