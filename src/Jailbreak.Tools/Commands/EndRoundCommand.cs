using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Tools.Utils;

namespace Jailbreak.Tools.Commands;

public class EndRoundCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
    RoundUtil.SetTimeRemaining(0);
    info.ReplyToCommand("Round ended.");
  }
}
