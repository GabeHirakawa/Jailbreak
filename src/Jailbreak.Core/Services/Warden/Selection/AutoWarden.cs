using System.Collections.Concurrent;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Core.Services.Stubs;

namespace Jailbreak.Core.Services.Warden.Selection;

/// <summary>
/// Auto-warden functionality for players with special permissions.
/// Migrated from Jailbreak.Warden.Selection.AutoWarden.
/// Actain cookie integration is disabled until migrated.
/// </summary>
public class AutoWarden {
  private static readonly ConcurrentDictionary<ulong, bool> CACHED_COOKIES = new();

  private static readonly FakeConVar<string> CV_AUTOWARDEN_FLAG =
    new("css_autowarden_flag", "Permission flag required to enable auto-Warden",
      "@ego/dssilver");
  private static readonly FakeConVar<float> CV_AUTOWARDEN_DELAY_INTERVAL =
    new("css_autowarden_delay_interval",
      "The amount of time in seconds to wait after round start to queue users with auto-warden enabled for warden",
      5f);

  private readonly IWardenSelectionService selectionService;
  private readonly IWardenLocale locale;
  private readonly IGenericCmdLocale generic;
  private BasePlugin plugin = null!;

  public AutoWarden(IWardenSelectionService selectionService,
    IWardenLocale locale, IGenericCmdLocale generic) {
    this.selectionService = selectionService;
    this.locale = locale;
    this.generic = generic;
  }

  /// <summary>
  /// Initialize with plugin reference for timer/listener support.
  /// Called from CorePlugin.Load().
  /// </summary>
  public void Initialize(BasePlugin basePlugin) {
    plugin = basePlugin;
    basePlugin.RegisterEventHandler<EventRoundPoststart>(OnRoundStart);
  }

  private HookResult OnRoundStart(EventRoundPoststart @event, GameEventInfo info) {
    plugin.AddTimer(CV_AUTOWARDEN_DELAY_INTERVAL.Value, () => {
      foreach (var player in Utilities.GetPlayers()
       .Where(p => p.Team == CsTeam.CounterTerrorist
          && p.IsReal()
          && p.PawnIsAlive
          && AdminManager.PlayerHasPermissions(p, CV_AUTOWARDEN_FLAG.Value))) {

        if (player.PlayerPawn.Value == null
          || !player.PlayerPawn.Value.HasMovedSinceSpawn)
          continue;

        var steam = player.SteamID;
        if (!CACHED_COOKIES.TryGetValue(steam, out var value) || !value)
          continue;
        selectionService.TryEnter(player);
        locale.JoinRaffle.ToChat(player);
      }
    });
    return HookResult.Continue;
  }

  [ConsoleCommand("css_aw")]
  [ConsoleCommand("css_autowarden")]
  public void Command_AutoWarden(CCSPlayerController? player, CommandInfo info) {
    if (player == null) return;
    if (!AdminManager.PlayerHasPermissions(player, CV_AUTOWARDEN_FLAG.Value)) {
      generic.NoPermissionMessage(CV_AUTOWARDEN_FLAG.Value).ToChat(player);
      return;
    }

    // TODO: Re-enable when Actain/cookie service is migrated
    // For now, just toggle the in-memory cache
    locale.TogglingNotEnabled.ToChat(player);
  }
}
