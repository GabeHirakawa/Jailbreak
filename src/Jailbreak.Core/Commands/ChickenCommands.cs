using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Locale;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Chicken spawn command for the warden.
/// Migrated from Jailbreak.Warden.Commands.ChickenCommandBehavior.
/// </summary>
public class ChickenCommands {
  public static readonly FakeConVar<int> CV_MAX_CHICKENS =
    new("css_jb_max_chickens",
      "The maximum number of chickens that the warden can spawn", 5);

  private readonly IWardenService warden;
  private readonly ICoreLocale locale;
  private int chickens;

  public ChickenCommands(IWardenService warden, ICoreLocale locale) {
    this.warden = warden;
    this.locale = locale;
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    chickens = 0;
    return HookResult.Continue;
  }

  [ConsoleCommand("css_chicken", "Spawn a chicken as the warden")]
  public void Command_Toggle(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.IsWarden(player)) {
      locale.NotWarden.ToChat(player);
      return;
    }

    if (chickens >= CV_MAX_CHICKENS.Value) {
      locale.TooManyChickens.ToChat(player);
      return;
    }

    var chicken = Utilities.CreateEntityByName<CChicken>("chicken");
    if (chicken == null || !chicken.IsValid) {
      locale.ChickenSpawnFailed.ToChat(player);
      return;
    }

    var loc = player.Pawn.Value?.AbsOrigin;
    if (loc == null) {
      locale.ChickenSpawnFailed.ToChat(player);
      return;
    }

    chicken.Teleport(loc);
    locale.ChickenSpawned.ToAllChat();
    chicken.DispatchSpawn();
    chickens++;
  }
}
