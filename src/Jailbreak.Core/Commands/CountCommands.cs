using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Contracts.Utils;
using Jailbreak.Core.Services.Stubs;
using Jailbreak.Core.Services.Warden;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Count prisoners in marker command.
/// Migrated from Jailbreak.Warden.Commands.CountCommandsBehavior.
/// </summary>
public class CountCommands {
  public static readonly FakeConVar<int> CV_COUNT_COMMAND_COOLDOWN = new(
    "css_jb_warden_count_cooldown",
    "Minimum seconds warden must wait before being able to count prisoners in marker.",
    30, customValidators: new RangeValidator<int>(0, 300));

  private readonly IWardenService warden;
  private readonly IWardenLocale msg;
  private readonly IWardenCmdCountLocale locale;
  private readonly IMarkerService markers;

  public CountCommands(IWardenService warden, IWardenLocale msg,
    IWardenCmdCountLocale locale, IMarkerService markers) {
    this.warden = warden;
    this.msg = msg;
    this.locale = locale;
    this.markers = markers;
  }

  [ConsoleCommand("css_count", "Counts the prisoners in marker")]
  public void Command_Count(CCSPlayerController? executor, CommandInfo info) {
    if (executor == null) return;
    if (!warden.IsWarden(executor)) {
      msg.NotWarden.ToChat(executor);
      return;
    }

    var timeTillCount =
      CV_COUNT_COMMAND_COOLDOWN.Value - RoundUtil.GetTimeElapsed();
    if (timeTillCount > 0) {
      locale.CannotCountYet(timeTillCount).ToChat(executor);
      return;
    }

    if (markers.MarkerPosition == null) {
      locale.NoMarkerSet.ToChat(executor);
      return;
    }

    var prisoners = PlayerUtil.FromTeam(CsTeam.Terrorist)
     .Count(markers.InMarker);

    locale.PrisonersInMarker(prisoners).ToChat(executor);
  }
}
