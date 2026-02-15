using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Fun.Locale;

public class FogDayLocale() : SoloDayLocale("Fog War",
  "A heavy fog is creeping in...", "Your visibility will be gone soon.",
  "Fog expands periodically — Stay alert.") {

  public IView FogComingIn() {
    return new SimpleView {
      SDLocale.PREFIX,
      "Fog's rolling in!",
      "Match Starts in 15 seconds."
    };
  }

  public IView FogExpandsIn(int seconds) {
    if (seconds == 0) return new SimpleView { SDLocale.PREFIX, "Fog is expanding." };
    return new SimpleView {
      SDLocale.PREFIX,
      "Fog will start expanding in",
      seconds,
      "second" + (seconds == 1 ? "" : "s")
    };
  }
}
