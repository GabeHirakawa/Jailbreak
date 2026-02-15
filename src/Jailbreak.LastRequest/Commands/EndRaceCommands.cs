using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.LastRequest.Enums;
using Jailbreak.LastRequest.Locale;
using Jailbreak.LastRequest.Services;

namespace Jailbreak.LastRequest.Commands;

public class EndRaceCommands {
  private readonly ILastRequestManager lrManager;
  private readonly ILastRequestLocale messages;

  public EndRaceCommands(ILastRequestManager lrManager,
    ILastRequestLocale messages) {
    this.lrManager = lrManager;
    this.messages  = messages;
  }

  public void Command_EndRace(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor == null) return;
    var lr = lrManager.GetActiveLR(executor);

    if (lr is not { Type: LRType.RACE }) {
      messages.RaceNotInRaceLR().ToChat(executor);
      return;
    }

    if (lr.State != LRState.PENDING) {
      messages.RaceNotInPendingState().ToChat(executor);
      return;
    }

    lr.Execute();
  }
}
