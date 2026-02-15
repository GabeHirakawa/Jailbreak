using CounterStrikeSharp.API.Core;

namespace Jailbreak.Contracts.Services;

/// <summary>
/// Represents a type of Special Day (e.g., FFA, HnS, Warday, Infection).
/// Each implementation defines one SD type and its lifecycle.
/// Implementations are registered via DI in the SpecialDay/Fun plugin.
/// </summary>
public interface ISpecialDay {
    /// <summary>
    /// Display name for this Special Day type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Brief description of the rules/mechanics.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Called when the warden initially picks the special day.
    /// Use for teleporting, stripping weapons, starting timers,
    /// setting convars, freezing players, etc.
    /// </summary>
    void Setup();

    /// <summary>
    /// Called when the actual action begins (e.g., after a freeze period).
    /// Typically enables damage, hooks weapon acquisition, etc.
    /// </summary>
    void Execute();

    /// <summary>
    /// Called when the round ends. Use for cleanup: restoring convars,
    /// unhooking events, etc.
    /// </summary>
    void OnEnd();
}
