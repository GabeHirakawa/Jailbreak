using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using Jailbreak.Contracts;
using Jailbreak.Core.Services;
using Jailbreak.Core.Services.Warden;
using Jailbreak.Contracts.Services;

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

        // Create services
        var wardenService = new WardenServiceStub();
        var weaponService = new WeaponService();
        coreService = new JailbreakCoreService(wardenService, weaponService);

        // Register capability
        Capabilities.RegisterPluginCapability(JailbreakApi.Core, () => coreService);
    }

    public override void Unload(bool hotReload) {
        coreService = null;
    }
}
