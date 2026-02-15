using CounterStrikeSharp.API.Core;
using Jailbreak.Fun.Commands;
using Jailbreak.Fun.Locale;
using Jailbreak.Fun.Services.Rainbow;
using Jailbreak.Fun.Services.RTD;
using Jailbreak.Fun.Services.SpecialDay;

namespace Jailbreak.Fun;

public class FunPlugin : BasePlugin {
  public override string ModuleName => "Jailbreak Fun";
  public override string ModuleVersion => "2.0.0";
  public override string ModuleAuthor => "EdgeGamers Development";

  private SpecialDayFactory factory = null!;
  private SpecialDayManager manager = null!;
  private SpecialDayCommands sdCommands = null!;

  private RewardGenerator rewardGenerator = null!;
  private RTDRewarder rtdRewarder = null!;
  private RTDCommands rtdCommands = null!;
  private RainbowService rainbow = null!;

  public override void Load(bool hotReload) {
    var sdLocale  = new FunLocale();
    var rtdLocale = new RTDLocale();
    var provider  = new FunServiceProvider(this);

    // SpecialDay system
    factory = new SpecialDayFactory(provider);
    factory.Start(this);
    manager    = new SpecialDayManager(factory, sdLocale);
    sdCommands = new SpecialDayCommands(factory, sdLocale, manager);
    sdCommands.Start(this);

    RegisterFakeConVars(typeof(SpecialDayManager));
    RegisterEventHandler<EventRoundStart>(manager.OnRoundStart);
    RegisterEventHandler<EventRoundEnd>(manager.OnRoundEnd);

    AddCommand("css_sd", "Start a special day", sdCommands.OnSpecialDayCommand);
    AddCommand("css_specialday", "Start a special day",
      sdCommands.OnSpecialDayCommand);
    AddCommand("css_startday", "Start a special day",
      sdCommands.OnSpecialDayCommand);

    // RTD system
    rewardGenerator = new RewardGenerator();
    rewardGenerator.Start(this);
    rtdRewarder = new RTDRewarder();
    rtdCommands = new RTDCommands(rtdRewarder, rewardGenerator, rtdLocale);

    RegisterFakeConVars(typeof(RTDCommands));
    RegisterEventHandler<EventPlayerSpawn>(rtdRewarder.OnSpawn);
    RegisterEventHandler<EventRoundEnd>(rtdCommands.OnRoundEnd);
    RegisterEventHandler<EventRoundStart>(rtdCommands.OnRoundStart);

    AddCommand("css_rtd", "Roll the dice!", rtdCommands.OnRTDCommand);

    // Rainbow system
    rainbow = new RainbowService();
    rainbow.Start(this);
    RegisterEventHandler<EventRoundEnd>(rainbow.OnRoundEnd);
  }

  public override void Unload(bool hotReload) { }
}

/// <summary>
///   Minimal service provider for the Fun plugin.
///   Special days only need the plugin and manager; we avoid full DI.
/// </summary>
internal class FunServiceProvider(FunPlugin plugin) : IServiceProvider {
  public object? GetService(Type serviceType) {
    if (serviceType == typeof(BasePlugin)) return plugin;
    return null;
  }
}
