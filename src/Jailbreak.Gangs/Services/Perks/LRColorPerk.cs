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
public enum LRColor {
  ORANGE = 1 << 0,
  YELLOW = 1 << 1,
  GREEN = 1 << 2,
  CYAN = 1 << 3,
  BLUE = 1 << 4,
  PURPLE = 1 << 5,
  DEFAULT = 1 << 6,
  RANDOM = 1 << 7,
  RAINBOW = 1 << 8
}

public static class LRColorExtensions {
  private static readonly Random rng = new();

  public static int GetCost(this LRColor color) {
    if (color == LRColor.RAINBOW) return 10 * 8000;
    if (color == LRColor.DEFAULT) return 0;
    return (int)Math.Round(color.GetColor().GetColorMultiplier() * 8000);
  }

  public static Color? GetColor(this LRColor color) {
    return color switch {
      LRColor.ORANGE  => Color.Orange,
      LRColor.YELLOW  => Color.Yellow,
      LRColor.GREEN   => Color.Green,
      LRColor.CYAN    => Color.Cyan,
      LRColor.BLUE    => Color.Blue,
      LRColor.PURPLE  => Color.Purple,
      LRColor.DEFAULT => null,
      LRColor.RANDOM  => null,
      _               => Color.White
    };
  }

  public static Color? PickRandomColor(this LRColor color) {
    var n = rng.Next(Enum.GetValues<LRColor>().Length);
    var available = Enum.GetValues<LRColor>()
     .Where(c => color.HasFlag(c) && c.GetColor() != null)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    if (available.Count == 0) return null;

    return available[n % available.Count].GetColor();
  }
}

public class LRColorPerk(IServiceProvider provider)
  : BasePerk<LRColor>(provider) {
  public const string STAT_ID = "jb_lr_color";

  public const string DESC =
    "Pick the color of you and your partner during your LRs\\nConflicting colors are resolved by gang rank";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "LR Colors";

  public override string Description => DESC;

  public override LRColor Value { get; set; } = LRColor.DEFAULT;

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var data =
      await gangStats.GetForGang<LRColor>(player.GangId.Value, STAT_ID);
    var equipped =
      await playerStats.GetForPlayer<LRColor>(player.Steam, STAT_ID);
    return new LRColorMenu(Provider, data, equipped);
  }
}

public class LRColorCommand(IServiceProvider provider)
  : AbstractEnumCommand<LRColor>(provider, LRColorPerk.STAT_ID, LRColor.DEFAULT,
    "LR Color") {
  // TODO: IRainbowColorizer.RAINBOW was from old Jailbreak.Public, not available in new architecture
  private const string RAINBOW = "\u2726 Rainbow \u2726";

  public override string Name => "css_lrcolor";

  override protected void openMenu(PlayerWrapper player, LRColor data,
    LRColor equipped) {
    var menu = new LRColorMenu(Provider, data, equipped);
    Menus.OpenMenu(player, menu);
  }

  override protected int getCost(LRColor item) { return item.GetCost(); }

  override protected string formatItem(LRColor item) {
    var result = item.GetColor().GetChatColor().ToString();
    if (item == LRColor.RAINBOW) return RAINBOW;
    if (item.GetColor() == null) return result + "Random";
    return $"{result}{item.GetColor()!.Value.Name}{ChatColors.Grey}";
  }
}

public class LRColorMenu(IServiceProvider provider, LRColor data,
  LRColor equipped) : AbstractEnumMenu<LRColor>(provider, data, equipped,
  "css_lrcolor", "LR Color", LRColorPerk.DESC) {
  // TODO: IRainbowColorizer.RAINBOW was from old Jailbreak.Public, not available in new architecture
  private const string RAINBOW = "\u2726 Rainbow \u2726";

  override protected int getCost(LRColor item) { return item.GetCost(); }

  override protected List<LRColor> getValues() {
    return Enum.GetValues<LRColor>().ToList();
  }

  override protected string formatItem(LRColor item) {
    var result = item.GetColor().GetChatColor().ToString();
    if (item == LRColor.RAINBOW) return RAINBOW;
    if (item.GetColor() == null) return result + "Random";
    return $"{result}{item.GetColor()!.Value.Name}{ChatColors.Grey}";
  }
}
