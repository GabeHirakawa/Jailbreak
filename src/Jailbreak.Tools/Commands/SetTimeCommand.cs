using CounterStrikeSharp.API.Core;
using Jailbreak.Tools.Utils;

namespace Jailbreak.Tools.Commands;

public class SetTimeCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    if (info.ArgCount != 2) {
      info.ReplyToCommand("Specify time?");
      return;
    }

    if (!int.TryParse(info.GetArg(1), out var time)) {
      info.ReplyToCommand("Invalid time");
      return;
    }

    RoundUtil.SetTimeRemaining(time);
    info.ReplyToCommand($"Set time to {time} seconds.");
  }
}
