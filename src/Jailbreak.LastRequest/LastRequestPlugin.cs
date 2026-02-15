using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.LastRequest.Commands;
using Jailbreak.LastRequest.Locale;
using Jailbreak.LastRequest.Services;
using Jailbreak.LastRequest.Utils;

namespace Jailbreak.LastRequest;

public class LastRequestPlugin : BasePlugin {
  public override string ModuleName => "Jailbreak LastRequest";
  public override string ModuleVersion => "2.0.0";
  public override string ModuleAuthor => "EdgeGamers Development";

  private LastRequestManager? manager;
  private LastRequestRebelManager? rebelManager;
  private LastRequestFactory? factory;
  private ILastRequestLocale? locale;

  private LastRequestCommands? lrCommands;
  private LastRequestRebelCommands? rebelCommands;
  private EndRaceCommands? endRaceCommands;

  public override void Load(bool hotReload) {
    // Create services
    locale       = new LastRequestLocale();
    manager      = new LastRequestManager(locale, this);
    rebelManager = new LastRequestRebelManager(locale);

    var shapeFactory = new StubBeamShapeFactory();
    factory = new LastRequestFactory(manager, locale, shapeFactory, this);

    // Initialize manager with factory reference
    manager.Initialize(factory);

    // Create command handlers
    lrCommands = new LastRequestCommands(manager, rebelManager, locale,
      factory, this);
    rebelCommands = new LastRequestRebelCommands(manager, rebelManager,
      locale, this);
    endRaceCommands = new EndRaceCommands(manager, locale);

    // Register commands
    AddCommand("css_lr", "Start a last request as a prisoner",
      Command_LastRequest);
    AddCommand("css_rebel", "Rebel during last request as a prisoner",
      Command_Rebel);
    AddCommand("css_endrace", "Used to set the end point of a race LR",
      Command_EndRace);

    // Register event handlers
    RegisterEventHandler<EventRoundStart>(OnRoundStart);
    RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
    RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
  }

  public override void Unload(bool hotReload) {
    manager?.Shutdown();
  }

  // Command wrappers
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  private void Command_LastRequest(CCSPlayerController? executor,
    CommandInfo info) {
    lrCommands?.Command_LastRequest(executor, info);
  }

  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  private void Command_Rebel(CCSPlayerController? executor,
    CommandInfo info) {
    rebelCommands?.Command_Rebel(executor, info);
  }

  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  private void Command_EndRace(CCSPlayerController? executor,
    CommandInfo info) {
    endRaceCommands?.Command_EndRace(executor, info);
  }

  // Event handler wrappers
  private HookResult OnRoundStart(EventRoundStart @event,
    GameEventInfo info) {
    return manager?.OnRoundStart(@event, info) ?? HookResult.Continue;
  }

  private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = manager?.OnRoundEnd(@event, info) ?? HookResult.Continue;
    rebelCommands?.OnRoundEnd(@event, info);
    return result;
  }

  private HookResult OnPlayerDeath(EventPlayerDeath @event,
    GameEventInfo info) {
    return manager?.OnPlayerDeath(@event, info) ?? HookResult.Continue;
  }

  private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event,
    GameEventInfo info) {
    return manager?.OnPlayerDisconnect(@event, info) ?? HookResult.Continue;
  }

  private HookResult OnPlayerHurt(EventPlayerHurt @event,
    GameEventInfo info) {
    return manager?.OnTakeDamage(@event, info) ?? HookResult.Continue;
  }
}
