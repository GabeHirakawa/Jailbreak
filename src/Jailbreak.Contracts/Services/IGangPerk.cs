namespace Jailbreak.Contracts.Services;

/// <summary>
/// Represents a Gang Perk that can be purchased and activated.
/// Each implementation defines one perk type and its behavior.
/// Implementations are registered via DI in the Gangs plugin.
///
/// Note: This is a Jailbreak-side contract for gang perks. The actual
/// GangsAPI types (IPerk, IStat, IGangPlayer) remain in the GangsAPI
/// assembly; this interface provides the Jailbreak-specific contract
/// that perk implementations fulfill.
/// </summary>
public interface IGangPerk {
    /// <summary>
    /// Unique identifier for this perk (used for persistence).
    /// </summary>
    string StatId { get; }

    /// <summary>
    /// Display name for this perk.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Optional description shown in the perk shop/menu.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// The CLR type of the perk's stored value (e.g., typeof(int), typeof(bool)).
    /// Used for serialization and storage.
    /// </summary>
    Type ValueType { get; }
}
