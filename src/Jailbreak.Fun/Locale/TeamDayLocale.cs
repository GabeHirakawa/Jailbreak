using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Utils;

namespace Jailbreak.Fun.Locale;

public class TeamDayLocale(string name, params string[] description)
  : ISDInstanceLocale {
  public string Name => name;

  public string[] Description => description;

  public virtual IView SpecialDayStart => GenerateStartMessage();

  public virtual IView SpecialDayEnd {
    get {
      var winner = PlayerUtil.GetAlive().FirstOrDefault()?.Team
        ?? CsTeam.Spectator;
      return new SimpleView {
        SDLocale.PREFIX,
        Name,
        "ended.",
        winner,
        "won!"
      };
    }
  }

  public virtual IView BeginsIn(int seconds) {
    return seconds == 0 ?
      new SimpleView { SDLocale.PREFIX, Name, "begins now!" } :
      new SimpleView {
        SDLocale.PREFIX,
        Name,
        "begins in",
        seconds,
        "seconds."
      };
  }

  public IView GenerateStartMessage() {
    var result = new SimpleView {
      SDLocale.PREFIX, { "Today is a" + (Name[0].IsVowel() ? "n" : ""), Name, "day!" }
    };

    if (description.Length == 0) return result;

    result.Add(description[0]);

    for (var i = 1; i < description.Length; i++) {
      result.Add(SimpleView.NEWLINE);
      result.Add(SDLocale.PREFIX);
      result.Add(description[i]);
    }

    return result;
  }
}
