using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Fun.Locale;

public class FunLocale : IFunLocale {
  public IView SpecialDayRunning(string name) {
    return new SimpleView {
      SDLocale.PREFIX,
      "The current day is",
      ChatColors.BlueGrey + name + ChatColors.Grey + "."
    };
  }

  public IView InvalidSpecialDay(string name) {
    return new SimpleView {
      SDLocale.PREFIX,
      ChatColors.Red + name,
      ChatColors.Grey + "is not a valid special day."
    };
  }

  public IView SpecialDayCooldown(int rounds) {
    return new SimpleView {
      SDLocale.PREFIX,
      "You must wait",
      rounds,
      "more round" + (rounds == 1 ? "" : "s")
      + " before starting a special day."
    };
  }

  public IView TooLateForSpecialDay(int maxTime) {
    return new SimpleView {
      SDLocale.PREFIX,
      "You must start a special day within",
      maxTime,
      "second" + (maxTime == 1 ? "" : "s") + " of round start."
    };
  }

  public IView CannotCallDay(string reason) {
    return new SimpleView {
      SDLocale.PREFIX, "You cannot call this special day:", ChatColors.Red + reason
    };
  }

  public IView NotWarden
    => new SimpleView {
      SDLocale.PREFIX, "You must be the warden to start a special day."
    };
}
