using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Fun.Locale;

/// <summary>
/// Global special day messages (not per-day instance).
/// </summary>
public interface IFunLocale {
  IView SpecialDayRunning(string name);
  IView InvalidSpecialDay(string name);
  IView SpecialDayCooldown(int rounds);
  IView TooLateForSpecialDay(int maxTime);
  IView CannotCallDay(string reason);
  IView NotWarden { get; }
}
