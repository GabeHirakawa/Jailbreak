using CounterStrikeSharp.API.Core;

namespace Jailbreak.Tools.Commands;

// TODO: Wire to IRebelService when cross-plugin capability is available
// Original: foreach player -> IRebelService.UnmarkRebel(player)
public class PardonCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    info.ReplyToCommand("Pardon: not yet wired to Core plugin");
  }
}
