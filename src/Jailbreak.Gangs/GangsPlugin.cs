using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using GangsAPI;
using GangsAPI.Services;
using Jailbreak.Gangs.Services.Perks;

namespace Jailbreak.Gangs;

public class GangsPlugin : BasePlugin {
  public override string ModuleName => "Jailbreak Gangs";
  public override string ModuleVersion => "2.0.0";
  public override string ModuleAuthor => "EdgeGamers Development";

  public override void Load(bool hotReload) {
    // TODO: Access GangsAPI services via capability when available
    // The old code used API.Gangs?.Services to get the IServiceProvider
    // For now, perk registration is deferred until GangsAPI is loaded
    RegisterListener<Listeners.OnMapStart>(_ => TryBootstrapPerks());
  }

  private bool bootstrapped;

  private void TryBootstrapPerks() {
    if (bootstrapped) return;
    // TODO: Get GangsAPI service provider
    // var services = /* GangsAPI capability */;
    // if (services == null) return;
    // bootstrapped = true;
    //
    // Register perks:
    // new BombIconCommand(services).Start();
    // services.GetRequiredService<IPerkManager>().Perks.Add(new BombPerk(services));
    //
    // new WardenIconCommand(services).Start();
    // services.GetRequiredService<IPerkManager>().Perks.Add(new WardenIconPerk(services));
    //
    // new SpecialIconCommand(services).Start();
    // services.GetRequiredService<IPerkManager>().Perks.Add(new SpecialIconPerk(services));
    //
    // new WardenColorCommand(services).Start();
    // services.GetRequiredService<IPerkManager>().Perks.Add(new WardenPaintColorPerk(services));
    //
    // new SDColorCommand(services).Start();
    // services.GetRequiredService<IPerkManager>().Perks.Add(new SDColorPerk(services));
    //
    // new LRColorCommand(services).Start();
    // services.GetRequiredService<IPerkManager>().Perks.Add(new LRColorPerk(services));
    //
    // services.GetRequiredService<IPerkManager>().Perks.Add(new CellsPerk(services));
    Server.PrintToConsole("[Jailbreak.Gangs] Waiting for GangsAPI capability...");
  }

  public override void Unload(bool hotReload) { }
}
