using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Objects;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Services.Rebel;
using Jailbreak.Core.Services.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Generates rich player tags based on the player's current role (warden, CT, rebel, ST, T).
/// Uses lazy-loaded dependencies to avoid circular dependency issues.
/// Migrated from Jailbreak.Logs.Tags.PlayerTagHelper.
/// </summary>
public class PlayerTagHelper : IRichPlayerTag {
  private readonly Lazy<IRebelService?> rebelService;
  private readonly Lazy<ISpecialTreatmentService?> stService;
  private readonly Lazy<IWardenService?> wardenService;

  public PlayerTagHelper(IServiceProvider provider) {
    rebelService = new Lazy<IRebelService?>(provider.GetService<IRebelService>);
    stService = new Lazy<ISpecialTreatmentService?>(provider.GetService<ISpecialTreatmentService>);
    wardenService = new Lazy<IWardenService?>(provider.GetService<IWardenService>);
  }

  //  Lazy-load dependencies to avoid loops, since we are a lower-level class.

  public FormatObject Rich(CCSPlayerController player) {
    if (wardenService.Value != null && wardenService.Value.IsWarden(player))
      return new StringFormatObject("(WARDEN)", ChatColors.DarkBlue);
    if (player.Team == CsTeam.CounterTerrorist)
      return new StringFormatObject("(CT)", ChatColors.BlueGrey);
    if (rebelService.Value != null && rebelService.Value.IsRebel(player))
      return new StringFormatObject("(REBEL)", ChatColors.DarkRed);
    if (stService.Value != null && stService.Value.IsSpecialTreatment(player))
      return new StringFormatObject("(ST)", ChatColors.Green);

    return new StringFormatObject("(T)", ChatColors.Yellow);
  }

  public string Plain(CCSPlayerController playerController) {
    return Rich(playerController).ToPlain();
  }
}
