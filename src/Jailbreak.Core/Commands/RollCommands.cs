using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Services.Stubs;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Roll command for the warden.
/// Migrated from Jailbreak.Warden.Commands.RollCommandBehavior.
/// </summary>
public class RollCommands {
  private readonly IWardenService warden;
  private readonly IWardenCmdRollLocale notifications;
  private readonly IWardenLocale wardenLocale;
  private readonly IGenericCmdLocale generics;
  private readonly Random rng = new();

  public RollCommands(IWardenService warden,
    IWardenCmdRollLocale notifications, IWardenLocale wardenLocale,
    IGenericCmdLocale generics) {
    this.warden = warden;
    this.notifications = notifications;
    this.wardenLocale = wardenLocale;
    this.generics = generics;
  }

  [ConsoleCommand("css_roll",
    "Roll a number between min and max. If no min and max are provided, it will default to 0 and 10.")]
  [CommandHelper(1, "[min] [max]", CommandUsage.CLIENT_ONLY)]
  public void Command_Toggle(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.IsWarden(player)) {
      wardenLocale.NotWarden.ToChat(player);
      return;
    }

    var min = 0;
    var max = 10;

    if (command.ArgCount == 3) {
      if (!int.TryParse(command.GetArg(1), out min)) {
        generics.InvalidParameter(command.GetArg(1), "number");
        return;
      }

      if (!int.TryParse(command.GetArg(2), out max)) {
        generics.InvalidParameter(command.GetArg(2), "number");
        return;
      }
    }

    notifications.Roll(rng.Next(min, max)).ToAllChat();
  }
}
