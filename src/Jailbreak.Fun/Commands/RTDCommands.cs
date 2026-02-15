using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Fun.Locale;
using Jailbreak.Fun.Services.RTD;
using Jailbreak.Fun.Utils;

namespace Jailbreak.Fun.Commands;

public class RTDCommands {
  public static readonly FakeConVar<int> CV_RTD_ENABLED =
    new("css_jb_rtd_minplayers",
      "Minimum amount of players to enable rolling the dice", 3);

  private readonly RTDRewarder rewarder;
  private readonly RewardGenerator generator;
  private readonly RTDLocale locale;
  private bool inBetweenRounds;

  public RTDCommands(RTDRewarder rewarder, RewardGenerator generator,
    RTDLocale locale) {
    this.rewarder  = rewarder;
    this.generator = generator;
    this.locale    = locale;
  }

  public void OnRTDCommand(CCSPlayerController? executor, CommandInfo info) {
    if (executor == null) return;
    var bypass = AdminManager.PlayerHasPermissions(executor, "@css/root")
      && info.ArgCount == 2;

    var old = rewarder.GetReward(executor);
    if (!bypass && old != null) {
      locale.AlreadyRolled(old).ToChat(executor);
      return;
    }

    var count = Utilities.GetPlayers().Count(p => p.Team > CsTeam.Spectator);

    if (!bypass) {
      if (count < CV_RTD_ENABLED.Value) {
        locale.RollingDisabled().ToChat(executor);
        return;
      }

      if (!inBetweenRounds && !RoundUtil.IsWarmup() && executor.PawnIsAlive) {
        locale.CannotRollYet().ToChat(executor);
        return;
      }
    }

    var reward = generator.GenerateReward(executor);
    if (bypass) {
      if (!int.TryParse(info.GetArg(1), out var slot)) return;
      if (slot != -1) {
        var rewards = generator.ToList();
        if (slot < 0 || slot >= rewards.Count) return;
        reward = rewards[slot].Item1;
      }
    }

    rewarder.SetReward(executor, reward);
    locale.RewardSelected(reward).ToChat(executor);
  }

  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    inBetweenRounds = true;
    return HookResult.Continue;
  }

  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    inBetweenRounds = false;
    return HookResult.Continue;
  }
}
