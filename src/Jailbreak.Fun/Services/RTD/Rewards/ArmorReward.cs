using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Fun.Services.RTD.Rewards;

public class ArmorReward(int armor) : IRTDReward {
  public string Name => armor + " Armor";
  public string Description => "You will spawn with " + armor + " Armor next round.";

  public bool GrantReward(CCSPlayerController player) {
    player.SetArmor(armor);
    return true;
  }
}
