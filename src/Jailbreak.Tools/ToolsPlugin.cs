using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Tools.Commands;

namespace Jailbreak.Tools;

public class ToolsPlugin : BasePlugin {
  public override string ModuleName => "Jailbreak Tools";
  public override string ModuleVersion => "2.0.0";
  public override string ModuleAuthor => "EdgeGamers Development";

  private readonly Dictionary<string, AbstractCommand> commands = new();

  public override void Load(bool hotReload) {
    commands.Add("markrebel", new MarkRebelCommand());
    commands.Add("pardon", new PardonCommand());
    commands.Add("lr", new LastRequestCommand());
    commands.Add("st", new MarkSTCommand());
    commands.Add("lg", new LastGuardCommand());
    commands.Add("zone", new ZoneCommand());
    commands.Add("endround", new EndRoundCommand());
    commands.Add("testnearopen", new TestNearOpenCommand());
    commands.Add("settime", new SetTimeCommand());
    commands.Add("centerhud", new CenterHudCommand());
    commands.Add("csay", new CSayCommand());
    commands.Add("color", new ColorCommand());

    AddCommand("css_debug", "Debug command for Jailbreak", OnDebugCommand);
  }

  [RequiresPermissions("@css/root")]
  private void OnDebugCommand(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor == null) return;

    if (info.ArgCount == 1) {
      foreach (var command in commands) info.ReplyToCommand(command.Key);
      return;
    }

    if (!commands.TryGetValue(info.GetArg(1), out var subcommand)) {
      info.ReplyToCommand("Invalid subcommand");
      return;
    }

    subcommand.OnCommand(executor, new WrappedInfo(info));
  }

  public override void Unload(bool hotReload) { }
}
