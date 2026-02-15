using CounterStrikeSharp.API.Core;

namespace Jailbreak.Core.Services.Warden;

/// <summary>
/// Service responsible for managing the warden selection queue.
/// Co-located from Jailbreak.Public.Mod.Warden.IWardenSelectionService.
/// </summary>
public interface IWardenSelectionService {
  /// <summary>
  /// Whether the warden queue is currently active.
  /// </summary>
  bool Active { get; }

  /// <summary>
  /// Guarantee this player becomes warden if they enter the queue.
  /// </summary>
  void SetGuaranteedWarden(CCSPlayerController player);

  /// <summary>
  /// Enter this player into the warden queue.
  /// </summary>
  bool TryEnter(CCSPlayerController player);

  /// <summary>
  /// Remove this player from the warden queue.
  /// </summary>
  bool TryExit(CCSPlayerController player);

  /// <summary>
  /// Determine whether this player is in the queue.
  /// </summary>
  bool InQueue(CCSPlayerController player);
}
