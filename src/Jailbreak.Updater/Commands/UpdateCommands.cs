using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Updater.Services;

namespace Jailbreak.Updater.Commands;

public class UpdateCommands {
  private readonly IUpdateService _updateService;
  private readonly string _pluginsDirectory;

  public UpdateCommands(IUpdateService updateService, string pluginsDirectory) {
    _updateService = updateService;
    _pluginsDirectory = pluginsDirectory;
  }

  public void OnUpdateCommand(CCSPlayerController? executor, CommandInfo info) {
    if (info.ArgCount < 2) {
      info.ReplyToCommand("[Updater] Usage: css_update <check|apply|status>");
      return;
    }

    var subcommand = info.GetArg(1).ToLower();

    switch (subcommand) {
      case "check":
        _ = Task.Run(async () => {
          var found = await _updateService.CheckForUpdate();
        });
        info.ReplyToCommand("[Updater] Checking for updates...");
        break;

      case "apply":
        if (_updateService.State.Status != "staged") {
          info.ReplyToCommand("[Updater] No staged update to apply.");
          return;
        }
        info.ReplyToCommand(
          $"[Updater] Will apply v{_updateService.State.StagedVersion} on map end.");
        break;

      case "status":
        var state = _updateService.State;
        info.ReplyToCommand($"[Updater] Installed: v{state.InstalledVersion}");
        if (state.StagedVersion != null)
          info.ReplyToCommand($"[Updater] Staged: v{state.StagedVersion}");
        info.ReplyToCommand($"[Updater] Status: {state.Status}");
        if (state.LastCheck.HasValue)
          info.ReplyToCommand($"[Updater] Last check: {state.LastCheck.Value:u}");
        break;

      default:
        info.ReplyToCommand("[Updater] Unknown subcommand. Use: check, apply, status");
        break;
    }
  }

  public void OnVersionCommand(CCSPlayerController? executor, CommandInfo info) {
    info.ReplyToCommand(
      $"[Jailbreak] Installed version: v{_updateService.State.InstalledVersion}");
  }
}
