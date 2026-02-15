using CounterStrikeSharp.API.Core;
using Jailbreak.Fun.Commands;
using Jailbreak.Fun.Locale;
using Jailbreak.Fun.Services.SpecialDay;

namespace Jailbreak.Fun;

public class FunPlugin : BasePlugin {
  public override string ModuleName => "Jailbreak Fun";
  public override string ModuleVersion => "2.0.0";
  public override string ModuleAuthor => "EdgeGamers Development";

  private SpecialDayFactory factory = null!;
  private SpecialDayManager manager = null!;
  private SpecialDayCommands commands = null!;

  public override void Load(bool hotReload) {
    var locale = new FunLocale();
    var provider = new FunServiceProvider(this);

    factory  = new SpecialDayFactory(provider);
    factory.Start(this);

    manager  = new SpecialDayManager(factory, locale);
    commands = new SpecialDayCommands(factory, locale, manager);
    commands.Start(this);

    RegisterFakeConVars(typeof(SpecialDayManager));

    RegisterEventHandler<EventRoundStart>(manager.OnRoundStart);
    RegisterEventHandler<EventRoundEnd>(manager.OnRoundEnd);

    AddCommand("css_sd", "Start a special day", commands.OnSpecialDayCommand);
    AddCommand("css_specialday", "Start a special day",
      commands.OnSpecialDayCommand);
    AddCommand("css_startday", "Start a special day",
      commands.OnSpecialDayCommand);
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
