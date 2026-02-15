using System.Collections;
using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Models;
using Jailbreak.Fun.Services.RTD.Rewards;

namespace Jailbreak.Fun.Services.RTD;

public class RewardGenerator : IReadOnlyCollection<(IRTDReward, float)> {
  private const float PROB_LOTTERY = 1 / 5000f;
  private const float PROB_EXTREMELY_LOW = 1 / 800f;
  private const float PROB_VERY_LOW = 1 / 100f;
  private const float PROB_LOW = 1 / 20f;
  private const float PROB_MEDIUM = 1 / 10f;
  private const float PROB_OFTEN = 1 / 5f;
  private const float PROB_VERY_OFTEN = 1 / 2f;

  private readonly List<(IRTDReward, float)> rewards = [];
  private readonly Random rng = new();

  private float totalWeight
    => rewards.Where(r => r.Item1.Enabled).Select(s => s.Item2).Sum();

  public void Start(BasePlugin plugin) {
    rewards.AddRange([
      // Very often
      (new NothingReward(), PROB_VERY_OFTEN),
      // TODO: CreditReward requires GangsAPI

      // Often
      (new WeaponReward("weapon_healthshot"), PROB_OFTEN),
      (new WeaponReward("weapon_decoy"), PROB_OFTEN),
      (new HPReward(110), PROB_OFTEN),
      (new ArmorReward(15), PROB_OFTEN),

      // Medium
      (new WeaponReward("weapon_flashbang"), PROB_MEDIUM),
      (new WeaponReward("weapon_hegrenade"), PROB_MEDIUM),
      (new WeaponReward("weapon_smokegrenade"), PROB_MEDIUM),
      (new WeaponReward("weapon_molotov"), PROB_MEDIUM),
      (new WeaponReward("weapon_taser"), PROB_MEDIUM),
      (new CannotUseReward(plugin, WeaponType.UTILITY), PROB_MEDIUM),
      (new HPReward(150), PROB_MEDIUM),
      (new HPReward(50), PROB_MEDIUM),
      (new ArmorReward(150), PROB_MEDIUM),
      // TODO: GuaranteedWardenReward requires IWardenSelectionService
      (new WeaponReward("weapon_g3sg1", CsTeam.CounterTerrorist), PROB_MEDIUM / 2),

      // Low
      (new AmmoWeaponReward("weapon_glock", 0, 0), PROB_LOW),
      // TODO: ChatSpyReward requires MAULActain
      (new ColorReward(Color.FromArgb(0, 255, 0), true), PROB_LOW),
      (new CannotUseReward(plugin, WeaponType.GRENADE), PROB_LOW),
      (new CannotScope(plugin), PROB_LOW),
      (new CannotRightKnife(plugin), PROB_LOW),
      (new CannotUseReward(plugin, WeaponType.SNIPERS), PROB_LOW),
      (new CannotUseReward(plugin, WeaponType.HEAVY), PROB_LOW),
      (new TransparentReward(), PROB_LOW / 2),
      (new AmmoWeaponReward("weapon_glock", 2, 0), PROB_LOW / 2),
      (new AmmoWeaponReward("weapon_negev", 0, 6), PROB_LOW / 2),
      (new HPReward(1), PROB_LOW / 2),

      // Very low
      (new FakeBombReward(), PROB_VERY_LOW * 2),
      (new CannotLeftKnife(plugin), PROB_VERY_LOW),
      (new NoWeaponReward(), PROB_VERY_LOW),
      (new CannotUseReward(plugin, WeaponType.SMGS), PROB_VERY_LOW),
      (new CannotUseReward(plugin, WeaponType.PISTOLS), PROB_VERY_LOW),
      (new CannotUseReward(plugin, WeaponType.RIFLES), PROB_VERY_LOW),
      // TODO: RandomTeleportReward requires IZoneManager
      // TODO: BombReward requires IC4Service
      (new AmmoWeaponReward("weapon_deagle", 1, 0), PROB_VERY_LOW / 2),
      (new AmmoWeaponReward("weapon_awp", 1, 0), PROB_VERY_LOW / 4),

      // Extremely low
      (new CannotUseReward(plugin, WeaponType.KNIVES), PROB_EXTREMELY_LOW),
      (new CannotUseReward(plugin, WeaponType.GUNS), PROB_EXTREMELY_LOW),
      (new AmmoWeaponReward("weapon_awp", 3, 0), PROB_EXTREMELY_LOW),
      (new WeaponReward("weapon_glock"), PROB_EXTREMELY_LOW),
    ]);
  }

  public IRTDReward GenerateReward(int? id) {
    var effectiveTotal = id == null ?
      totalWeight :
      rewards.Where(reward
          => reward.Item1.Enabled && reward.Item1.CanGrantReward(id.Value))
       .Select(reward => reward.Item2)
       .Sum();
    var roll = rng.NextDouble() * effectiveTotal;

    foreach (var reward in rewards.Where(reward => reward.Item1.Enabled)) {
      if (id != null && !reward.Item1.CanGrantReward(id.Value)) continue;
      roll -= reward.Item2;
      if (roll <= 0) return reward.Item1;
    }

    throw new InvalidOperationException("No reward was generated. (" + roll + ")");
  }

  public IRTDReward GenerateReward(CCSPlayerController player) {
    return GenerateReward(player.UserId);
  }

  public IEnumerator<(IRTDReward, float)> GetEnumerator() {
    return rewards.Where(r => r.Item1.Enabled).GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
  public int Count => rewards.Count(r => r.Item1.Enabled);
}
