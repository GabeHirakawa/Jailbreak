using System.Diagnostics;
using GangsAPI.Data;
using GangsAPI.Data.Gang;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Player;
using Jailbreak.Contracts.Extensions;
using Microsoft.Extensions.DependencyInjection;
using IMenu = GangsAPI.Services.Menu.IMenu;

namespace Jailbreak.Gangs.Services.Perks;

[Flags]
public enum WardenIcon {
  DEFAULT = 1 << 0,
  LOWER_DEFAULT = 1 << 1,
  KATA_SMILE = 1 << 2,
  DOWN_ARROW = 1 << 3,
  STAR = 1 << 4,
  KING = 1 << 5,
  QUEEN = 1 << 6,
  ROOK = 1 << 7,
  BISHOP = 1 << 8,
  KNIGHT = 1 << 9,
  PAWN = 1 << 10,
  RANDOM = 1 << 11
}

public static class WardenIconExtensions {
  private static readonly Random rng = new();

  public static string GetIcon(this WardenIcon icon) {
    return icon switch {
      WardenIcon.KING          => "\u2654",
      WardenIcon.QUEEN         => "\u2655",
      WardenIcon.ROOK          => "\u2656",
      WardenIcon.BISHOP        => "\u2657",
      WardenIcon.KNIGHT        => "\u2658",
      WardenIcon.PAWN          => "\u2659",
      WardenIcon.DEFAULT       => "W",
      WardenIcon.LOWER_DEFAULT => "w",
      WardenIcon.KATA_SMILE    => "\u30C4",
      WardenIcon.DOWN_ARROW    => "\u2193",
      WardenIcon.STAR          => "\u2605",
      WardenIcon.RANDOM        => "?",
      _                        => "W"
    };
  }

  public static int GetCost(this WardenIcon icon) {
    return icon switch {
      WardenIcon.DEFAULT       => 0,
      WardenIcon.LOWER_DEFAULT => 1500,
      WardenIcon.KATA_SMILE    => 4000,
      WardenIcon.DOWN_ARROW    => 3500,
      WardenIcon.STAR          => 6500,
      WardenIcon.KING          => 7000,
      WardenIcon.QUEEN         => 6500,
      WardenIcon.ROOK          => 5000,
      WardenIcon.BISHOP        => 2500,
      WardenIcon.KNIGHT        => 3000,
      WardenIcon.PAWN          => 4000,
      WardenIcon.RANDOM        => 10000,
      _                        => 5000
    };
  }

  public static string PickRandom(this WardenIcon icon) {
    var n = rng.Next(Enum.GetValues<WardenIcon>().Length);
    var available = Enum.GetValues<WardenIcon>()
     .Where(c => icon.HasFlag(c) && c != WardenIcon.RANDOM)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    return available.Count == 0 ?
      WardenIcon.DEFAULT.GetIcon() :
      available[n % available.Count].GetIcon();
  }
}

public class WardenIconPerk(IServiceProvider provider)
  : BasePerk<WardenIcon>(provider) {
  public const string STAT_ID = "jb_wardenicon";

  public const string DESC =
    "Change the icon that appears above your head as warden";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "Warden Icon";

  public override string Description => DESC;

  public override WardenIcon Value { get; set; } = WardenIcon.DEFAULT;

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var data =
      await gangStats.GetForGang<WardenIcon>(player.GangId.Value, STAT_ID);
    var equipped =
      await playerStats.GetForPlayer<WardenIcon>(player.Steam, STAT_ID);
    return new WardenIconMenu(Provider, data, equipped);
  }
}

public class WardenIconCommand(IServiceProvider provider)
  : AbstractEnumCommand<WardenIcon>(provider, WardenIconPerk.STAT_ID,
    WardenIcon.DEFAULT, "Warden Icon") {
  public override string Name => "css_wardenicon";

  override protected void openMenu(PlayerWrapper player, WardenIcon data,
    WardenIcon equipped) {
    var menu = new WardenIconMenu(Provider, data, equipped);
    Menus.OpenMenu(player, menu);
  }

  override protected int getCost(WardenIcon item) { return item.GetCost(); }
}

public class WardenIconMenu(IServiceProvider provider, WardenIcon data,
  WardenIcon equipped) : AbstractEnumMenu<WardenIcon>(provider, data, equipped,
  "css_wardenicon", "Warden Icon", WardenIconPerk.DESC) {
  override protected int getCost(WardenIcon item) { return item.GetCost(); }

  override protected List<WardenIcon> getValues() {
    return Enum.GetValues<WardenIcon>().ToList();
  }

  override protected string formatItem(WardenIcon item) {
    return $"{item.GetIcon()} ({item.ToString().ToTitleCase()})";
  }
}
