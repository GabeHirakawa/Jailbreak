using CounterStrikeSharp.API.Core;

namespace Jailbreak.Contracts.Services;

/// <summary>
/// Represents a single Roll-The-Dice reward type (e.g., HP boost, weapon, credits).
/// Each implementation defines one reward and its grant logic.
/// Implementations are registered via DI in the RTD plugin.
/// </summary>
public interface IRTDReward {
    /// <summary>
    /// Display name for this reward.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Optional description shown to the player.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Whether this reward is currently enabled.
    /// Disabled rewards are skipped during generation.
    /// </summary>
    bool Enabled => true;

    /// <summary>
    /// Probability weight for random selection.
    /// Higher values mean more likely to be rolled.
    /// </summary>
    float Weight => 1.0f;

    /// <summary>
    /// Whether the given player is eligible to receive this reward.
    /// For example, team-restricted rewards may return false for the wrong team.
    /// </summary>
    bool CanGrantReward(CCSPlayerController player) => true;

    /// <summary>
    /// Called before the reward is granted. Use for validation or
    /// setup that must happen before the actual grant.
    /// Return false to abort granting.
    /// </summary>
    bool PrepareReward(CCSPlayerController player) => true;

    /// <summary>
    /// Grants the reward to the given player.
    /// Return true if the reward was successfully applied.
    /// </summary>
    bool GrantReward(CCSPlayerController player);
}
