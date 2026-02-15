using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Services.Stubs;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Soccer ball spawn command for the warden.
/// Migrated from Jailbreak.Warden.Commands.SoccerCommandBehavior.
/// </summary>
public class SoccerCommands {
  public static readonly FakeConVar<int> CV_MAX_SOCCERS =
    new("css_jb_max_soccers",
      "The maximum number of soccer balls that the warden can spawn", 3);

  private readonly IWardenService warden;
  private readonly IWardenLocale wardenLocale;
  private readonly IWardenCmdSoccerLocale locale;
  private int soccerBalls;

  public SoccerCommands(IWardenService warden,
    IWardenLocale wardenLocale, IWardenCmdSoccerLocale locale) {
    this.warden = warden;
    this.wardenLocale = wardenLocale;
    this.locale = locale;
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    soccerBalls = 0;
    return HookResult.Continue;
  }

  [ConsoleCommand("css_soccer", "Spawn a soccer ball as the warden")]
  [ConsoleCommand("css_spawnball", "Spawn a soccer ball as the warden")]
  public void Command_Toggle(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.IsWarden(player)) {
      wardenLocale.NotWarden.ToChat(player);
      return;
    }

    if (soccerBalls >= CV_MAX_SOCCERS.Value) {
      locale.TooManySoccers.ToChat(player);
      return;
    }

    var ball =
      Utilities.CreateEntityByName<CPhysicsPropMultiplayer>(
        "prop_physics_multiplayer");
    if (ball == null || !ball.IsValid) {
      locale.SpawnFailed.ToChat(player);
      return;
    }

    var loc = player.Pawn.Value?.AbsOrigin;
    if (loc == null) {
      locale.SpawnFailed.ToChat(player);
      return;
    }

    ball.SetModel(
      "models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl");
    ball.Teleport(loc);
    locale.SoccerSpawned.ToAllChat();
    ball.DispatchSpawn();
    soccerBalls++;
  }
}
