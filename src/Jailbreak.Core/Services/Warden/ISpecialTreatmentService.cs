using CounterStrikeSharp.API.Core;

namespace Jailbreak.Core.Services.Warden;

/// <summary>
/// Service for managing special treatment (ST) for prisoners.
/// Co-located from Jailbreak.Public.Mod.Warden.ISpecialTreatmentService.
/// </summary>
public interface ISpecialTreatmentService {
  bool IsSpecialTreatment(CCSPlayerController player);

  void SetSpecialTreatment(CCSPlayerController player, bool special) {
    if (special)
      Grant(player);
    else
      Revoke(player);
  }

  void Grant(CCSPlayerController player);

  void Revoke(CCSPlayerController player) { Revoke(player, true); }

  void Revoke(CCSPlayerController player, bool print);
}
