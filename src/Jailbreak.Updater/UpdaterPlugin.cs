using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Updater.Commands;
using Jailbreak.Updater.Models;
using Jailbreak.Updater.Services;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Updater;

public class UpdaterPlugin : BasePlugin {
  public override string ModuleName => "Jailbreak Updater";
  public override string ModuleVersion => "2.0.0";
  public override string ModuleAuthor => "EdgeGamers Development";

  private UpdateService? _updateService;
  private UpdateCommands? _commands;
  private UpdaterConfig _config = new();

  public override void Load(bool hotReload) {
    var pluginDir = Path.GetDirectoryName(ModulePath)!;
    var dataDir = Path.Combine(pluginDir, "data");
    var configPath = Path.Combine(pluginDir, "config.json");

    _config = LoadOrCreateConfig(configPath);
    _updateService = new UpdateService(_config, dataDir, Logger);

    // Resume from self-update: if state is "applying", finalize it
    _updateService.FinalizeIfApplying();

    // Set installed version from plugin metadata on first run
    if (_updateService.State.InstalledVersion == "0.0.0") {
      _updateService.State.InstalledVersion = ModuleVersion;
    }

    var pluginsDirectory = Path.GetDirectoryName(pluginDir)!;
    _commands = new UpdateCommands(_updateService, pluginsDirectory);

    AddCommand("css_update", "Manage Jailbreak updates", OnUpdateCommand);
    AddCommand("css_version", "Show Jailbreak version", OnVersionCommand);

    // Check for updates on load
    _ = Task.Run(async () => {
      await _updateService.CheckForUpdate();
      if (_updateService.State.Status == "staged") {
        Logger.LogInformation(
          "[Updater] v{Version} is staged and will apply on map end.",
          _updateService.State.StagedVersion);
      }
    });

    if (_config.CheckOnMapChange) {
      RegisterListener<Listeners.OnMapEnd>(() => {
        // Apply staged update before map fully ends
        if (_updateService.State.Status == "staged" && _config.AutoApply) {
          _updateService.ApplyUpdate(pluginsDirectory);
        }
      });

      RegisterListener<Listeners.OnMapStart>(mapName => {
        // Check for updates on each map start
        _ = Task.Run(async () => await _updateService.CheckForUpdate());
      });
    }
  }

  [RequiresPermissions("@css/root")]
  private void OnUpdateCommand(CCSPlayerController? executor, CommandInfo info) {
    _commands?.OnUpdateCommand(executor, info);
  }

  [RequiresPermissions("@css/root")]
  private void OnVersionCommand(CCSPlayerController? executor, CommandInfo info) {
    _commands?.OnVersionCommand(executor, info);
  }

  public override void Unload(bool hotReload) { }

  private static UpdaterConfig LoadOrCreateConfig(string path) {
    if (File.Exists(path)) {
      try {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<UpdaterConfig>(json) ?? new UpdaterConfig();
      } catch {
        return new UpdaterConfig();
      }
    }

    var config = new UpdaterConfig();
    var defaultJson = JsonSerializer.Serialize(config, new JsonSerializerOptions {
      WriteIndented = true
    });
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, defaultJson);
    return config;
  }
}
