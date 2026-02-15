using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Fun.Locale;

public class WardayInstanceLocale() : TeamDayLocale("Warday",
  "CTs pick a spot to hold. Ts must try to break through!",
  "After a while, guards will receive a boost to hunt down remaining prisoners.") {
  public IView ExpandIn(int seconds) {
    return new SimpleView {
      SDLocale.PREFIX,
      "CTs will expand in",
      seconds,
      "second" + (seconds == 1 ? "" : "s") + "!"
    };
  }

  public IView ExpandNow
    => new SimpleView { SDLocale.PREFIX, "CTs are now expanding! Hunt them down!" };
}
