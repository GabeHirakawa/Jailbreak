using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Formatting.Objects;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Services.Stubs;

namespace Jailbreak.Core.Commands;

/// <summary>
/// Countdown command for the warden.
/// Migrated from CountdownCommandBehavior.
/// </summary>
public class CountdownCommands {
  public static readonly FakeConVar<int> CV_WARDEN_MAX_COUNTDOWN =
    new("css_jb_warden_countdown_max",
      "The maximum duration for a countdown", 15);

  public static readonly FakeConVar<int> CV_WARDEN_MIN_COUNTDOWN =
    new("css_jb_warden_countdown_min",
      "The minimum duration for a countdown", 3);

  public static readonly FakeConVar<int> CV_WARDEN_DEFAULT_COUNTDOWN =
    new("css_jb_warden_countdown_default",
      "The default duration for a countdown", 5);

  private readonly IWardenService warden;
  private readonly IMuteService mute;
  private readonly IWardenLocale wardenLocale;
  private readonly IGenericCmdLocale generics;
  private DateTime lastCountdown = DateTime.MinValue;
  private int countdownDuration;

  public CountdownCommands(IWardenService warden, IMuteService mute,
    IWardenLocale wardenLocale, IGenericCmdLocale generics) {
    this.warden = warden;
    this.mute = mute;
    this.wardenLocale = wardenLocale;
    this.generics = generics;
  }

  [ConsoleCommand("css_countdown", "Invokes a countdown")]
  public void Command_Countdown(CCSPlayerController? executor, CommandInfo command) {
    countdownDuration = CV_WARDEN_DEFAULT_COUNTDOWN.Value;

    if (command.ArgCount == 2) {
      if (!int.TryParse(command.GetArg(1), out countdownDuration)) {
        generics.InvalidParameter(command.GetArg(1), "number");
        command.ReplyToCommand("Expected a number parameter.");
        return;
      }

      if (countdownDuration <= 0) {
        generics.InvalidParameter(command.GetArg(1), "number greater than 0");
        command.ReplyToCommand("Expected a number greater than 0.");
        return;
      }
    }

    if (countdownDuration < CV_WARDEN_MIN_COUNTDOWN.Value) {
      generics.InvalidParameter(command.GetArg(1),
        $"number greater than or equal to {CV_WARDEN_MIN_COUNTDOWN.Value}");
      command.ReplyToCommand($"Expected a number greater than or equal to {CV_WARDEN_MIN_COUNTDOWN.Value}");
      return;
    }

    if (countdownDuration > CV_WARDEN_MAX_COUNTDOWN.Value) {
      generics.InvalidParameter(command.GetArg(1),
        $"number less than or equal to {CV_WARDEN_MAX_COUNTDOWN.Value}");
      command.ReplyToCommand($"Expected a number less than or equal to {CV_WARDEN_MAX_COUNTDOWN.Value}");
      return;
    }

    bool success = PermCheckAndEnactPeace(executor);
    if (!success) return;

    StartCountDown(countdownDuration);

    for (int i = countdownDuration; i > 0; --i) {
      int current = i;

      if (current <= 5 || current % 5 == 0) {
        Server.RunOnTick(Server.TickCount + (64 * (countdownDuration - current)),
          () => PrintCountdownToPlayers(current));
      }
    }
    Server.RunOnTick(Server.TickCount + (64 * countdownDuration), () => PrintGoToPlayers());
  }

  private static readonly FormatObject PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}Countdown>") {
      Plain = false, Panorama = false, Chat = true
    };

  private void StartCountDown(int duration) {
    new SimpleView { PREFIX, $"A {duration} second countdown has begun!" }.ToAllChat();
  }

  private void PrintCountdownToPlayers(int seconds) {
    new SimpleView { PREFIX, seconds.ToString() }.ToAllChat();
  }

  private void PrintGoToPlayers() {
    new SimpleView { PREFIX, "GO! GO! GO!" }.ToAllChat();
  }

  private bool PermCheckAndEnactPeace(CCSPlayerController? executor) {
    var fromWarden = executor != null && warden.IsWarden(executor);

    if (executor == null
      || AdminManager.PlayerHasPermissions(executor, "@css/cheats")) {
      mute.PeaceMute(MuteReason.ADMIN);
      lastCountdown = DateTime.Now;
      return true;
    }

    if (!fromWarden
      && AdminManager.PlayerHasPermissions(executor, "@css/chat")) {
      mute.PeaceMute(MuteReason.ADMIN);
      lastCountdown = DateTime.Now;
      return true;
    }

    if (DateTime.Now - lastCountdown < TimeSpan.FromSeconds(60)) {
      generics.CommandOnCooldown(lastCountdown.AddSeconds(60))
       .ToChat(executor);
      return false;
    }

    if (fromWarden) {
      mute.PeaceMute(fromWarden ? MuteReason.WARDEN_INVOKED : MuteReason.ADMIN);
      lastCountdown = DateTime.Now;
      return true;
    } else {
      wardenLocale.NotWarden.ToChat(executor);
    }
    return false;
  }
}
