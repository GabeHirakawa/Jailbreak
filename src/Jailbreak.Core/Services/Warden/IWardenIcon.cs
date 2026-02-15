using CounterStrikeSharp.API.Core;

namespace Jailbreak.Core.Services.Warden;

/// <summary>
/// Service for managing warden overhead icons.
/// Co-located from Jailbreak.Public.Mod.Warden.IWardenIcon.
/// </summary>
public interface IWardenIcon {
  void AssignWardenIcon(CCSPlayerController warden);
  void RemoveWardenIcon(CCSPlayerController warden);
}

/// <summary>
/// Service for managing special treatment overhead icons.
/// Co-located from Jailbreak.Public.Mod.Warden.ISpecialIcon.
/// </summary>
public interface ISpecialIcon {
  void AssignSpecialIcon(CCSPlayerController player);
  void RemoveSpecialIcon(CCSPlayerController player);
}
