using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.LastRequest.Enums;
using Jailbreak.LastRequest.Locale;
using Jailbreak.LastRequest.Services;

namespace Jailbreak.LastRequest.Commands;

public class LastRequestCommands {
  private readonly ILastRequestManager lastRequestManager;
  private readonly ILastRequestRebelManager lastRequestRebelManager;
  private readonly ILastRequestLocale messages;
  private readonly ILastRequestFactory factory;
  private readonly BasePlugin plugin;
  private LastRequestMenuSelector? menuSelector;
  private LastRequestPlayerSelector? playerSelector;

  public LastRequestCommands(ILastRequestManager lastRequestManager,
    ILastRequestRebelManager lastRequestRebelManager,
    ILastRequestLocale messages, ILastRequestFactory factory,
    BasePlugin plugin) {
    this.lastRequestManager      = lastRequestManager;
    this.lastRequestRebelManager = lastRequestRebelManager;
    this.messages                = messages;
    this.factory                 = factory;
    this.plugin                  = plugin;

    playerSelector = new LastRequestPlayerSelector(lastRequestManager, plugin);
    menuSelector   = new LastRequestMenuSelector(factory, plugin);
  }

  public void Command_LastRequest(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor == null || !executor.IsReal()) return;
    if (!lastRequestManager.IsLREnabled) {
      messages.LastRequestNotEnabled().ToChat(executor);
      return;
    }

    if (executor.Team != CsTeam.Terrorist) {
      messages.CannotLR("You are not a Prisoner").ToChat(executor);
      return;
    }

    if (!executor.PawnIsAlive) {
      messages.CannotLR("You are not alive").ToChat(executor);
      return;
    }

    if (!playerSelector!.WouldHavePlayers()) {
      messages.CannotLR("No players available to LR").ToChat(executor);
      return;
    }

    if (lastRequestManager.IsInLR(executor)
      || lastRequestRebelManager.IsInLRRebelling(executor.Slot)) {
      messages.CannotLR("You are already in an LR").ToChat(executor);
      return;
    }

    if (info.ArgCount == 1) {
      MenuManager.OpenCenterHtmlMenu(plugin, executor,
        menuSelector!.GetMenu());
      return;
    }

    var type = LRTypeExtensions.FromString(info.GetArg(1));
    if (type is null) {
      messages.InvalidLastRequest(info.GetArg(1)).ToChat(executor);
      return;
    }

    if (info.ArgCount == 2) {
      MenuManager.OpenCenterHtmlMenu(plugin, executor,
        playerSelector.CreateMenu(executor,
          str => "css_lr " + type + " #" + str));
      return;
    }

    var target = info.GetArgTargetResult(2);
    if (target.Players.Count == 0) {
      executor.PrintToChat("Player not found: " + info.GetArg(2));
      return;
    }

    if (target.Players.Count > 1) {
      executor.PrintToChat("Multiple players found: " + info.GetArg(2));
      return;
    }

    var player = target.Players.First();
    if (player.Team != CsTeam.CounterTerrorist) {
      messages.CannotLR(player, "They are not a Guard").ToChat(executor);
      return;
    }

    if (!player.PawnIsAlive) {
      messages.CannotLR(player, "They are not alive").ToChat(executor);
      return;
    }

    if (lastRequestManager.IsInLR(player)) {
      messages.CannotLR(player, "They are already in an LR").ToChat(executor);
      return;
    }

    if (!lastRequestManager.InitiateLastRequest(executor, player, (LRType)type))
      info.ReplyToCommand(
        "An error occurred while initiating the last request. Please try again later.");
  }
}
