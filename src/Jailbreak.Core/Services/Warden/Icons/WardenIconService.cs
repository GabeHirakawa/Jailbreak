using System.Drawing;
using CounterStrikeSharp.API.Core;
using Jailbreak.Core.Services.Stubs;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Core.Services.Warden.Icons;

/// <summary>
/// Manages the warden overhead icon.
/// Migrated from Jailbreak.Warden.Global.WardenIconBehavior.
/// Gangs icon perk integration disabled until Gangs is migrated (Task 15).
/// </summary>
public class WardenIconService : GenericIconBehavior, IWardenIcon {
  public WardenIconService(IServiceProvider provider)
    : base(provider.GetService<ITextSpawner>(), Color.Blue) { }

  public void AssignWardenIcon(CCSPlayerController warden) {
    AssignIcon(warden);
  }

  public void RemoveWardenIcon(CCSPlayerController warden) {
    RemoveIcon(warden);
  }

  protected override Task<string> getIcon(ulong steam) {
    // TODO: Re-enable when Gangs plugin is migrated (Task 15)
    // Return gang-specific icon based on WardenIconPerk
    return Task.FromResult("\u2605"); // Default star icon
  }
}
