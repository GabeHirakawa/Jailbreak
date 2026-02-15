using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Services;

namespace Jailbreak.Core.Services;

public class WeaponService : IWeaponService {
    public void Strip(CCSPlayerController player) {
        player.RemoveWeapons();
    }

    public void Give(CCSPlayerController player, string weaponName) {
        player.GiveNamedItem(weaponName);
    }
}
