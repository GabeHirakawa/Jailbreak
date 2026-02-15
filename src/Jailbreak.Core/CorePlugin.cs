using CounterStrikeSharp.API.Core;

namespace Jailbreak.Core;

public class CorePlugin : BasePlugin {
    public override string ModuleName => "Jailbreak Core";
    public override string ModuleVersion => "2.0.0";
    public override string ModuleAuthor => "EdgeGamers Development";

    public override void Load(bool hotReload) { }
    public override void Unload(bool hotReload) { }
}
