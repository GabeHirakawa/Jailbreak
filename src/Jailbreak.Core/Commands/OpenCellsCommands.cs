using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Locale;
using Jailbreak.Core.Services.Stubs;
using Jailbreak.Core.Services.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Open cells command for the warden.
/// Migrated from Jailbreak.Warden.Commands.WardenOpenCommandsBehavior.
/// </summary>
public class OpenCellsCommands : IWardenOpenCommand {
  public static readonly FakeConVar<int> CV_OPEN_COMMAND_COOLDOWN = new(
    "css_jb_warden_open_cooldown",
    "Minimum seconds warden must wait before being able to open the cells.", 25,
    customValidators: new RangeValidator<int>(0, 300));

  private readonly IWardenService warden;
  private readonly ICoreLocale locale;
  private readonly IZoneManager? zoneManager;

  public bool OpenedCells { get; set; }

  public OpenCellsCommands(IWardenService warden, ICoreLocale locale,
    IServiceProvider provider) {
    this.warden = warden;
    this.locale = locale;
    zoneManager = provider.GetService<IZoneManager>();
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    OpenedCells = false;
    return HookResult.Continue;
  }

  [ConsoleCommand("css_open", "Opens the cell doors")]
  [ConsoleCommand("css_o", "Opens the cell doors")]
  public void Command_Open(CCSPlayerController? executor, CommandInfo info) {
    if (executor != null
      && !AdminManager.PlayerHasPermissions(executor, "@css/cheats")) {
      if (!warden.IsWarden(executor)) {
        locale.NotWarden.ToChat(executor);
        return;
      }

      if (RoundUtil.GetTimeElapsed() < CV_OPEN_COMMAND_COOLDOWN.Value) {
        locale.CannotOpenYet(CV_OPEN_COMMAND_COOLDOWN.Value)
         .ToChat(executor);
        return;
      }

      if (OpenedCells) {
        locale.AlreadyOpened.ToChat(executor);
        return;
      }
    }

    OpenedCells = true;

    if (zoneManager == null) { Services.Stubs.MapUtil.OpenCells(); } else {
      var result = Services.Stubs.MapUtil.OpenCells(zoneManager);
      IView message;
      if (result) {
        if (executor != null && !warden.IsWarden(executor))
          message = locale.CellsOpenedBy(executor);
        else
          message = locale.CellsOpenedBy(null);
      } else { message = locale.OpeningFailed; }

      message.ToAllChat();
    }
  }
}
