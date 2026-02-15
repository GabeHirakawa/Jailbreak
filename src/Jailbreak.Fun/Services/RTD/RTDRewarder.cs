using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Fun.Utils;

namespace Jailbreak.Fun.Services.RTD;

public class RTDRewarder {
  private readonly Dictionary<int, IRTDReward> rewards = new();

  public bool HasReward(int id) { return GetReward(id) != null; }
  public bool HasReward(CCSPlayerController player) { return HasReward(player.UserId ?? -1); }

  public IRTDReward? GetReward(int id) {
    return rewards.TryGetValue(id, out var reward) ? reward : null;
  }

  public IRTDReward? GetReward(CCSPlayerController player) {
    return GetReward(player.UserId ?? -1);
  }

  public bool SetReward(int id, IRTDReward reward) {
    if (!reward.PrepareReward(id)) return false;
    rewards[id] = reward;
    return true;
  }

  public bool SetReward(CCSPlayerController player, IRTDReward reward) {
    return SetReward(player.UserId ?? -1, reward);
  }

  public HookResult OnSpawn(EventPlayerSpawn @event, GameEventInfo info) {
    if (RoundUtil.IsWarmup()) return HookResult.Continue;
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;

    var id = player.UserId ?? -1;
    var reward = GetReward(id);
    if (reward == null) return HookResult.Continue;
    if (!reward.CanGrantReward(player)) return HookResult.Continue;

    Server.RunOnTick(Server.TickCount + 2, () => {
      if (!player.IsValid) return;
      reward.GrantReward(id);
      rewards.Remove(id);
    });
    return HookResult.Continue;
  }
}
