using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using Jailbreak.Contracts;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Services;
using Jailbreak.Core.Services.Logs;
using Jailbreak.Core.Services.Mute;
using Jailbreak.Core.Services.Rebel;
using Jailbreak.Core.Services.State;
using Jailbreak.Core.Locale;
using Jailbreak.Core.Services.Stubs;
using Jailbreak.Core.Services.Warden;
using Jailbreak.Core.Services.Warden.Icons;
using Jailbreak.Core.Services.Warden.Markers;
using Jailbreak.Core.Services.Warden.Paint;
using Jailbreak.Core.Services.Warden.Selection;
using Jailbreak.Core.Services.Warden.SpecialTreatment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Core;

public class CorePlugin : BasePlugin {
    public override string ModuleName => "Jailbreak Core";
    public override string ModuleVersion => "2.0.0";
    public override string ModuleAuthor => "EdgeGamers Development";

    private JailbreakCoreService? coreService;

    public override void Load(bool hotReload) {
        // Precache resources
        RegisterListener<Listeners.OnServerPrecacheResources>(manifest => {
            manifest.AddResource("particles/explosions_fx/explosion_c4_500.vpcf");
            manifest.AddResource("soundevents/soundevents_jb.vsndevts");
            manifest.AddResource("sounds/explosion.vsnd");
            manifest.AddResource("sounds/jihad.vsnd");
            manifest.AddResource(
                "models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl");
        });

        // Create stub services for dependencies not yet migrated
        var specialDayManager = new StubSpecialDayManager();

        // Create consolidated locale
        var locale = new CoreLocale();

        // Create draw stubs
        var registry = new StubBeamShapeRegistry();
        var shapeFactory = new StubBeamShapeFactory();

        // Create state tracking services
        var globalTracker = new GlobalStateTracker();
        var aliveTracker = new AliveStateTracker();
        var roundTracker = new RoundStateTracker();
        var stateFactory = new PlayerStateFactory(globalTracker, aliveTracker, roundTracker);
        var coroutines = new CoroutineManager();

        // Create marker settings
        var markerSettings = new WardenMarkerSettingsService(registry);

        // Build a service provider with all services registered
        var services = new ServiceCollection();
        services.AddSingleton<IWardenMarkerSettings>(markerSettings);
        services.AddLogging();

        // Pre-build provider for services that need early resolution
        var provider = services.BuildServiceProvider();

        // Create icon services (no-op without ITextSpawner)
        var wardenIcon = new WardenIconService(provider);
        var specialIcon = new SpecialIconService(provider);

        // Create log service (needed by rebel service)
        var playerTagHelper = new PlayerTagHelper(provider);
        var logService = new LogService(locale, playerTagHelper);

        // Create special treatment service
        var stService = new SpecialTreatmentService(stateFactory, locale, provider);

        // Create rebel service (replaces StubRebelService)
        var rebelService = new RebelService(locale, logService, stService);
        rebelService.Initialize(this);

        // Create mute service (replaces StubMuteService)
        // Warden is set after construction to break circular dependency
        var muteService = new MuteService(locale);

        // Create the warden service
        var wardenService = new WardenService(
            provider.GetRequiredService<ILogger<WardenService>>(),
            locale, markerSettings, stService,
            muteService, rebelService, specialDayManager, provider);
        wardenService.Initialize(this);

        // Now wire up the circular reference
        muteService.SetWardenService(wardenService);
        muteService.Initialize(this);

        // Now that rebel and warden are available, rebuild the service provider
        // for services that resolve lazily (PlayerTagHelper, SpecialTreatmentService)
        var fullServices = new ServiceCollection();
        fullServices.AddSingleton<IWardenMarkerSettings>(markerSettings);
        fullServices.AddSingleton<IRebelService>(rebelService);
        fullServices.AddSingleton<IWardenService>(wardenService);
        fullServices.AddSingleton<ISpecialTreatmentService>(stService);
        fullServices.AddSingleton<IRichLogService>(logService);
        fullServices.AddSingleton<ILogService>(logService);
        fullServices.AddSingleton<ICoreLocale>(locale);
        fullServices.AddLogging();
        var fullProvider = fullServices.BuildServiceProvider();

        // Re-create PlayerTagHelper with full provider so it can lazily resolve services
        var fullPlayerTagHelper = new PlayerTagHelper(fullProvider);
        var fullLogService = new LogService(locale, fullPlayerTagHelper);

        // Initialize SpecialTreatmentService with rebel service
        stService.Initialize(rebelService);

        // Create C4 behavior
        var c4Behavior = new C4Behavior(locale, rebelService, fullProvider);
        c4Behavior.Initialize(this);

        // Create rebel listener
        var rebelListener = new RebelListener(rebelService);
        rebelListener.Initialize(this);

        // Create selection service
        var selectionService = new WardenSelection(stateFactory,
            wardenService, locale,
            provider.GetRequiredService<ILogger<WardenSelection>>(),
            coroutines);

        // Create marker service
        var markerService = new WardenMarkerService(
            wardenService, locale, shapeFactory, markerSettings);
        markerService.Initialize(this);

        // Create paint service
        var paintService = new WardenPaintService(wardenService, provider);
        paintService.Initialize(this);

        // Register game event handlers manually
        RegisterEventHandler<EventPlayerDeath>(wardenService.OnDeath);
        RegisterEventHandler<EventPlayerTeam>(wardenService.OnChangeTeam);
        RegisterEventHandler<EventRoundEnd>(wardenService.OnRoundEnd);
        RegisterEventHandler<EventRoundStart>(wardenService.OnRoundStart);
        RegisterEventHandler<EventPlayerDisconnect>(wardenService.OnPlayerDisconnect);

        RegisterEventHandler<EventRoundStart>(selectionService.OnRoundStart);

        RegisterEventHandler<EventPlayerDeath>(stService.OnDeath);

        RegisterEventHandler<EventPlayerPing>(markerService.OnPing);

        RegisterEventHandler<EventRoundStart>(paintService.OnRoundStart);

        // Register state tracker event handlers
        RegisterEventHandler<EventPlayerDisconnect>(globalTracker.OnDisconnect);
        RegisterEventHandler<EventPlayerDeath>(aliveTracker.OnDeath);
        RegisterEventHandler<EventRoundEnd>(roundTracker.OnRoundEnd);
        RegisterEventHandler<EventRoundEnd>(coroutines.OnRoundEnd);

        // Register mute event handlers
        RegisterEventHandler<EventRoundStart>(muteService.OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(muteService.OnRoundEnd);
        RegisterEventHandler<EventPlayerDeath>(muteService.OnDeath);

        // Register C4 event handlers
        RegisterEventHandler<EventRoundStart>(c4Behavior.OnRoundStart);
        RegisterEventHandler<EventBombDropped>(c4Behavior.OnPlayerDropC4);
        RegisterEventHandler<EventPlayerDeath>(c4Behavior.OnPlayerDeath, HookMode.Pre);

        // Register log service event handlers
        RegisterEventHandler<EventRoundEnd>(fullLogService.OnRoundEnd);
        RegisterEventHandler<EventRoundStart>(fullLogService.OnRoundStart);

        // Register log damage listeners
        var logDamageListeners = new LogDamageListeners(fullLogService);
        RegisterEventHandler<EventGrenadeThrown>(logDamageListeners.OnGrenadeThrown);
        RegisterEventHandler<EventPlayerHurt>(logDamageListeners.OnPlayerHurt);
        RegisterEventHandler<EventPlayerDeath>(logDamageListeners.OnPlayerDeath);

        // Register log entity listeners
        var logEntityListeners = new LogEntityListeners(fullLogService);
        HookEntityOutput("func_button", "OnPressed", logEntityListeners.OnButtonPressed);
        HookEntityOutput("func_breakable", "OnBreak", logEntityListeners.OnBreakableBroken);

        // Register log entity parent listeners
        var logEntityParentListeners = new LogEntityParentListeners(fullLogService);
        logEntityParentListeners.Initialize(this);
        RegisterEventHandler<EventRoundEnd>(logEntityParentListeners.OnRoundEnd);

        // Register logs command
        var logsCommand = new LogsCommand(fullLogService);
        AddCommand("css_logs", "View game logs", logsCommand.Command_Logs);

        // Register commands
        var wardenCmds = new Commands.WardenCommands(locale,
            selectionService, wardenService);
        RegisterEventHandler<EventRoundStart>(wardenCmds.OnRoundStart);
        RegisterEventHandler<EventPlayerDeath>(wardenCmds.OnWardenDeath);
        AddCommand("css_pass", "Pass warden", wardenCmds.Command_Pass);
        AddCommand("css_uw", "Pass warden", wardenCmds.Command_Pass);
        AddCommand("css_fire", "Force warden to pass", wardenCmds.Command_Fire);
        AddCommand("css_warden", "Become warden or join queue", wardenCmds.Command_Warden);
        AddCommand("css_w", "Become warden or join queue", wardenCmds.Command_Warden);

        var openCmds = new Commands.OpenCellsCommands(wardenService,
            locale, provider);
        RegisterEventHandler<EventRoundStart>(openCmds.OnRoundStart);
        AddCommand("css_open", "Open cells", openCmds.Command_Open);
        AddCommand("css_o", "Open cells", openCmds.Command_Open);

        var countCmds = new Commands.CountCommands(wardenService,
            locale, markerService);
        AddCommand("css_count", "Count prisoners in marker", countCmds.Command_Count);

        var chickenCmds = new Commands.ChickenCommands(wardenService, locale);
        RegisterEventHandler<EventRoundStart>(chickenCmds.OnRoundStart);
        AddCommand("css_chicken", "Spawn chicken", chickenCmds.Command_Toggle);

        var soccerCmds = new Commands.SoccerCommands(wardenService, locale);
        RegisterEventHandler<EventRoundStart>(soccerCmds.OnRoundStart);
        AddCommand("css_soccer", "Spawn soccer ball", soccerCmds.Command_Toggle);
        AddCommand("css_spawnball", "Spawn soccer ball", soccerCmds.Command_Toggle);

        var rollCmds = new Commands.RollCommands(wardenService, locale);
        AddCommand("css_roll", "Roll a number", rollCmds.Command_Toggle);

        var peaceCmds = new Commands.PeaceCommands(wardenService,
            muteService, locale);
        AddCommand("css_peace", "Invoke peace period", peaceCmds.Command_Peace);

        var stCmds = new Commands.SpecialTreatmentCommands(wardenService,
            stService, locale);
        AddCommand("css_treat", "Toggle special treatment", stCmds.Command_Toggle);
        AddCommand("css_st", "Toggle special treatment", stCmds.Command_Toggle);

        var countdownCmds = new Commands.CountdownCommands(wardenService,
            muteService, locale);
        AddCommand("css_countdown", "Start countdown", countdownCmds.Command_Countdown);

        var markerCmds = new Commands.MarkerCommands(locale,
            registry, markerSettings);
        markerCmds.Initialize(this);
        AddCommand("css_markertype", "Change marker type", markerCmds.Command_MarkerType);
        AddCommand("css_markercolor", "Change marker color", markerCmds.Command_MarkerColor);

        var autoWarden = new AutoWarden(selectionService, locale);
        autoWarden.Initialize(this);
        AddCommand("css_aw", "Toggle auto-warden", autoWarden.Command_AutoWarden);
        AddCommand("css_autowarden", "Toggle auto-warden", autoWarden.Command_AutoWarden);

        // Create core service and expose via capability
        var weaponService = new WeaponService();
        coreService = new JailbreakCoreService(wardenService, weaponService);
        Capabilities.RegisterPluginCapability(JailbreakApi.Core, () => coreService);
    }

    public override void Unload(bool hotReload) {
        coreService = null;
    }
}

/// <summary>
/// No-op stub implementations for beam shape systems.
/// Will be replaced when Draw module is migrated.
/// </summary>
file class StubBeamShapeRegistry : IBeamShapeRegistry {
    public IEnumerable<BeamShapeType> GetAllTypes() => [BeamShapeType.CIRCLE];
    public System.Collections.Generic.Dictionary<string, System.Drawing.Color> GetAllColors()
        => new() { ["White"] = System.Drawing.Color.White };
}

file class StubBeamShapeFactory : IBeamShapeFactory {
    public BeamedPolylineShape CreateShape(CounterStrikeSharp.API.Modules.Utils.Vector position,
        BeamShapeType shapeType, float? radius = null, float? width = null)
        => new();
}
