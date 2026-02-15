using CounterStrikeSharp.API.Core.Capabilities;

namespace Jailbreak.Contracts;

/// <summary>
/// PluginCapability definitions for cross-plugin communication.
/// </summary>
public static class JailbreakApi {
    /// <summary>
    /// The main Jailbreak Core API. Satellite plugins use this to interact with core game state.
    /// </summary>
    public static PluginCapability<IJailbreakCore> Core { get; } =
        new("jailbreak:core");
}
