using CounterStrikeSharp.API.Core;

namespace Jailbreak.Tools.Commands;

// TODO: Wire to ILastGuardService when cross-plugin capability is available
// Original: find first alive CT (or specified target), then lgService.StartLastGuard(target)
public class LastGuardCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    info.ReplyToCommand("LastGuard: not yet wired to Core plugin");
  }
}
