using CounterStrikeSharp.API.Core;

namespace Jailbreak.LastRequest.Services;

/// <summary>
/// Interface for Last Requests that require additional configuration
/// before Setup() is called.
/// </summary>
public interface ILastRequestConfig {
  /// <summary>
  /// Opens a configuration menu for the prisoner to make choices.
  /// </summary>
  void OpenConfigMenu(CCSPlayerController prisoner,
    CCSPlayerController guard,
    Action onComplete);

  /// <summary>
  /// Whether this LR requires configuration.
  /// </summary>
  bool RequiresConfiguration { get; }
}
