using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Fun.Locale;

namespace Jailbreak.Fun.Services.SpecialDay;

public abstract class AbstractCellRestrictedDay : AbstractZoneRestrictedDay {
  protected AbstractCellRestrictedDay(BasePlugin plugin,
    IServiceProvider provider,
    CsTeam restrictedTeam = CsTeam.Terrorist) : base(plugin, provider,
    restrictedTeam) { }

  public override IView ZoneReminder => CellReminder;

  public virtual IView CellReminder
    => this is ISpecialDayMessageProvider messaged ?
      new SimpleView {
        SDLocale.PREFIX,
        $"{ChatColors.Grey}Today is {ChatColors.White}{messaged.Locale.Name}{ChatColors.Grey}, so stay in cells!"
      } :
      new SimpleView { SDLocale.PREFIX, ChatColors.Grey + "Stay in cells!" };
}
