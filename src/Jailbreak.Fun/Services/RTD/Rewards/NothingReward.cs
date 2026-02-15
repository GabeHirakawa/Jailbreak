using CounterStrikeSharp.API.Core;

namespace Jailbreak.Fun.Services.RTD.Rewards;

public class NothingReward : IRTDReward {
  public string Name => "Nothing";
  public string Description => "You won nothing.";
  public bool GrantReward(CCSPlayerController player) { return true; }
}
