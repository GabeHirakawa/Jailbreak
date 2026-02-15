using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Utils;

namespace Jailbreak.Fun.Services.SpecialDay;

public abstract class AbstractZoneRestrictedDay : AbstractSpecialDay {
  protected CsTeam RestrictedTeam;

  protected AbstractZoneRestrictedDay(BasePlugin plugin,
    IServiceProvider provider,
    CsTeam restrictedTeam = CsTeam.Terrorist) : base(plugin, provider) {
    RestrictedTeam = restrictedTeam;
  }

  public abstract IView ZoneReminder { get; }

  public override void Setup() {
    base.Setup();

    ZoneReminder.ToTeamChat(RestrictedTeam);

    // TODO: Re-enable zone drawing when IBeamShapeFactory is migrated
    // GetZone().Draw(Plugin, Provider.GetRequiredService<IBeamShapeFactory>(),
    //   Color.Firebrick, 55);

    // TODO: Re-enable zone movement restriction when IZoneManager is migrated
    // Without zones, we just print reminder messages periodically
    Plugin.AddTimer(15f, () => {
      ZoneReminder.ToTeamChat(RestrictedTeam);
    }, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT
      | CounterStrikeSharp.API.Modules.Timers.TimerFlags.STOP_ON_MAPCHANGE);
  }

  public override void Execute() {
    base.Execute();
    if (this is ISpecialDayMessageProvider messaged)
      messaged.Locale.BeginsIn(0).ToAllChat();
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    return base.OnEnd(@event, info);
  }
}
