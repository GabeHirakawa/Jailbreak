using CounterStrikeSharp.API.Core;

namespace Jailbreak.Tools.Commands;

// TODO: Wire to MapUtil cell opening when cross-plugin capability is available
// Original: MapUtil.OpenCells(Sensitivity.ANY, executor position)
public class TestNearOpenCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    info.ReplyToCommand("TestNearOpen: not yet wired to Core plugin");
  }
}
