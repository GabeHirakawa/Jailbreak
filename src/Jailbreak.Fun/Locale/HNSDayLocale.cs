using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Fun.Locale;

public class HNSDayLocale() : TeamDayLocale("Hide and Seek",
  "CTs must hide while the Ts seek!", "Ts have increased HP!") {
  public IView StayInArmory
    => new SimpleView { SDLocale.PREFIX, "Today is", Name, ", stay in the armory!" };

  public override IView BeginsIn(int seconds) {
    if (seconds == 0)
      return new SimpleView { SDLocale.PREFIX, "Ready or not, here they come!" };
    return new SimpleView {
      SDLocale.PREFIX,
      Name,
      "begins in",
      seconds,
      "second" + (seconds == 1 ? "" : "s") + "."
    };
  }

  public IView DamageWarning(int seconds) {
    return new SimpleView {
      SDLocale.PREFIX,
      "You will be vulnerable to damage in",
      seconds,
      "second" + (seconds == 1 ? "" : "s") + "."
    };
  }
}
