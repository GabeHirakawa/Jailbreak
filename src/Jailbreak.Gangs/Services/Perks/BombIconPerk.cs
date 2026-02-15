using System.Diagnostics;
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
public enum BombIcon {
  ANYASMUG = 1 << 0,
  C4_RED = 1 << 1,
  CACHINGA = 1 << 2,
  CLOWN_EMOJI = 1 << 3,
  CSGO = 1 << 4,
  DOGE = 1 << 5,
  DOLLAR = 1 << 6,
  EGO = 1 << 7,
  GOAT = 1 << 8,
  IM = 1 << 9,
  IMBAD = 1 << 10,
  KZ = 1 << 11,
  OMEGALUL = 1 << 12,
  PEPEGA = 1 << 13,
  POOP = 1 << 14,
  STEAM = 1 << 15,
  THINKING = 1 << 16,
  UWUNUKE = 1 << 17,
  ZZZ = 1 << 18,
  DEFAULT = 1 << 19
}

public static class BombIconExtensions {
  public static int GetCost(this BombIcon icon) {
    return icon switch {
      BombIcon.ANYASMUG    => 1750,
      BombIcon.C4_RED      => 2000,
      BombIcon.CACHINGA    => 25000,
      BombIcon.CLOWN_EMOJI => 15000,
      BombIcon.CSGO        => 3500,
      BombIcon.DOGE        => 12500,
      BombIcon.DOLLAR      => 100000,
      BombIcon.EGO         => 7500,
      BombIcon.GOAT        => 20000,
      BombIcon.IM          => 10000,
      BombIcon.IMBAD       => 12500,
      BombIcon.KZ          => 10000,
      BombIcon.OMEGALUL    => 25000,
      BombIcon.PEPEGA      => 20000,
      BombIcon.POOP        => 5000,
      BombIcon.STEAM       => 2500,
      BombIcon.THINKING    => 7500,
      BombIcon.UWUNUKE     => 5000,
      BombIcon.ZZZ         => 10000,
      _                    => 0
    };
  }

  public static string GetEquipment(this BombIcon icon) {
    return icon.ToString().ToLower();
  }
}

public class BombPerkData {
  public BombIcon Unlocked { get; set; }
  public BombIcon Equipped { get; set; }
}

public class BombPerk(IServiceProvider provider)
  : BasePerk<BombPerkData>(provider) {
  public const string STAT_ID = "jb_bomb_icon";

  public const string DESC =
    "Customize the icon that is shown when you bomb a CT";

  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  public override string StatId => STAT_ID;
  public override string Name => "Bomb Icon";

  public override string Description => DESC;

  public override BombPerkData Value { get; set; } = new();

  public override Task<int?> GetCost(IGangPlayer player) {
    return Task.FromResult<int?>(null);
  }

  public override Task OnPurchase(IGangPlayer player) {
    throw new NotImplementedException();
  }

  public override async Task<IMenu?> GetMenu(IGangPlayer player) {
    Debug.Assert(player.GangId != null, "player.GangId != null");

    var data =
      await gangStats.GetForGang<BombPerkData>(player.GangId.Value, STAT_ID)
      ?? new BombPerkData();

    return new BombIconMenu(Provider, data);
  }
}

public class BombIconCommand(IServiceProvider provider) : ICommand {
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

  public string Name => "css_bombicon";
  public string[] Usage => ["<icon>"];

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

    var data = await gangStats.GetForGang<BombPerkData>(gang, BombPerk.STAT_ID)
      ?? new BombPerkData();

    if (info.ArgCount == 1) {
      var menu = new BombIconMenu(provider, data);
      await menus.OpenMenu(executor, menu);
      return CommandResult.SUCCESS;
    }

    BombIcon icon;
    var      query = string.Join('_', info.Args.Skip(1)).ToUpper();
    if (!int.TryParse(info[1], out var iconInt) || iconInt < 0) {
      if (!Enum.TryParse(query, out icon)) {
        info.ReplySync(localizer.Get(MSG.COMMAND_INVALID_PARAM, info[1],
          "an icon"));
        return CommandResult.SUCCESS;
      }
    } else { icon = (BombIcon)iconInt; }

    if (!data.Unlocked.HasFlag(icon)) {
      var (canPurchase, minRank) = await ranks.CheckRank(player,
        Perm.PURCHASE_PERKS);

      if (!canPurchase) {
        info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, minRank.Name));
        return CommandResult.SUCCESS;
      }

      var cost = icon.GetCost();
      if (await eco.TryPurchase(executor, cost,
        item: "Bomb Icon: " + icon.ToString().ToTitleCase()) < 0)
        return CommandResult.SUCCESS;

      data.Unlocked |= icon;
      data.Equipped =  icon;

      await gangStats.SetForGang(gang, BombPerk.STAT_ID, data);

      if (gangChat == null) return CommandResult.SUCCESS;

      await gangChat.SendGangChat(player, gang,
        localizer.Get(MSG.PERK_PURCHASED, icon.ToString()));
      return CommandResult.SUCCESS;
    }

    if (data.Equipped == icon) return CommandResult.SUCCESS;

    var (canManage, required) =
      await ranks.CheckRank(player, Perm.MANAGE_PERKS);
    if (!canManage) {
      info.ReplySync(localizer.Get(MSG.GENERIC_NOPERM_RANK, required.Name));
      return CommandResult.SUCCESS;
    }

    data.Equipped = icon;
    await gangStats.SetForGang(gang, BombPerk.STAT_ID, data);

    if (gangChat == null) return CommandResult.SUCCESS;

    await gangChat.SendGangChat(player, gang,
      localizer.Get(MSG.GANG_THING_SET, "Bomb Icon",
        icon.ToString().ToTitleCase()));
    return CommandResult.SUCCESS;
  }
}

public class BombIconMenu(IServiceProvider provider, BombPerkData data)
  : AbstractEnumMenu<BombIcon>(provider, data.Unlocked, data.Equipped,
    "css_bombicon", "Bomb Icon", BombPerk.DESC) {
  override protected int getCost(BombIcon item) { return item.GetCost(); }

  override protected List<BombIcon> getValues() {
    return Enum.GetValues<BombIcon>().ToList();
  }

  override protected string formatItem(BombIcon item) {
    return $"{item.ToString().ToTitleCase()}";
  }
}
