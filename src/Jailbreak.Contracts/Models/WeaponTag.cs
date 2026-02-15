namespace Jailbreak.Contracts.Models;

/// <summary>
/// Static weapon tag data: sets of weapon designer names grouped by category.
/// Migrated from Jailbreak.Tag for use in validators and weapon logic.
/// </summary>
public static class WeaponTag {
  /// <summary>
  ///   Items that can backstab
  /// </summary>
  public static readonly IReadOnlySet<string> KNIVES = new HashSet<string>([
    "weapon_knife", "weapon_knife_bayonet", "weapon_knife_butterfly",
    "weapon_knife_canis", "weapon_knife_cord", "weapon_knife_css",
    "weapon_knife_falchion", "weapon_knife_flip", "weapon_knife_gut",
    "weapon_knife_gypsy_jackknife", "weapon_knife_karambit",
    "weapon_knife_m9_bayonet", "weapon_knife_push", "weapon_knife_skeleton",
    "weapon_knife_stiletto", "weapon_knife_survival_bowie",
    "weapon_knife_tactical", "weapon_knife_talon", "weapon_knife_ursus",
    "weapon_bayonet"
  ]);

  /// <summary>
  ///   Items that are thrown and exist in the grenade slot
  /// </summary>
  public static readonly IReadOnlySet<string> GRENADES = new HashSet<string>([
    "weapon_decoy", "weapon_firebomb", "weapon_flashbang", "weapon_hegrenade",
    "weapon_incgrenade", "weapon_molotov", "weapon_smokegrenade",
    "weapon_tagrenade", "weapon_frag"
  ]);

  /// <summary>
  ///   Items that do not shoot bullets
  /// </summary>
  public static readonly IReadOnlySet<string> UTILITY = new HashSet<string>([
      "weapon_healthshot", "item_assaultsuit", "item_kevlar",
      "weapon_diversion",
      "weapon_breachcharge", "weapon_bumpmine", "weapon_c4", "weapon_tablet",
      "weapon_taser", "weapon_shield", "weapon_snowball"
    ]).Union(GRENADES)
   .ToHashSet();

  public static readonly IReadOnlySet<string> SNIPERS = new HashSet<string>([
    "weapon_awp", "weapon_ssg08", "weapon_scar20", "weapon_g3sg1"
  ]);

  public static readonly IReadOnlySet<string> PISTOLS = new HashSet<string>([
    "weapon_deagle", "weapon_elite", "weapon_fiveseven", "weapon_glock",
    "weapon_hkp2000", "weapon_p250", "weapon_usp_silencer", "weapon_tec9",
    "weapon_cz75a", "weapon_revolver"
  ]);

  public static readonly IReadOnlySet<string> SHOTGUNS = new HashSet<string>([
    "weapon_mag7", "weapon_nova", "weapon_sawedoff", "weapon_xm1014"
  ]);

  public static readonly IReadOnlySet<string> SMGS = new HashSet<string>([
    "weapon_bizon", "weapon_mac10", "weapon_mp5sd", "weapon_mp7", "weapon_mp9",
    "weapon_p90", "weapon_ump45"
  ]);

  public static readonly IReadOnlySet<string> HEAVY = new HashSet<string>([
    "weapon_negev", "weapon_m249"
  ]);

  public static readonly IReadOnlySet<string> RIFLES = new HashSet<string>([
      "weapon_ak47", "weapon_aug", "weapon_famas", "weapon_galilar",
      "weapon_m4a1", "weapon_m4a1_silencer", "weapon_sg556"
    ]).Union(SNIPERS)
   .Union(SHOTGUNS)
   .Union(SMGS)
   .Union(HEAVY)
   .ToHashSet();

  public static readonly IReadOnlySet<string> GUNS = RIFLES.Union(PISTOLS)
   .Union(RIFLES)
   .ToHashSet();

  public static readonly IReadOnlySet<string> WEAPONS =
    GUNS.Union(KNIVES).ToHashSet();
}

/// <summary>
/// Weapon category flags for filtering and validation.
/// </summary>
[Flags]
public enum WeaponType {
  GRENADE = 1 << 0,
  UTILITY = 1 << 1,
  WEAPON = 1 << 2,
  SNIPERS = 1 << 3,
  RIFLES = 1 << 4,
  PISTOLS = 1 << 5,
  SHOTGUNS = 1 << 6,
  SMGS = 1 << 7,
  HEAVY = 1 << 8,
  GUNS = 1 << 9,
  KNIVES = 1 << 10
}

public static class WeaponTypeExtensions {
  public static IReadOnlySet<string> GetItems(this WeaponType type) {
    var result = new HashSet<string>();

    switch (type) {
      case WeaponType.GUNS:
        return WeaponTag.GUNS;
      case WeaponType.HEAVY:
        return WeaponTag.HEAVY;
      case WeaponType.SMGS:
        return WeaponTag.SMGS;
      case WeaponType.SHOTGUNS:
        return WeaponTag.SHOTGUNS;
      case WeaponType.PISTOLS:
        return WeaponTag.PISTOLS;
      case WeaponType.RIFLES:
        return WeaponTag.RIFLES;
      case WeaponType.SNIPERS:
        return WeaponTag.SNIPERS;
      case WeaponType.UTILITY:
        return WeaponTag.UTILITY;
      case WeaponType.GRENADE:
        return WeaponTag.GRENADES;
      case WeaponType.WEAPON:
        return WeaponTag.WEAPONS;
      case WeaponType.KNIVES:
        return WeaponTag.KNIVES;
      default:
        foreach (var t in Enum.GetValues<WeaponType>())
          if (type.HasFlag(t))
            result.UnionWith(t.GetItems());

        return result;
    }
  }
}
