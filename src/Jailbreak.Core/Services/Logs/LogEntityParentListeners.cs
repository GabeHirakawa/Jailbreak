using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Models;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Logs weapon pickup/drop events by monitoring entity parent changes.
/// Migrated from Jailbreak.Logs.Listeners.LogEntityParentListeners.
/// </summary>
public class LogEntityParentListeners {
  private readonly IRichLogService logs;
  private readonly HashSet<int> recentWeaponEvents = new();

  public LogEntityParentListeners(IRichLogService logs) { this.logs = logs; }

  public void Initialize(BasePlugin parent) {
    parent
     .RegisterListener<
        CounterStrikeSharp.API.Core.Listeners.OnEntityParentChanged>(
        OnEntityParentChanged);
  }

  public void OnEntityParentChanged(CEntityInstance affectedEntity,
    CEntityInstance newParent) {
    if (!affectedEntity.IsValid) return;
    if (!WeaponTag.WEAPONS.Contains(affectedEntity.DesignerName)
      && !WeaponTag.UTILITY.Contains(affectedEntity.DesignerName))
      return;

    var weaponEntity =
      Utilities.GetEntityFromIndex<CCSWeaponBase>((int)affectedEntity.Index);
    if (weaponEntity == null
      || weaponEntity.PrevOwner.Get()?.OriginalController.Get() == null)
      return;

    var weaponOwner = weaponEntity.PrevOwner.Get()?.OriginalController.Get();
    if (weaponOwner == null) return;

    if (!newParent.IsValid) // a.k.a parent is world
    {
      logs.Append(logs.Player(weaponOwner),
        $"dropped their {weaponEntity.ToFriendlyString()}");
      return;
    }

    if (!recentWeaponEvents.Add((int)weaponEntity.Index)) {
      recentWeaponEvents.Remove((int)weaponEntity.Index);
      return;
    }

    var weaponPickerUpper = Utilities
     .GetEntityFromIndex<CCSPlayerPawn>((int)newParent.Index)
    ?.OriginalController.Get();
    if (weaponPickerUpper == null) return;

    if (weaponPickerUpper == weaponOwner) {
      logs.Append(weaponPickerUpper,
        $"picked up their {weaponEntity.ToFriendlyString()}");
      return;
    }

    logs.Append(weaponPickerUpper, "picked up", logs.Player(weaponOwner),
      $"{weaponEntity.ToFriendlyString()}");
  }

  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    recentWeaponEvents.Clear();
    return HookResult.Continue;
  }
}
