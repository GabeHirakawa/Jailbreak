using CounterStrikeSharp.API.Core;

namespace Jailbreak.Tools.Commands;

public class CSayCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    if (executor == null) return;

    executor.PrintToCenterHtml(info.ArgString);
  }
}
