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
public enum SpecialIcon {
  DEFAULT = 1 << 0,
  LOWER_DEFAULT = 1 << 1,
  CIRCLE = 1 << 2,
  SQUARE = 1 << 3,
  TRIANGLE = 1 << 4,
  ASTERISK = 1 << 5,
  DIAMOND = 1 << 6,
  HEART = 1 << 7,
  CLUB = 1 << 8,
  SPADE = 1 << 9,
  RANDOM = 1 << 10
}

public static class SpecialIconExtensions {
  private static readonly Random rng = new();

  public static string GetIcon(this SpecialIcon icon) {
    return icon switch {
      SpecialIcon.DEFAULT       => "S",
      SpecialIcon.LOWER_DEFAULT => "s",
      SpecialIcon.CIRCLE        => "\u2B24",
      SpecialIcon.SQUARE        => "\u25A0",
      SpecialIcon.TRIANGLE      => "\u25B2",
      SpecialIcon.ASTERISK      => "\u2738",
      SpecialIcon.DIAMOND       => "\u2666",
      SpecialIcon.HEART         => "\u2665",
      SpecialIcon.CLUB          => "\u2663",
      SpecialIcon.SPADE         => "\u2660",
      SpecialIcon.RANDOM        => "?",
      _                         => "S"
    };
  }

  public static int GetCost(this SpecialIcon icon) {
    return icon switch {
      SpecialIcon.DEFAULT       => 0,
      SpecialIcon.LOWER_DEFAULT => 1250,
      SpecialIcon.CIRCLE        => 1000,
      SpecialIcon.SQUARE        => 1200,
      SpecialIcon.TRIANGLE      => 1300,
      SpecialIcon.ASTERISK      => 4000,
      SpecialIcon.DIAMOND       => 3000,
      SpecialIcon.HEART         => 3000,
      SpecialIcon.CLUB          => 3000,
      SpecialIcon.SPADE         => 2500,
      SpecialIcon.RANDOM        => 10000,
      _                         => 3000
    };
  }

  public static string PickRandom(this SpecialIcon icon) {
    var n = rng.Next(Enum.GetValues<SpecialIcon>().Length);
    var available = Enum.GetValues<SpecialIcon>()
     .Where(c => icon.HasFlag(c) && c != SpecialIcon.RANDOM)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    return available.Count == 0 ?
      SpecialIcon.DEFAULT.GetIcon() :
      available[n % available.Count].GetIcon();
  }
}

public class SpecialIconPerk(IServiceProvider provider)
  : BasePerk<SpecialIcon>(provider) {
  public const string STAT_ID = "jb_specialicon";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IPlayerStatManager playerStats =
    provider.GetRequiredService<IPlayerStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "ST Icon";

  public override string Description
    => "Change the icon that appears above your head as ST";

  public override SpecialIcon Value { get; set; } = SpecialIcon.DEFAULT;

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var data =
      await gangStats.GetForGang<SpecialIcon>(player.GangId.Value, STAT_ID);
    var equipped =
      await playerStats.GetForPlayer<SpecialIcon>(player.Steam, STAT_ID);
    return new SpecialIconMenu(Provider, data, equipped);
  }
}

public class SpecialIconCommand(IServiceProvider provider)
  : AbstractEnumCommand<SpecialIcon>(provider, SpecialIconPerk.STAT_ID,
    SpecialIcon.DEFAULT, "ST Icon") {
  public override string Name => "css_sticon";

  override protected void openMenu(PlayerWrapper player, SpecialIcon data,
    SpecialIcon equipped) {
    var menu = new SpecialIconMenu(Provider, data, equipped);
    Menus.OpenMenu(player, menu);
  }

  override protected int getCost(SpecialIcon item) { return item.GetCost(); }

  override protected string formatItem(SpecialIcon item) {
    return $"{item.GetIcon()} ({item.ToString().ToTitleCase()})";
  }
}

public class SpecialIconMenu(IServiceProvider provider, SpecialIcon data,
  SpecialIcon equipped) : AbstractEnumMenu<SpecialIcon>(provider, data,
  equipped, "css_sticon", "ST Icon",
  "Change the icon that appears above your head as ST") {
  override protected int getCost(SpecialIcon item) { return item.GetCost(); }

  override protected List<SpecialIcon> getValues() {
    return Enum.GetValues<SpecialIcon>().ToList();
  }

  override protected string formatItem(SpecialIcon item) {
    return $"{item.GetIcon()} ({item.ToString().ToTitleCase()})";
  }
}
