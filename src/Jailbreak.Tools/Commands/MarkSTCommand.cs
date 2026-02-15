using CounterStrikeSharp.API.Core;

namespace Jailbreak.Tools.Commands;

// TODO: Wire to ISpecialTreatmentService when cross-plugin capability is available
// Original: foreach player -> toggle stService.SetSpecialTreatment(player, !stService.IsSpecialTreatment(player))
public class MarkSTCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    info.ReplyToCommand("MarkST: not yet wired to Core plugin");
  }
}
