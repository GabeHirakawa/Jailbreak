using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Fun.Locale;

public interface ISDInstanceLocale {
  string Name { get; }
  string[] Description { get; }

  IView SpecialDayStart { get; }

  IView SpecialDayEnd { get; }

  IView BeginsIn(int seconds);
}
