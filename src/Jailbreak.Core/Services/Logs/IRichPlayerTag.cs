using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Provides a rich format object tag for a player based on their current role.
/// Migrated from Jailbreak.Formatting.Views.Logging.IRichPlayerTag.
/// </summary>
public interface IRichPlayerTag : IPlayerTag {
  /// <summary>
  ///   Get a tag for this player, which contains context about the player's current actions
  /// </summary>
  FormatObject Rich(CCSPlayerController player);
}
