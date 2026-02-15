using CounterStrikeSharp.API.Core;

namespace Jailbreak.Contracts.Services;

/// <summary>
/// Result of a Last Request round.
/// </summary>
public enum LRResult {
    PrisonerWin,
    GuardWin,
    TimedOut,
    Interrupted
}

/// <summary>
/// Lifecycle state of a Last Request.
/// </summary>
public enum LRState {
    Pending,
    Active,
    Completed,
    Cancelled
}

/// <summary>
/// Represents a type of Last Request (e.g., RPS, Race, Coinflip, Knife Fight).
/// Each implementation defines one LR type and its lifecycle.
/// Implementations are registered via DI in the LastRequest plugin.
/// </summary>
public interface ILastRequest {
    /// <summary>
    /// Display name for this Last Request type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Brief description of the rules/mechanics.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The prisoner participating in this Last Request.
    /// </summary>
    CCSPlayerController Prisoner { get; }

    /// <summary>
    /// The guard participating in this Last Request.
    /// </summary>
    CCSPlayerController Guard { get; }

    /// <summary>
    /// Current lifecycle state of this Last Request instance.
    /// </summary>
    LRState State { get; }

    /// <summary>
    /// Called when the LR is first initiated. Use for teleporting,
    /// stripping weapons, giving items, etc.
    /// </summary>
    void Setup();

    /// <summary>
    /// Called when the LR countdown finishes and the actual gameplay begins.
    /// </summary>
    void Execute();

    /// <summary>
    /// Called when the LR ends (win, loss, timeout, or interruption).
    /// Use for cleanup.
    /// </summary>
    void OnEnd(LRResult result);

    /// <summary>
    /// Determines whether the given player should be prevented from
    /// equipping a weapon during this Last Request.
    /// Default implementation blocks equipping for both participants
    /// once the LR is active.
    /// </summary>
    bool PreventEquip(CCSPlayerController player, CCSWeaponBaseVData weapon) {
        if (State == LRState.Pending) return false;
        return player.Slot == Prisoner.Slot || player.Slot == Guard.Slot;
    }
}
