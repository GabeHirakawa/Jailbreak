using System.Diagnostics;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI;
using GangsAPI.Data;
using GangsAPI.Data.Command;
using GangsAPI.Data.Gang;
using GangsAPI.Exceptions;
using GangsAPI.Extensions;
using GangsAPI.Perks;
using GangsAPI.Permissions;
using GangsAPI.Services;
using GangsAPI.Services.Commands;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using GangsAPI.Services.Player;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Jailbreak.Gangs.Services.Perks;

[Flags]
public enum SDColor {
  RED = 1 << 0,
  ORANGE = 1 << 1,
  YELLOW = 1 << 2,
  GREEN = 1 << 3,
  CYAN = 1 << 4,
  BLUE = 1 << 5,
  PURPLE = 1 << 6,
  DEFAULT = 1 << 7,
  RANDOM = 1 << 8,
  RAINBOW = 1 << 9
}

public static class SDColorExtensions {
  public static int GetCost(this SDColor color) {
    if (color == SDColor.RAINBOW) return 10 * 5000;
    if (color == SDColor.DEFAULT) return 0;
    return (int)Math.Round(color.GetColor().GetColorMultiplier() * 5000);
  }

  public static Color? GetColor(this SDColor color) {
    return color switch {
      SDColor.RED     => Color.Red,
      SDColor.ORANGE  => Color.Orange,
      SDColor.YELLOW  => Color.Yellow,
      SDColor.GREEN   => Color.Green,
      SDColor.CYAN    => Color.Cyan,
      SDColor.BLUE    => Color.Blue,
      SDColor.PURPLE  => Color.Purple,
      SDColor.DEFAULT => null,
      SDColor.RANDOM  => null,
      _               => Color.White
    };
  }

  public static Color? PickRandom(this SDColor color) {
    var n = new Random().Next(Enum.GetValues<SDColor>().Length);
    var available = Enum.GetValues<SDColor>()
     .Where(c => color.HasFlag(c) && c.GetColor() != null)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    if (available.Count == 0) return null;

    return available[n % available.Count].GetColor();
  }
}

public class SDColorData {
  public SDColor Unlocked { get; set; }
  public SDColor Equipped { get; set; }
}

public class SDColorPerk(IServiceProvider provider)
  : BasePerk<SDColorData>(provider) {
  public const string STAT_ID = "jb_sd_color";

  public const string DESC =
    "Change the color of your gang during special days!";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "Special Day Color";

  public override string Description => DESC;

  public override SDColorData Value { get; set; } = new();

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");
    var data =
      await gangStats.GetForGang<SDColorData>(player.GangId.Value, STAT_ID)
      ?? new SDColorData();
    return new SDColorMenu(Provider, data);
  }

  public override Task OnPurchase(IGangPlayer player) {
    return Task.CompletedTask;
  }
}

public class SDColorCommand(IServiceProvider provider) : ICommand {
  private readonly ICommandManager commands =
    provider.GetRequiredService<ICommandManager>();

  private readonly IEcoManager eco = provider.GetRequiredService<IEcoManager>();

  private readonly IGangChatPerk? gangChat =
    provider.GetService<IGangChatPerk>();

  private readonly IGangManager gangs =
    provider.GetRequiredService<IGangManager>();

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IStringLocalizer localizer =
    provider.GetRequiredService<IStringLocalizer>();

  private readonly IMenuManager menus =
    provider.GetRequiredService<IMenuManager>();

  private readonly IPlayerManager players =
    provider.GetRequiredService<IPlayerManager>();

  private readonly IRankManager ranks =
    provider.GetRequiredService<IRankManager>();

  public string Name => "css_sdcolor";
  public string[] Usage => ["<color>"];

  public void Start() { commands.RegisterCommand(this); }

  public async Task<CommandResult> Execute(PlayerWrapper? executor,
    CommandInfoWrapper info) {
    if (executor == null) return CommandResult.PLAYER_ONLY;
    var player = await players.GetPlayer(executor.Steam)
      ?? throw new PlayerNotFoundException(executor.Steam);
    if (player.GangId == null) {
      info.ReplySync(localizer.Get(MSG.NOT_IN_GANG));
      return CommandResult.SUCCESS;
    }

    var gang = await gangs.GetGang(player.GangId.Value)
      ?? throw new GangNotFoundException(player.GangId.Value);

    var data =
      await gangStats.GetForGang<SDColorData>(gang, SDColorPerk.STAT_ID)
      ?? new SDColorData();

    if (info.ArgCount == 1) {
      var menu = new SDColorMenu(provider, data);
      await menus.OpenMenu(executor, menu);
      return CommandResult.SUCCESS;
    }

    SDColor color;
    var     query = string.Join('_', info.Args.Skip(1)).ToUpper();
    if (!int.TryParse(info[1], out var iconInt) || iconInt < 0) {
      if (!Enum.TryParse(query, out color)) {
        info.ReplySync(localizer.Get(MSG.COMMAND_INVALID_PARAM, info[1],
          "a positive integer"));
        return CommandResult.SUCCESS;
      }
    } else { color = (SDColor)iconInt; }

    if (!data.Unlocked.HasFlag(color)) {
      var (canPurchase, minRank) = await ranks.CheckRank(player,
        Perm.PURCHASE_PERKS);

      if (!canPurchase) {
        info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, minRank.Name));
        return CommandResult.SUCCESS;
      }

      var cost = color.GetCost();
      if (await eco.TryPurchase(executor, cost,
        item: "Special Day Color: " + color.ToString().ToTitleCase()) < 0)
        return CommandResult.SUCCESS;

      data.Unlocked |= color;
      data.Equipped =  color;

      await gangStats.SetForGang(gang, SDColorPerk.STAT_ID, data);

      if (gangChat == null) return CommandResult.SUCCESS;

      await gangChat.SendGangChat(player, gang,
        localizer.Get(MSG.PERK_PURCHASED, color.ToString()));
      return CommandResult.SUCCESS;
    }

    if (data.Equipped == color) return CommandResult.SUCCESS;

    var (canManage, required) =
      await ranks.CheckRank(player, Perm.MANAGE_PERKS);
    if (!canManage) {
      info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, required.Name));
      return CommandResult.SUCCESS;
    }

    data.Equipped = color;
    await gangStats.SetForGang(gang, SDColorPerk.STAT_ID, data);

    if (gangChat == null) return CommandResult.SUCCESS;

    await gangChat.SendGangChat(player, gang,
      localizer.Get(MSG.GANG_THING_SET, "SD Color",
        color.GetColor().GetChatColor() + color.ToString().ToTitleCase()
        + ChatColors.Grey));
    return CommandResult.SUCCESS;
  }
}

public class SDColorMenu(IServiceProvider provider, SDColorData data)
  : AbstractEnumMenu<SDColor>(provider, data.Unlocked, data.Equipped,
    "css_sdcolor", "SD Color", SDColorPerk.DESC) {
  // TODO: IRainbowColorizer.RAINBOW was from old Jailbreak.Public, not available in new architecture
  private const string RAINBOW = "\u2726 Rainbow \u2726";

  override protected int getCost(SDColor item) { return item.GetCost(); }

  override protected List<SDColor> getValues() {
    return Enum.GetValues<SDColor>().ToList();
  }

  override protected string formatItem(SDColor item) {
    if (item == SDColor.RAINBOW) return RAINBOW;
    return
      $"{item.GetColor().GetChatColor()}{item.ToString().ToTitleCase()}{ChatColors.Grey}";
  }
}
