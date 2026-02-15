using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Logs damage-related events (grenade throws, player hurt, player death).
/// Migrated from Jailbreak.Logs.Listeners.LogDamageListeners.
/// </summary>
public class LogDamageListeners {
  private readonly IRichLogService logs;

  public LogDamageListeners(IRichLogService logs) { this.logs = logs; }

  public HookResult OnGrenadeThrown(EventGrenadeThrown @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal()) return HookResult.Continue;
    var grenade = @event.Weapon;

    logs.Append(logs.Player(player), $"threw a {grenade}");

    return HookResult.Continue;
  }

  public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal()) return HookResult.Continue;
    var attacker = @event.Attacker;

    var isWorld = attacker == null || !attacker.IsReal();
    var health  = @event.DmgHealth;

    if (isWorld) {
      logs.Append("The world hurt", logs.Player(player),
        $"for {health} damage");
    } else {
      logs.Append(logs.Player(attacker!), "hurt", logs.Player(player),
        $"for {health} damage");
    }

    return HookResult.Continue;
  }

  public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal()) return HookResult.Continue;
    var attacker = @event.Attacker;

    var isWorld = attacker == null || !attacker.IsReal();

    if (isWorld) {
      logs.Append("The world killed", logs.Player(player));
    } else {
      logs.Append(logs.Player(attacker!), "killed", logs.Player(player));
    }

    return HookResult.Continue;
  }
}
