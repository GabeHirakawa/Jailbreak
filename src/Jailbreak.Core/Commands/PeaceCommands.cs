using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Services.Mute;
using Jailbreak.Core.Services.Stubs;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Peace command for the warden.
/// Migrated from Jailbreak.Warden.Commands.PeaceCommandsBehavior.
/// </summary>
public class PeaceCommands {
  private readonly IWardenService warden;
  private readonly IMuteService mute;
  private readonly IWardenPeaceLocale peaceLocale;
  private readonly IWardenLocale wardenLocale;
  private readonly IGenericCmdLocale generics;

  public PeaceCommands(IWardenService warden, IMuteService mute,
    IWardenPeaceLocale peaceLocale, IWardenLocale wardenLocale,
    IGenericCmdLocale generics) {
    this.warden = warden;
    this.mute = mute;
    this.peaceLocale = peaceLocale;
    this.wardenLocale = wardenLocale;
    this.generics = generics;
  }

  [ConsoleCommand("css_peace",
    "Invokes a peace period where only the warden can talk")]
  public void Command_Peace(CCSPlayerController? executor, CommandInfo info) {
    if (mute.IsPeaceEnabled()) {
      if (executor != null) peaceLocale.PeaceActive.ToChat(executor);
      return;
    }

    var fromWarden = executor != null && warden.IsWarden(executor);

    if (executor == null
      || AdminManager.PlayerHasPermissions(executor, "@css/cheats")) {
      mute.PeaceMute(fromWarden ? MuteReason.WARDEN_INVOKED : MuteReason.ADMIN);
      return;
    }

    if (!warden.IsWarden(executor)
      && !AdminManager.PlayerHasPermissions(executor, "@css/chat")) {
      wardenLocale.NotWarden.ToChat(executor);
      return;
    }

    if (DateTime.Now - mute.GetLastPeace() < TimeSpan.FromSeconds(60)) {
      generics.CommandOnCooldown(mute.GetLastPeace().AddSeconds(60))
       .ToChat(executor);
      return;
    }

    mute.PeaceMute(fromWarden ? MuteReason.WARDEN_INVOKED : MuteReason.ADMIN);
  }
}
