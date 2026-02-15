using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Services.Mute;
using Jailbreak.Core.Locale;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Peace command for the warden.
/// Migrated from Jailbreak.Warden.Commands.PeaceCommandsBehavior.
/// </summary>
public class PeaceCommands {
  private readonly IWardenService warden;
  private readonly IMuteService mute;
  private readonly ICoreLocale locale;

  public PeaceCommands(IWardenService warden, IMuteService mute,
    ICoreLocale locale) {
    this.warden = warden;
    this.mute = mute;
    this.locale = locale;
  }

  [ConsoleCommand("css_peace",
    "Invokes a peace period where only the warden can talk")]
  public void Command_Peace(CCSPlayerController? executor, CommandInfo info) {
    if (mute.IsPeaceEnabled()) {
      if (executor != null) locale.PeaceActive.ToChat(executor);
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
      locale.NotWarden.ToChat(executor);
      return;
    }

    if (DateTime.Now - mute.GetLastPeace() < TimeSpan.FromSeconds(60)) {
      locale.CommandOnCooldown(mute.GetLastPeace().AddSeconds(60))
       .ToChat(executor);
      return;
    }

    mute.PeaceMute(fromWarden ? MuteReason.WARDEN_INVOKED : MuteReason.ADMIN);
  }
}
