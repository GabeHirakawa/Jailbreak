using CounterStrikeSharp.API.Core;

namespace Jailbreak.Tools.Commands;

// TODO: Wire to ILastRequestManager when cross-plugin capability is available
// Original supported: enable/disable LR, initiate LR with type/player/target, menu selectors
public class LastRequestCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    info.ReplyToCommand("LastRequest: not yet wired to LastRequest plugin");
  }
}
