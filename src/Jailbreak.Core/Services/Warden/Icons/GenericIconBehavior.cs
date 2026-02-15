using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Core.Services.Stubs;

namespace Jailbreak.Core.Services.Warden.Icons;

/// <summary>
/// Base class for overhead icon behaviors.
/// Migrated from Jailbreak.Public.Mod.Warden.GenericIconBehavior.
/// Gangs icon perk integration disabled until Gangs is migrated (Task 15).
/// </summary>
public abstract class GenericIconBehavior {
  private readonly ITextSpawner? spawner;
  private readonly Color color;
  private readonly IEnumerable<CPointWorldText>?[] icons =
    new IEnumerable<CPointWorldText>?[65];

  protected GenericIconBehavior(ITextSpawner? spawner, Color color) {
    this.spawner = spawner;
    this.color = color;
  }

  public void AssignIcon(CCSPlayerController player) {
    if (spawner == null) return;
    var localSpawner = spawner;

    Task.Run(async () => {
      var icon = await getIcon(player.SteamID);

      var data = new TextSetting { msg = icon, color = color };

      await Server.NextFrameAsync(() => {
        var hat = localSpawner.CreateTextHat(data, player);
        icons[player.Slot] = hat;
      });
    });
  }

  public void RemoveIcon(CCSPlayerController player) {
    var hat = icons[player.Slot];
    if (hat == null) return;
    foreach (var text in hat) {
      if (!text.IsValid) continue;
      text.Remove();
    }

    icons[player.Slot] = null;
  }

  /// <summary>
  /// Get the icon string for the given player.
  /// Override to customize per-player icons.
  /// </summary>
  protected abstract Task<string> getIcon(ulong steam);
}
