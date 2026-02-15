using CounterStrikeSharp.API.Core;

namespace Jailbreak.Contracts.Services;

/// <summary>
/// Cross-plugin contract for common weapon operations on players.
/// </summary>
public interface IWeaponService {
    /// <summary>
    /// Strips all weapons from the player.
    /// </summary>
    void Strip(CCSPlayerController player);

    /// <summary>
    /// Gives the player the specified weapon (e.g. "weapon_ak47").
    /// </summary>
    void Give(CCSPlayerController player, string weaponName);
}
