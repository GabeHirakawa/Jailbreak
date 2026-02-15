using CounterStrikeSharp.API.Core;

namespace Jailbreak.Contracts.Services;

/// <summary>
/// Cross-plugin contract for querying and managing warden state.
/// Only exposes methods that satellite plugins need; internal
/// warden implementation details live in the Core plugin.
/// </summary>
public interface IWardenService {
    /// <summary>
    /// The current warden, or null if no warden is assigned.
    /// </summary>
    CCSPlayerController? Warden { get; }

    /// <summary>
    /// Whether a warden is currently assigned.
    /// </summary>
    bool HasWarden { get; }

    /// <summary>
    /// Returns true if the given player is the current warden.
    /// </summary>
    bool IsWarden(CCSPlayerController player);

    /// <summary>
    /// Attempts to assign the given player as warden.
    /// Returns true if the assignment succeeded.
    /// </summary>
    bool TrySetWarden(CCSPlayerController controller);

    /// <summary>
    /// Attempts to remove the current warden.
    /// <paramref name="dead"/> indicates whether the warden died
    /// (as opposed to voluntarily passing).
    /// </summary>
    bool TryRemoveWarden(bool dead = false);
}
