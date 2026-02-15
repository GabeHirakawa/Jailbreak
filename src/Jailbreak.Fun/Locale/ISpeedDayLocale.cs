using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Fun.Locale;

public interface ISpeedDayLocale : ISDInstanceLocale {
  IView NoneEliminated { get; }

  IView StayStillToSpeedup { get; }
  IView RunnerAssigned(CCSPlayerController player);

  IView YouAreRunner(int seconds);

  IView RunnerLeftAndReassigned(CCSPlayerController player);
  IView RunnerAFKAndReassigned(CCSPlayerController player);

  IView RuntimeLeft(int seconds);

  IView BeginRound(int round, int eliminations, int seconds);

  IView BestTime(CCSPlayerController player, float time);

  IView PlayerTime(CCSPlayerController player, int place, float time);

  IView ImpossibleLocation(CsTeam team, CCSPlayerController player);

  IView PlayerWon(CCSPlayerController player);

  IView PlayerEliminated(CCSPlayerController player);
}
