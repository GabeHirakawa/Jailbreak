using CounterStrikeSharp.API.Core;

namespace Jailbreak.Tools.Commands;

// TODO: Wire to IRebelService when cross-plugin capability is available
// Original: foreach player -> IRebelService.MarkRebel(player, duration)
public class MarkRebelCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    info.ReplyToCommand("MarkRebel: not yet wired to Core plugin");
  }
}
