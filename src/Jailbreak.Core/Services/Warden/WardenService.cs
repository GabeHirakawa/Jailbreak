using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Contracts.Utils;
using Jailbreak.Core.Services.Stubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Core.Services.Warden;

// By making it a struct we ensure values from the CCSPlayerPawn are passed by VALUE.
public struct PreWardenStats(int armorValue, int health, int maxHealth,
  bool headHealthShot, bool hadHelmetArmor) {
  public readonly int ArmorValue = armorValue;
  public readonly int Health = health;
  public readonly int MaxHealth = maxHealth;
  public readonly bool HeadHealthShot = headHealthShot;
  public readonly bool HadHelmetArmor = hadHelmetArmor;
}

public class WardenService : IWardenService {
  public static readonly FakeConVar<int> CV_ARMOR_EQUAL = new("css_jb_hp_equal",
    "Health points for when CTs have equal ratio", 50, ConVarFlags.FCVAR_NONE,
    new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_ARMOR_OUTNUMBER =
    new("css_jb_hp_outnumber", "HP for CTs when outnumbering Ts", 25,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_ARMOR_OUTNUMBERED =
    new("css_jb_hp_outnumbered", "Health points for CTs when outnumbered by Ts",
      100, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_WARDEN_ARMOR =
    new("css_jb_warden_armor", "Armor for the warden", 125,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_WARDEN_AUTO_OPEN_CELLS =
    new("css_jb_warden_opencells_delay",
      "Delay in seconds to auto-open cells at, -1 to disable", 60);

  public static readonly FakeConVar<bool> CV_WARDEN_AUTO_SNITCH =
    new("css_jb_warden_auto_snitch",
      "True to broadcast how many prisoners were in cells when they auto-open",
      false);

  public static readonly FakeConVar<int> CV_WARDEN_HEALTH =
    new("css_jb_warden_hp", "HP for the warden", 125, ConVarFlags.FCVAR_NONE,
      new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<int> CV_WARDEN_MAX_HEALTH =
    new("css_jb_warden_maxhp", "Max HP for the warden", 100,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 200));

  public static readonly FakeConVar<string> CV_WARDEN_SOUND_KILLED =
    new("css_jb_warden_sound_killed", "Sound to play when the warden is killed",
      "wardenKilled");

  public static readonly FakeConVar<string> CV_WARDEN_SOUND_PASSED =
    new("css_jb_warden_sound_passed", "Sound to play when the warden passes",
      "wardenPassed");

  public static readonly FakeConVar<string> CV_WARDEN_SOUND_NEW =
    new("css_jb_warden_sound_new",
      "Sound to play when the warden is assigned", "wardenNew");

  public static readonly FakeConVar<int> CV_WARDEN_TERRORIST_RATIO =
    new("css_jb_warden_t_ratio", "Ratio of T:CT to use for HP adjustments", 3);

  private readonly ISet<CCSPlayerController> bluePrisoners =
    new HashSet<CCSPlayerController>();

  private readonly ILogger<WardenService> logger;
  private readonly IWardenLocale locale;
  private readonly IWardenMarkerSettings markerSettings;
  private readonly ISpecialTreatmentService specialTreatment;
  private readonly IMuteService mute;
  private readonly IRebelService rebels;
  private readonly ISpecialDayManager specialDays;
  private readonly IServiceProvider provider;
  private readonly IWardenIcon? iconer;

  private bool firstWarden;
  private string? oldTag;
  private char? oldTagColor;

  private BasePlugin parent = null!;
  private PreWardenStats? preWardenStats;
  private Timer? unblueTimer, openCellsTimer, passFreedayTimer;

  public WardenService(
    ILogger<WardenService> logger,
    IWardenLocale locale,
    IWardenMarkerSettings markerSettings,
    ISpecialTreatmentService specialTreatment,
    IMuteService mute,
    IRebelService rebels,
    ISpecialDayManager specialDays,
    IServiceProvider provider) {
    this.logger = logger;
    this.locale = locale;
    this.markerSettings = markerSettings;
    this.specialTreatment = specialTreatment;
    this.mute = mute;
    this.rebels = rebels;
    this.specialDays = specialDays;
    this.provider = provider;
    this.iconer = provider.GetService<IWardenIcon>();
  }

  /// <summary>
  /// Initialize the service with a reference to the plugin for timers.
  /// Called from CorePlugin.Load().
  /// </summary>
  public void Initialize(BasePlugin basePlugin) {
    parent = basePlugin;
    // TODO: Re-enable when Gangs plugin is migrated (Task 15)
    // Register WardenStat with stat manager
  }

  /// <summary>
  /// Get the current warden, if there is one.
  /// </summary>
  public CCSPlayerController? Warden { get; private set; }

  /// <summary>
  /// Whether or not a warden is currently assigned.
  /// </summary>
  public bool HasWarden { get; private set; }

  public bool IsWarden(CCSPlayerController player) {
    if (!player.IsReal()) return false;
    return HasWarden && Warden != null && Warden.Slot == player.Slot;
  }

  public bool TrySetWarden(CCSPlayerController controller) {
    if (HasWarden) return false;

    // Verify player is a CT
    if (controller.Team != CsTeam.CounterTerrorist) return false;
    if (!controller.PawnIsAlive) return false;

    mute.UnPeaceMute();

    HasWarden = true;
    Warden = controller;
    Warden.SetColor(Color.Blue);

    locale.NewWarden(Warden).ToAllChat().ToAllCenter();

    Warden.Clan = "[WARDEN]";
    Utilities.SetStateChanged(Warden, "CCSPlayerController", "m_szClan");
    var ev = new EventNextlevelChanged(true);
    ev.FireEvent(false);

    // TODO: Re-enable when Gangs plugin is migrated (Task 15)
    // API.Stats?.PushStat(...)
    // API.Actain tag handling
    // Gangs stat tracking

    iconer?.AssignWardenIcon(Warden);

    markerSettings.EnsureCachedAsync(Warden.SteamID);

    foreach (var player in Utilities.GetPlayers())
      player.ExecuteClientCommand($"play sounds/{CV_WARDEN_SOUND_NEW.Value}");

    // TODO: Re-enable when logging is migrated (Task 9)
    // logs.Append(logs.Player(Warden), "is now the warden.");

    unblueTimer = parent.AddTimer(3, unmarkPrisonersBlue);
    passFreedayTimer
      ?.Kill(); // If a warden is assigned, cancel the 10s freeday timer on pass
    mute.PeaceMute(firstWarden ?
      MuteReason.INITIAL_WARDEN :
      MuteReason.WARDEN_TAKEN);

    // Always store the stats of the warden b4 they became warden
    var wardenPawn = Warden.PlayerPawn.Value;
    if (wardenPawn == null) return false;

    if (firstWarden) {
      firstWarden = false;

      var hasHealthshot = playerHasHealthshot(Warden);
      var hasHelmet = playerHasHelmetArmor(Warden);
      preWardenStats = new PreWardenStats(wardenPawn.ArmorValue,
        wardenPawn.Health, wardenPawn.MaxHealth, hasHealthshot, hasHelmet);

      if (!hasHelmet) Warden.GiveNamedItem("item_assaultsuit");

      var ctArmorValue = getBalance() switch {
        0  => CV_ARMOR_EQUAL.Value,
        1  => CV_ARMOR_OUTNUMBERED.Value,
        -1 => CV_ARMOR_OUTNUMBER.Value,
        _  => CV_ARMOR_EQUAL.Value
      };

      /* Round start CT buff */
      foreach (var guardController in Utilities.GetPlayers()
       .Where(p => p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true })) {
        var guardPawn = guardController.PlayerPawn.Value;
        if (guardPawn == null) continue;

        guardPawn.ArmorValue = ctArmorValue < guardPawn.ArmorValue ?
          guardPawn.ArmorValue :
          ctArmorValue;
        Utilities.SetStateChanged(guardPawn, "CCSPlayerPawn", "m_ArmorValue");
      }

      setWardenStats(wardenPawn, CV_WARDEN_ARMOR.Value, CV_WARDEN_HEALTH.Value,
        CV_WARDEN_MAX_HEALTH.Value);
      if (!hasHealthshot) Warden.GiveNamedItem("weapon_healthshot");
    } else { preWardenStats = null; }

    return true;
  }

  public bool TryRemoveWarden(bool isPass = false) {
    if (!HasWarden) return false;

    mute.UnPeaceMute();

    HasWarden = false;

    if (isPass) {
      passFreedayTimer =
        parent.AddTimer(10, () => { locale.NowFreeday.ToAllChat(); });
    }

    if (Warden != null && Warden.Pawn.Value != null) {
      Warden.Clan = "";
      Warden.SetColor(Color.White);
      Utilities.SetStateChanged(Warden, "CCSPlayerController", "m_szClan");
      var ev = new EventNextlevelChanged(true);
      ev.FireEvent(false);

      // TODO: Re-enable when Gangs/Actain is migrated
      // oldTag / oldTagColor restoration
      // Gangs death stats

      // TODO: Re-enable when logging is migrated (Task 9)
      // logs.Append(logs.Player(Warden), "is no longer the warden.");

      iconer?.RemoveWardenIcon(Warden);
    }

    var wardenPawn = Warden!.PlayerPawn.Value;
    if (wardenPawn == null) return false;

    if (isPass && preWardenStats != null) {
      setWardenStats(wardenPawn,
        Math.Min(wardenPawn.ArmorValue, preWardenStats.Value.ArmorValue),
        Math.Min(wardenPawn.Health, preWardenStats.Value.Health),
        Math.Min(wardenPawn.MaxHealth, preWardenStats.Value.MaxHealth));

      var itemServices = itemServicesOrNull(Warden);
      if (itemServices == null) return false;

      if (!preWardenStats.Value.HadHelmetArmor) itemServices.HasHelmet = false;

      Utilities.SetStateChanged(wardenPawn, "CBasePlayerPawn",
        "m_pItemServices");

      if (!preWardenStats.Value.HeadHealthShot)
        playerHasHealthshot(Warden, true);
    }

    Warden = null;
    return true;
  }

  [GameEventHandler]
  public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info) {
    var player = ev.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;
    if (player.Team != CsTeam.CounterTerrorist) return HookResult.Continue;
    var isWarden = IsWarden(player);

    // TODO: Re-enable when Gangs plugin is migrated (Task 15)
    // Gangs kill/death stat tracking

    if (!isWarden) return HookResult.Continue;

    // TODO: Re-enable when stats/logging is migrated
    // API.Stats?.PushStat(new ServerStat("JB_WARDEN_DEATH"));

    mute.UnPeaceMute();
    processWardenDeath();
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnChangeTeam(EventPlayerTeam @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;

    // TODO: Re-enable when Actain is migrated
    // Clear [WARDEN] tag if player changes team

    if (!IsWarden(player)) return HookResult.Continue;

    mute.UnPeaceMute();
    processWardenDeath();
    return HookResult.Continue;
  }

  private void processWardenDeath() {
    if (!TryRemoveWarden())
      logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");

    locale.WardenDied.ToAllChat().ToAllCenter();

    foreach (var player in Utilities.GetPlayers()) {
      if (!player.IsReal()) continue;
      player.ExecuteClientCommand(
        $"play sounds/{CV_WARDEN_SOUND_KILLED.Value}");
    }

    unblueTimer?.Kill();
    markPrisonersBlue();
  }

  private void unmarkPrisonersBlue() {
    foreach (var player in bluePrisoners) {
      if (!player.IsReal()) continue;
      if (ignoreColor(player)) continue;
      player.SetColor(Color.White);
    }

    bluePrisoners.Clear();
  }

  private void markPrisonersBlue() {
    foreach (var player in Utilities.GetPlayers()) {
      if (!player.IsReal() || player.Team != CsTeam.Terrorist) continue;
      if (ignoreColor(player)) continue;

      player.SetColor(Color.Blue);

      bluePrisoners.Add(player);
    }
  }

  private bool ignoreColor(CCSPlayerController player) {
    if (specialTreatment.IsSpecialTreatment(player)) return true;
    if (rebels.IsRebel(player)) return true;
    return false;
  }

  private int getBalance() {
    var ctCount = Utilities.GetPlayers()
     .Count(p => p.Team == CsTeam.CounterTerrorist);
    var tCount = Utilities.GetPlayers().Count(p => p.Team == CsTeam.Terrorist);

    var ratio = (float)tCount / CV_WARDEN_TERRORIST_RATIO.Value - ctCount;

    return ratio switch {
      > 0 => 1,
      0   => 0,
      _   => -1
    };
  }

  private CCSPlayer_ItemServices?
    itemServicesOrNull(CCSPlayerController player) {
    var itemServices = player.PlayerPawn.Value?.ItemServices;
    return itemServices != null ?
      new CCSPlayer_ItemServices(itemServices.Handle) :
      null;
  }

  private void setWardenStats(CCSPlayerPawn wardenPawn, int armor = -1,
    int health = -1, int maxHealth = -1) {
    if (armor != -1) {
      wardenPawn.ArmorValue = armor;
      Utilities.SetStateChanged(wardenPawn, "CCSPlayerPawn", "m_ArmorValue");
    }

    if (health != -1) {
      wardenPawn.Health = health;
      Utilities.SetStateChanged(wardenPawn, "CBaseEntity", "m_iHealth");
    }

    if (maxHealth != -1) {
      wardenPawn.MaxHealth = maxHealth;
      Utilities.SetStateChanged(wardenPawn, "CBaseEntity", "m_iMaxHealth");
    }
  }

  private bool playerHasHelmetArmor(CCSPlayerController player) {
    var itemServices = itemServicesOrNull(player);
    return itemServices is { HasHelmet: true };
  }

  private bool playerHasHealthshot(CCSPlayerController player,
    bool removeIfHas = false) {
    var playerPawn = player.PlayerPawn.Value;
    if (playerPawn == null || playerPawn.WeaponServices == null) return false;

    foreach (var weapon in playerPawn.WeaponServices.MyWeapons) {
      if (weapon.Value == null) continue;
      if (weapon.Value.DesignerName.Equals("weapon_healthshot")) {
        if (removeIfHas) weapon.Value.Remove();

        return true;
      }
    }

    return false;
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info) {
    TryRemoveWarden();
    mute.UnPeaceMute();
    openCellsTimer?.Kill();
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    firstWarden = true;
    preWardenStats = null;

    if (CV_WARDEN_AUTO_OPEN_CELLS.Value < 0 || RoundUtil.IsWarmup())
      return HookResult.Continue;
    var openCmd = provider.GetService<IWardenOpenCommand>();
    if (openCmd == null) return HookResult.Continue;
    var cmdLocale = provider.GetRequiredService<IWardenCmdOpenLocale>();

    openCellsTimer?.Kill();
    openCellsTimer = parent.AddTimer(CV_WARDEN_AUTO_OPEN_CELLS.Value, () => {
      var cellZone = getCellZone();

      var prisoners = PlayerUtil.FromTeam(CsTeam.Terrorist)
       .Where(p => p.Pawn.Value != null && p.PlayerPawn.Value?.AbsOrigin != null
          && cellZone.IsInsideZone(p.PlayerPawn.Value?.AbsOrigin!))
       .ToList();

      if (openCmd.OpenedCells) {
        // TODO: Re-enable when Gangs plugin is migrated (Task 15)
        // snitch prisoners with gang cells perk
        if (CV_WARDEN_AUTO_SNITCH.Value && prisoners.Count != 0)
          baseSnitchPrisoners(prisoners.Count, true);
        return;
      }

      var zoneMgr = provider.GetService<IZoneManager>();
      if (zoneMgr != null)
        Stubs.MapUtil.OpenCells(zoneMgr);
      else
        Stubs.MapUtil.OpenCells();

      if (prisoners.Count == 0) return;

      if (CV_WARDEN_AUTO_SNITCH.Value)
        baseSnitchPrisoners(prisoners.Count, false);
      else
        cmdLocale.CellsOpened.ToAllChat();
    });

    return HookResult.Continue;
  }

  private void baseSnitchPrisoners(int count, bool opened) {
    var cmdLocale = provider.GetRequiredService<IWardenCmdOpenLocale>();
    var msg = opened ?
      cmdLocale.CellsOpenedSnitchPrisoners(count) :
      cmdLocale.CellsOpenedWithPrisoners(count);
    msg.ToAllChat();
  }

  private IZone getCellZone() {
    var manager = provider.GetService<IZoneManager>();
    if (manager != null) {
      var zones = manager.GetZones(Server.MapName, ZoneType.CELL)
       .GetAwaiter()
       .GetResult();
      if (zones.Count > 0) return new MultiZoneWrapper(zones);
    }

    var bounds = new DistanceZone(
      Utilities
       .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!)
       .ToList(), DistanceZone.WIDTH_CELL);
    return bounds;
  }

  [GameEventHandler]
  public HookResult OnPlayerDisconnect(EventPlayerDisconnect ev,
    GameEventInfo info) {
    if (!IsWarden(ev.Userid)) return HookResult.Continue;

    if (!TryRemoveWarden())
      logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");


    locale.WardenLeft.ToAllChat().ToAllCenter();

    foreach (var player in Utilities.GetPlayers())
      player.ExecuteClientCommand(
        $"play sounds/{CV_WARDEN_SOUND_PASSED.Value}");

    locale.BecomeNextWarden.ToAllChat();
    return HookResult.Continue;
  }
}
