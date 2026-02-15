using System.IO.Compression;
using System.Text.Json;
using Jailbreak.Updater.Models;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Updater.Services;

public class UpdateService : IUpdateService {
  private readonly GitHubReleaseProvider _github;
  private readonly UpdaterConfig _config;
  private readonly string _dataDir;
  private readonly string _stateFilePath;
  private readonly ILogger _logger;
  private readonly SemaphoreSlim _checkLock = new(1, 1);

  public UpdateState State { get; private set; }

  public UpdateService(UpdaterConfig config, string dataDir, ILogger logger) {
    _config = config;
    _dataDir = dataDir;
    _stateFilePath = Path.Combine(dataDir, "update-state.json");
    _logger = logger;
    _github = new GitHubReleaseProvider(config.Repository);

    Directory.CreateDirectory(dataDir);
    State = LoadState();
  }

  public void FinalizeIfApplying() {
    if (State.Status != "applying") return;

    _logger.LogInformation(
      "[Updater] Finalizing update to {Version}", State.StagedVersion);
    State.InstalledVersion = State.StagedVersion!;
    State.StagedVersion = null;
    State.Status = "idle";
    CleanStaging();
    SaveState();
  }

  public async Task<bool> CheckForUpdate() {
    if (!await _checkLock.WaitAsync(0)) return false; // skip if already running
    try {
      try {
        var includePreRelease = _config.UpdateChannel == "prerelease";
        var release = await _github.GetLatestRelease(includePreRelease);

        if (release is null) {
          _logger.LogWarning("[Updater] Failed to fetch releases");
          return false;
        }

        State.LastCheck = DateTime.UtcNow;

        var remoteVersion = GitHubReleaseProvider.ParseVersion(release.TagName);

        if (!IsNewerVersion(remoteVersion, State.InstalledVersion)) {
          SaveState();
          return false;
        }

        _logger.LogInformation(
          "[Updater] New version available: {Version}", remoteVersion);

        State.Status = "downloading";
        SaveState();

        var stagingDir = Path.Combine(_dataDir, "staging");
        CleanStaging();
        Directory.CreateDirectory(stagingDir);

        var stream = await _github.DownloadAsset(release, "Jailbreak.zip");
        if (stream is null) {
          _logger.LogWarning("[Updater] Failed to download Jailbreak.zip");
          State.Status = "idle";
          SaveState();
          return false;
        }

        var zipPath = Path.Combine(_dataDir, "Jailbreak.zip");
        await using (var fs = File.Create(zipPath)) {
          await stream.CopyToAsync(fs);
        }

        ZipFile.ExtractToDirectory(zipPath, stagingDir, overwriteFiles: true);
        File.Delete(zipPath);

        State.StagedVersion = remoteVersion;
        State.Status = "staged";
        SaveState();

        _logger.LogInformation(
          "[Updater] Version {Version} staged and ready", remoteVersion);
        return true;
      } catch (Exception ex) {
        _logger.LogError(ex, "[Updater] Error checking for updates");
        State.Status = "idle";
        SaveState();
        return false;
      }
    } finally {
      _checkLock.Release();
    }
  }

  public void ApplyUpdate(string pluginsDirectory) {
    if (State.Status != "staged") return;

    State.Status = "applying";
    SaveState();

    var stagingDir = Path.Combine(_dataDir, "staging");

    if (!Directory.Exists(stagingDir)) {
      _logger.LogWarning("[Updater] Staging directory not found");
      State.Status = "idle";
      SaveState();
      return;
    }

    _logger.LogInformation(
      "[Updater] Applying update to {Version}", State.StagedVersion);

    var updaterDir = "Jailbreak.Updater";

    foreach (var pluginDir in Directory.GetDirectories(stagingDir)) {
      var dirName = Path.GetFileName(pluginDir);

      // Skip the updater — copy it last
      if (dirName == updaterDir) continue;

      var targetDir = Path.Combine(pluginsDirectory, dirName);
      CopyDirectory(pluginDir, targetDir);
    }

    // Copy updater last (self-update)
    var stagedUpdater = Path.Combine(stagingDir, updaterDir);
    if (Directory.Exists(stagedUpdater)) {
      var targetUpdater = Path.Combine(pluginsDirectory, updaterDir);
      CopyDirectory(stagedUpdater, targetUpdater);
    }

    SaveState();

    _logger.LogInformation(
      "[Updater] Update applied. Plugins will reload on next map.");
  }

  private static bool IsNewerVersion(string remote, string installed) {
    if (Version.TryParse(NormalizeSemver(remote), out var rv)
        && Version.TryParse(NormalizeSemver(installed), out var iv)) {
      if (rv != iv) return rv > iv;
      // Base versions equal — compare full strings for pre-release ordering
      // e.g., "2.1.0-beta.2" > "2.1.0-beta.1", and "2.1.0" > "2.1.0-beta.1"
      return string.Compare(remote, installed, StringComparison.Ordinal) > 0;
    }
    return string.Compare(remote, installed, StringComparison.Ordinal) > 0;
  }

  private static string NormalizeSemver(string version) {
    var dash = version.IndexOf('-');
    return dash >= 0 ? version[..dash] : version;
  }

  private void CleanStaging() {
    var stagingDir = Path.Combine(_dataDir, "staging");
    if (Directory.Exists(stagingDir))
      Directory.Delete(stagingDir, recursive: true);
  }

  private static void CopyDirectory(string source, string target) {
    Directory.CreateDirectory(target);

    foreach (var file in Directory.GetFiles(source)) {
      var dest = Path.Combine(target, Path.GetFileName(file));
      File.Copy(file, dest, overwrite: true);
    }

    foreach (var dir in Directory.GetDirectories(source)) {
      var dest = Path.Combine(target, Path.GetFileName(dir));
      CopyDirectory(dir, dest);
    }
  }

  private UpdateState LoadState() {
    if (!File.Exists(_stateFilePath)) return new UpdateState();

    try {
      var json = File.ReadAllText(_stateFilePath);
      return JsonSerializer.Deserialize<UpdateState>(json) ?? new UpdateState();
    } catch {
      return new UpdateState();
    }
  }

  private void SaveState() {
    var json = JsonSerializer.Serialize(State, new JsonSerializerOptions {
      WriteIndented = true
    });
    File.WriteAllText(_stateFilePath, json);
  }
}
