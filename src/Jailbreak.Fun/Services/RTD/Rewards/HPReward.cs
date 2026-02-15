using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Fun.Services.RTD.Rewards;

public class HPReward(int hp) : IRTDReward {
  public string Name => hp + " HP";
  public string Description => "You won " + hp + " HP next round.";

  public bool GrantReward(CCSPlayerController player) {
    player.SetHealth(hp);
    return true;
  }
}
