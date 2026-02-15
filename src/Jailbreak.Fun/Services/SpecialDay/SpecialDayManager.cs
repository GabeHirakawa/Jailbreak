using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Utils;
using Jailbreak.Fun.Enums;
using Jailbreak.Fun.Locale;
using Jailbreak.Fun.Utils;

namespace Jailbreak.Fun.Services.SpecialDay;

public class SpecialDayManager(SpecialDayFactory factory, IFunLocale locale) {
  public static readonly FakeConVar<int> CV_MAX_ELAPSED_TIME = new(
    "css_jb_sd_max_elapsed_time",
    "Max time elapsed in a round to be able to call a special day", 30);

  public static readonly FakeConVar<int> CV_ROUNDS_BETWEEN_SD = new(
    "css_jb_sd_round_cooldown", "Rounds between special days", 5);

  public bool IsSDRunning { get; set; }
  public AbstractSpecialDay? CurrentSD { get; private set; }
  public int RoundsSinceLastSD { get; set; }

  public bool CanStartSpecialDay(SDType type, CCSPlayerController? player,
    bool print = true) {
    // TODO: Re-enable warden check when IWardenService is migrated
    // var warden = provider.GetRequiredService<IWardenService>();
    if (!AdminManager.PlayerHasPermissions(player, "@css/rcon")) {
      // TODO: Check warden status
      // if (!warden.IsWarden(player) || RoundUtil.IsWarmup()) {
      //   if (print) locale.NotWarden.ToChat(player);
      //   return false;
      // }

      if (IsSDRunning) {
        if (CurrentSD is ISpecialDayMessageProvider messaged) {
          if (print)
            locale.SpecialDayRunning(messaged.Locale.Name).ToChat(player);
          return false;
        }

        if (print)
          locale.SpecialDayRunning(CurrentSD?.Type.ToString() ?? "Unknown")
           .ToChat(player);
        return false;
      }

      var roundsToNext = RoundsSinceLastSD - CV_ROUNDS_BETWEEN_SD.Value;
      if (roundsToNext < 0) {
        if (print)
          locale.SpecialDayCooldown(Math.Abs(roundsToNext)).ToChat(player);
        return false;
      }

      if (RoundUtil.GetTimeElapsed() > CV_MAX_ELAPSED_TIME.Value) {
        if (print)
          locale.TooLateForSpecialDay(CV_MAX_ELAPSED_TIME.Value).ToChat(player);
        return false;
      }
    }

    var denyReason = type.CanCall(player);

    if (denyReason == null
      || AdminManager.PlayerHasPermissions(player, "@css/root"))
      return true;
    if (print) locale.CannotCallDay(denyReason).ToChat(player);
    return false;
  }

  public bool InitiateSpecialDay(SDType type) {
    // TODO: Push stats when MStatsShared is migrated
    RoundsSinceLastSD = 0;
    CurrentSD         = factory.CreateSpecialDay(type);
    IsSDRunning       = true;
    if (CurrentSD is ISpecialDayMessageProvider messaged)
      messaged.Locale.SpecialDayStart.ToAllChat();

    // TODO: assignGangColors() when GangsAPI is migrated
    CurrentSD.Setup();
    return true;
  }

  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    if (RoundUtil.IsWarmup()) return HookResult.Continue;
    RoundsSinceLastSD++;
    return HookResult.Continue;
  }

  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    if (!IsSDRunning || CurrentSD == null) return HookResult.Continue;
    IsSDRunning = false;
    if (CurrentSD is ISpecialDayMessageProvider messaged)
      messaged.Locale.SpecialDayEnd.ToAllChat();
    CurrentSD = null;
    return HookResult.Continue;
  }
}
