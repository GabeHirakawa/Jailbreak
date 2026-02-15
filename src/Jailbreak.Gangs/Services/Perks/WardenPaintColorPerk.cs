using System.Diagnostics;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using GangsAPI.Services.Player;
using Jailbreak.Contracts.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Gangs.Services.Perks;

[Flags]
public enum WardenPaintColor {
  DEFAULT = 1 << 0,
  RED = 1 << 1,
  ORANGE = 1 << 2,
  YELLOW = 1 << 3,
  GREEN = 1 << 4,
  CYAN = 1 << 5,
  BLUE = 1 << 6,
  PURPLE = 1 << 7,
  RANDOM = 1 << 8,
  RAINBOW = 1 << 9
}

public static class WardenColorExtensions {
  private static readonly Random rng = new();

  public static Color? GetColor(this WardenPaintColor paintColor) {
    return paintColor switch {
      WardenPaintColor.RED     => Color.Red,
      WardenPaintColor.ORANGE  => Color.Orange,
      WardenPaintColor.YELLOW  => Color.Yellow,
      WardenPaintColor.GREEN   => Color.Green,
      WardenPaintColor.CYAN    => Color.Cyan,
      WardenPaintColor.BLUE    => Color.Blue,
      WardenPaintColor.PURPLE  => Color.Purple,
      WardenPaintColor.DEFAULT => null,
      WardenPaintColor.RANDOM  => null,
      _                        => Color.White
    };
  }

  public static int GetCost(this WardenPaintColor paintColor) {
    if (paintColor == WardenPaintColor.RAINBOW) return 10 * 7500;
    if (paintColor == WardenPaintColor.DEFAULT) return 0;
    return (int)Math.Round(paintColor.GetColor().GetColorMultiplier() * 7500);
  }

  public static Color? PickRandom(this WardenPaintColor paintColor) {
    var n = rng.Next(Enum.GetValues<WardenPaintColor>().Length);
    var available = Enum.GetValues<WardenPaintColor>()
     .Where(c => paintColor.HasFlag(c) && c.GetColor() != null)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    if (available.Count == 0) return null;

    return available[n % available.Count].GetColor();
  }
}

public class WardenPaintColorPerk(IServiceProvider provider)
  : BasePerk<WardenPaintColor>(provider) {
  public const string STAT_ID = "jb_wardenpaintcolor";
  public const string DESC = "Change the color of your warden paint!";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "Paint Color";
  public override string Description => DESC;

  public override WardenPaintColor Value { get; set; }

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var data =
      await gangStats.GetForGang<WardenPaintColor>(player.GangId.Value,
        STAT_ID);
    var equipped =
      await playerStats.GetForPlayer<WardenPaintColor>(player.Steam, STAT_ID);
    return new WardenPaintColorMenu(Provider, data, equipped);
  }
}

public class WardenColorCommand(IServiceProvider provider)
  : AbstractEnumCommand<WardenPaintColor>(provider,
    WardenPaintColorPerk.STAT_ID, WardenPaintColor.DEFAULT, "Paint Color") {
  // TODO: IRainbowColorizer.RAINBOW was from old Jailbreak.Public, not available in new architecture
  private const string RAINBOW = "\u2726 Rainbow \u2726";

  public override string Name => "css_paint";

  override protected void openMenu(PlayerWrapper player, WardenPaintColor data,
    WardenPaintColor equipped) {
    var menu = new WardenPaintColorMenu(Provider, data, equipped);
    Menus.OpenMenu(player, menu);
  }

  override protected int getCost(WardenPaintColor item) {
    return item.GetCost();
  }

  override protected string formatItem(WardenPaintColor item) {
    if (item == WardenPaintColor.RAINBOW) return RAINBOW;
    return
      $"{item.GetColor().GetChatColor()}{item.ToString().ToTitleCase()}{ChatColors.Grey}";
  }
}

public class WardenPaintColorMenu(IServiceProvider provider,
  WardenPaintColor data, WardenPaintColor equipped)
  : AbstractEnumMenu<WardenPaintColor>(provider, data, equipped, "css_paint",
    "Paint Color", WardenPaintColorPerk.DESC) {
  // TODO: IRainbowColorizer.RAINBOW was from old Jailbreak.Public, not available in new architecture
  private const string RAINBOW = "\u2726 Rainbow \u2726";

  override protected int getCost(WardenPaintColor item) {
    return item.GetCost();
  }

  override protected List<WardenPaintColor> getValues() {
    return Enum.GetValues<WardenPaintColor>().ToList();
  }

  override protected string formatItem(WardenPaintColor item) {
    if (item == WardenPaintColor.RAINBOW) return RAINBOW;
    return
      $"{item.GetColor().GetChatColor()}{item.ToString().ToTitleCase()}{ChatColors.Grey}";
  }
}
