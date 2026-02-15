using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Services.Stubs;
using Jailbreak.Core.Services.Warden;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Core warden commands: css_warden, css_pass, css_fire.
/// Migrated from Jailbreak.Warden.Commands.WardenCommandsBehavior.
/// </summary>
public class WardenCommands {
  private readonly IWardenLocale locale;
  private readonly IWardenSelectionService queue;
  private readonly IWardenService warden;
  private readonly IGenericCmdLocale generics;
  private readonly Dictionary<CCSPlayerController, DateTime> lastPassCommand = new();

  public WardenCommands(IWardenLocale locale, IWardenSelectionService queue,
    IWardenService warden, IGenericCmdLocale generics) {
    this.locale = locale;
    this.queue = queue;
    this.warden = warden;
    this.generics = generics;
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    lastPassCommand.Clear();
    return HookResult.Continue;
  }

  [ConsoleCommand("css_pass", "Pass warden onto another player")]
  [ConsoleCommand("css_uw", "Pass warden onto another player")]
  [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
  public void Command_Pass(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.IsWarden(player)) return;

    locale.PassWarden(player).ToAllChat().ToAllCenter();

    foreach (var clients in Utilities.GetPlayers())
      clients.ExecuteClientCommand(
        $"play sounds/{WardenService.CV_WARDEN_SOUND_PASSED.Value}");

    locale.BecomeNextWarden.ToAllChat();

    if (!warden.TryRemoveWarden(true))
      Server.PrintToChatAll("[BUG] Couldn't remove warden :^(");

    lastPassCommand[player] = DateTime.Now;
  }

  [ConsoleCommand("css_fire", "Force the warden to pass")]
  [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
  public void Command_Fire(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.HasWarden || warden.Warden == null) {
      locale.CurrentWarden(null).ToChat(player);
      return;
    }

    if (!AdminManager.PlayerHasPermissions(player, "@css/ban")) {
      generics.NoPermissionMessage("@css/ban").ToChat(player);
      return;
    }

    foreach (var client in Utilities.GetPlayers()) {
      if (AdminManager.PlayerHasPermissions(client, "@css/chat"))
        locale.FireWarden(warden.Warden, player).ToChat(client);
      else
        locale.FireWarden(warden.Warden).ToChat(client);

      client.ExecuteClientCommand(
        $"play sounds/{WardenService.CV_WARDEN_SOUND_PASSED.Value}");
    }

    locale.BecomeNextWarden.ToAllChat();

    lastPassCommand[warden.Warden] = DateTime.Now;

    if (!warden.TryRemoveWarden(true))
      Server.PrintToChatAll("[BUG] Couldn't remove warden :^(");
  }

  [ConsoleCommand("css_warden",
    "Become a warden, Join the warden queue, or see information about the current warden.")]
  [ConsoleCommand("css_w",
    "Become a warden, Join the warden queue, or see information about the current warden.")]
  [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
  public void Command_Warden(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (player.Team != CsTeam.CounterTerrorist || !player.PawnIsAlive) return;

    if (lastPassCommand.TryGetValue(player, out var last)
      && !AdminManager.PlayerHasPermissions(player, "@css/rcon")) {
      var cooldown = last.AddSeconds(15);
      if (DateTime.Now < cooldown) {
        generics.CommandOnCooldown(cooldown).ToChat(player);
        return;
      }
    }

    if (RoundUtil.IsWarmup()) {
      locale.CannotWardenDuringWarmup.ToChat(player);
      return;
    }

    // Queue is open?
    if (queue.Active) {
      if (!queue.InQueue(player)) {
        if (queue.TryEnter(player)) locale.JoinRaffle.ToChat(player);
        return;
      }

      if (queue.InQueue(player))
        if (queue.TryExit(player))
          locale.LeaveRaffle.ToChat(player);

      return;
    }

    // Is a CT and there is no warden i.e. the queue is not open/active.
    if (!warden.HasWarden)
      if (warden.TrySetWarden(player))
        return;

    locale.CurrentWarden(warden.Warden).ToChat(player);
  }

  /// <summary>
  /// If the player who just died was the warden, clear the claim cooldown dictionary.
  /// </summary>
  [GameEventHandler]
  public HookResult OnWardenDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null) return HookResult.Continue;

    if (player != warden.Warden) return HookResult.Continue;

    lastPassCommand.Clear();
    return HookResult.Continue;
  }
}
