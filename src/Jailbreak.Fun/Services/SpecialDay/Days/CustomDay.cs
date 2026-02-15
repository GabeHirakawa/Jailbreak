using CounterStrikeSharp.API.Core;
using Jailbreak.Fun.Enums;
using Jailbreak.Fun.Locale;

namespace Jailbreak.Fun.Services.SpecialDay.Days;

public class CustomDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.CUSTOM;

  public override SpecialDaySettings Settings
    => new() {
      StripToKnife     = false,
      AllowLastRequests = true,
      AllowRebels      = true,
      OpenCells        = false,
      RespawnPlayers   = false
    };

  public ISDInstanceLocale Locale
    => new TeamDayLocale("Custom",
      "Listen to the Warden's orders. Anything goes!");

  public override void Setup() {
    Timers[3] += Execute;
    base.Setup();
  }
}
