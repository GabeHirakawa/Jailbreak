using System.Drawing;
using CounterStrikeSharp.API.Core;
using Jailbreak.Core.Services.Stubs;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Core.Services.Warden.Icons;

/// <summary>
/// Manages the special treatment overhead icon.
/// Migrated from Jailbreak.Warden.Global.SpecialIconBehavior.
/// Gangs icon perk integration disabled until Gangs is migrated (Task 15).
/// </summary>
public class SpecialIconService : GenericIconBehavior, ISpecialIcon {
  public SpecialIconService(IServiceProvider provider)
    : base(provider.GetService<ITextSpawner>(), Color.Green) { }

  public void AssignSpecialIcon(CCSPlayerController player) {
    AssignIcon(player);
  }

  public void RemoveSpecialIcon(CCSPlayerController player) {
    RemoveIcon(player);
  }

  protected override Task<string> getIcon(ulong steam) {
    // TODO: Re-enable when Gangs plugin is migrated (Task 15)
    // Return gang-specific icon based on SpecialIconPerk
    return Task.FromResult("\u2764"); // Default heart icon
  }
}
