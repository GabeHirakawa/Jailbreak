using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Fun.Locale;

public class SpeedrunDayLocale() : SoloDayLocale("Speedrunners",
    $"Follow the {ChatColors.Blue}blue{ChatColors.Default} player!",
    "They will run to a spot on the map.",
    $"Each round, the {ChatColors.Red}slowest players{ChatColors.Grey} to reach the target will be eliminated."),
  ISpeedDayLocale {
  public IView NoneEliminated
    => new SimpleView { SDLocale.PREFIX, "No one was eliminated this round!" };

  public IView YouAreRunner(int seconds) {
    return new SimpleView {
      { SDLocale.PREFIX, "You are the speedrunner!" },
      SimpleView.NEWLINE, {
        SDLocale.PREFIX, "You have", seconds, "seconds to run to a spot to set the goal."
      }
    };
  }

  public IView BeginRound(int round, int eliminationCount, int seconds) {
    if (eliminationCount == 1)
      return new SimpleView {
        {
          SDLocale.PREFIX,
          $"Round{ChatColors.Yellow}#{round}{ChatColors.Grey} begins! The slowest",
          "player to reach the goal will be eliminated!"
        },
        SimpleView.NEWLINE,
        { SDLocale.PREFIX, "You have", seconds, "seconds to reach the goal!" }
      };

    return new SimpleView {
      {
        SDLocale.PREFIX,
        $"Round {ChatColors.Yellow}#{round}{ChatColors.Grey} begins! The slowest",
        eliminationCount, "players to reach the goal will be eliminated!"
      },
      SimpleView.NEWLINE,
      { SDLocale.PREFIX, "You have", seconds, "seconds to reach the goal." }
    };
  }

  public IView RuntimeLeft(int seconds) {
    return new SimpleView {
      SDLocale.PREFIX, "You have", seconds, "seconds left to run to a spot!"
    };
  }

  public IView RunnerAssigned(CCSPlayerController player) {
    return new SimpleView {
      SDLocale.PREFIX, player, "is the speedrunner! Follow them closely!"
    };
  }

  public IView RunnerLeftAndReassigned(CCSPlayerController player) {
    return new SimpleView {
      SDLocale.PREFIX,
      "The original speedrunner left, so",
      player,
      "is now the speedrunner."
    };
  }

  public IView RunnerAFKAndReassigned(CCSPlayerController player) {
    return new SimpleView {
      SDLocale.PREFIX,
      "The original speedrunner isn't moving, so",
      player,
      "is now the speedrunner."
    };
  }

  public IView PlayerTime(CCSPlayerController player, int position,
    float time) {
    var place = position switch {
      1 => ChatColors.Green + "FIRST",
      2 => ChatColors.LightYellow + "Second",
      3 => ChatColors.BlueGrey + "3rd",
      _ => ChatColors.Grey + "" + position + "th"
    };
    if (time < 0)
      return new SimpleView {
        SDLocale.PREFIX,
        player,
        "finished in",
        -time,
        "seconds.",
        place,
        "place" + (position == 1 ? "!" : ".")
      };

    return new SimpleView {
      SDLocale.PREFIX,
      player,
      "was",
      time,
      "units away from the goal,",
      place,
      "place."
    };
  }

  public IView PlayerEliminated(CCSPlayerController player) {
    return new SimpleView { SDLocale.PREFIX, player, "was eliminated!" };
  }

  public IView StayStillToSpeedup
    => new SimpleView { SDLocale.PREFIX, "Stay still to start the round sooner..." };

  public IView PlayerWon(CCSPlayerController player) {
    return new SimpleView { SDLocale.PREFIX, player, "won the game!" };
  }

  public IView BestTime(CCSPlayerController player, float time) {
    return new SimpleView {
      SDLocale.PREFIX,
      player,
      "beat the best time with",
      time,
      $"seconds! {ChatColors.Green}FIRST PLACE{ChatColors.Default}!"
    };
  }

  public IView ImpossibleLocation(CsTeam team, CCSPlayerController player) {
    return new SimpleView {
      {
        SDLocale.PREFIX, "No one on", team,
        "reached the goal. Eliminating one player on each time."
      },
      SimpleView.NEWLINE,
      { SDLocale.PREFIX, "Randomly selected the path from", player, "." }
    };
  }
}
