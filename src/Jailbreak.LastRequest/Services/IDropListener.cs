using CounterStrikeSharp.API.Core;

namespace Jailbreak.LastRequest.Services;

public interface IDropListener {
  void OnWeaponDrop(CCSPlayerController player, CCSWeaponBase weapon);
}
