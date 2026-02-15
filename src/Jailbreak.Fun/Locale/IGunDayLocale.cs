using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Fun.Locale;

public interface IGunDayLocale : ISDInstanceLocale {
  IView DemotedDueToSuicide { get; }
  IView DemotedDueToKnife { get; }

  IView PromotedTo(string weapon, int weaponsLeft);
  IView PlayerOnLastPromotion(CCSPlayerController player);
  IView PlayerWon(CCSPlayerController player);
}
