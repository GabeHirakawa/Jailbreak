using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Locale;
using Jailbreak.Core.Services.Warden;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Special treatment toggle command for the warden.
/// Migrated from Jailbreak.Warden.Commands.SpecialTreatmentCommandsBehavior.
/// </summary>
public class SpecialTreatmentCommands {
  private readonly IWardenService warden;
  private readonly ISpecialTreatmentService specialTreatment;
  private readonly ICoreLocale locale;

  public SpecialTreatmentCommands(IWardenService warden,
    ISpecialTreatmentService specialTreatment, ICoreLocale locale) {
    this.warden = warden;
    this.specialTreatment = specialTreatment;
    this.locale = locale;
  }

  [ConsoleCommand("css_treat",
    "Grant or revoke special treatment from a player")]
  [ConsoleCommand("css_st", "Grant or revoke special treatment from a player")]
  [CommandHelper(0, "<target>", CommandUsage.CLIENT_ONLY)]
  public void Command_Toggle(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.IsWarden(player)) {
      locale.NotWarden.ToChat(player).ToConsole(player);
      return;
    }

    if (command.ArgCount == 1)
      // TODO: Pop up menu of prisoners to toggle ST for
      return;

    var targets = command.GetArgTargetResult(1);
    var eligible = targets
     .Where(p => p is { Team: CsTeam.Terrorist, PawnIsAlive: true })
     .ToList();

    if (eligible.Count == 0) {
      locale.PlayerNotFound(command.GetArg(1))
       .ToChat(player)
       .ToConsole(player);
      return;
    }

    if (eligible.Count != 1) {
      locale.PlayerFoundMultiple(command.GetArg(1))
       .ToChat(player)
       .ToConsole(player);
      return;
    }

    var special = eligible.First();
    // TODO: Re-enable when stats is migrated
    // API.Stats?.PushStat(...)

    specialTreatment.SetSpecialTreatment(special,
      !specialTreatment.IsSpecialTreatment(special));
  }
}
