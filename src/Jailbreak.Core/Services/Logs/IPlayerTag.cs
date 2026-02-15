using CounterStrikeSharp.API.Core;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Provides a plain text tag for a player based on their current role.
/// Migrated from Jailbreak.Public.Mod.Logs.IPlayerTag.
/// </summary>
public interface IPlayerTag {
  /// <summary>
  ///   Get a tag that contains context about the player.
  /// </summary>
  string Plain(CCSPlayerController playerController);
}
