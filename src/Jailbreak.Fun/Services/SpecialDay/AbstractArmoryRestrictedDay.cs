using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Fun.Locale;

namespace Jailbreak.Fun.Services.SpecialDay;

public abstract class AbstractArmoryRestrictedDay : AbstractZoneRestrictedDay {
  protected AbstractArmoryRestrictedDay(BasePlugin plugin,
    IServiceProvider provider,
    CsTeam restrictedTeam = CsTeam.Terrorist) : base(plugin, provider,
    restrictedTeam) { }

  public override IView ZoneReminder => ArmoryReminder;

  public virtual IView ArmoryReminder
    => this is ISpecialDayMessageProvider messaged ?
      new SimpleView {
        SDLocale.PREFIX,
        $"{ChatColors.Grey}Today is {ChatColors.White}{messaged.Locale.Name}{ChatColors.Grey}, so stay in armory!"
      } :
      new SimpleView { SDLocale.PREFIX, ChatColors.Grey + "Stay in armory!" };
}
