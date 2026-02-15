using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Models;
using Jailbreak.LastRequest.Enums;
using Jailbreak.LastRequest.Locale;
using Jailbreak.LastRequest.Utils;

namespace Jailbreak.LastRequest.Services;

public class LastRequestManager : ILastRequestManager {
  public static readonly FakeConVar<int> CV_LR_BASE_TIME =
    new("css_jb_lr_time_base",
      "Round time to set when LR is activated, 0 to disable", 60);

  public static readonly FakeConVar<int> CV_LR_BONUS_TIME =
    new("css_jb_lr_time_per_lr",
      "Additional round time to add per LR completion", 20);

  public static readonly FakeConVar<int> CV_LR_GUARD_TIME =
    new("css_jb_lr_time_per_guard", "Additional round time to add per guard");

  public static readonly FakeConVar<int> CV_PRISONER_TO_LR =
    new("css_jb_lr_activate_lr_at", "Number of prisoners to activate LR at", 2,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 32));

  public static readonly FakeConVar<int> CV_MAX_TIME_FOR_LR =
    new("css_jb_max_time_for_lr", "Maximum round time during LR", 60);

  private readonly ILastRequestLocale messages;
  private readonly BasePlugin plugin;
  private ILastRequestFactory? factory;

  public bool IsLREnabledForRound { get; set; } = true;

  public LastRequestManager(ILastRequestLocale messages, BasePlugin plugin) {
    this.messages = messages;
    this.plugin   = plugin;
  }

  public void Initialize(ILastRequestFactory factory) {
    this.factory = factory;

    plugin.RegisterListener<Listeners.OnEntityParentChanged>(OnDrop);
    VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Hook(OnCanAcquire,
      HookMode.Pre);
  }

  public void Shutdown() {
    VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(OnCanAcquire,
      HookMode.Pre);
  }

  public bool ShouldBlockDamage(CCSPlayerController victim,
    CCSPlayerController? attacker) {
    if (!IsLREnabled) return false;

    if (attacker == null || !attacker.IsReal()) return false;

    var victimLR   = ((ILastRequestManager)this).GetActiveLR(victim);
    var attackerLR = ((ILastRequestManager)this).GetActiveLR(attacker);

    if (victimLR == null && attackerLR == null)
      return false;

    if (victimLR == null != (attackerLR == null)) {
      messages.DamageBlockedInsideLastRequest.ToCenter(attacker);
      return true;
    }

    if (victimLR == null) return false;

    if (victimLR.Prisoner.Slot == attacker.Slot
        || victimLR.Guard.Slot == attacker.Slot)
      return false;

    messages.DamageBlockedNotInSameLR.ToCenter(attacker);
    return true;
  }

  public bool IsLREnabled { get; set; }

  public IList<AbstractLastRequest> ActiveLRs { get; } =
    new List<AbstractLastRequest>();

  public void DisableLR() { IsLREnabled = false; }

  public void DisableLRForRound() {
    DisableLR();
    IsLREnabledForRound = false;
  }

  public void EnableLR(CCSPlayerController? died = null) {
    messages.LastRequestEnabled().ToAllChat();
    IsLREnabled = true;

    // TODO: Re-enable when Stats plugin is migrated
    // API.Stats?.PushStat(new ServerStat("JB_LASTREQUEST_ACTIVATED"));

    var cts = Utilities.GetPlayers()
     .Count(p => p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true });

    if (CV_LR_BASE_TIME.Value != 0)
      RoundUtil.SetTimeRemaining(CV_LR_BASE_TIME.Value);

    RoundUtil.AddTimeRemaining(CV_LR_GUARD_TIME.Value * cts);

    var players = Utilities.GetPlayers();
    var core    = JailbreakApi.Core.Get();
    foreach (var player in players) {
      player.ExecuteClientCommand("play sounds/lr");

      // TODO: Re-enable when Gangs plugin is migrated
      // Gangs stat tracking for LR reached

      if (!player.PawnIsAlive) continue;
      if (player.Team != CsTeam.Terrorist) continue;
      if (died != null && player.SteamID == died.SteamID) continue;

      player.ExecuteClientCommandFromServer("css_lr");
    }

    // TODO: Re-enable when Gangs plugin is migrated
    // Credit granting for surviving to LR
  }

  public bool InitiateLastRequest(CCSPlayerController prisoner,
    CCSPlayerController guard, LRType type) {
    var lr = factory!.CreateLastRequest(prisoner, guard, type);

    if (lr is ILastRequestConfig configurable
      && configurable.RequiresConfiguration) {
      configurable.OpenConfigMenu(prisoner, guard, () => {
        completeLRInitiation(lr, prisoner, guard);
      });
    } else {
      completeLRInitiation(lr, prisoner, guard);
    }

    return true;
  }

  private void completeLRInitiation(AbstractLastRequest lr,
    CCSPlayerController prisoner, CCSPlayerController guard) {
    lr.Setup();
    ActiveLRs.Add(lr);

    // TODO: Re-enable when Stats plugin is migrated
    // API.Stats?.PushStat(new ServerStat("JB_LASTREQUEST",
    //   $"{prisoner.SteamID} {lr.Type.ToFriendlyString()}"));

    prisoner.SetHealth(100);
    guard.SetHealth(100);
    prisoner.SetArmor(0);
    guard.SetArmor(0);

    // TODO: Re-enable when Gangs plugin is migrated
    // LR start stat tracking and color application

    messages.InformLastRequest(lr).ToAllChat();
  }

  public bool EndLastRequest(AbstractLastRequest lr, LRResult result) {
    // TODO: Re-enable when Rainbow plugin is migrated
    // rainbowColorizer.StopRainbow(lr.Prisoner);
    // rainbowColorizer.StopRainbow(lr.Guard);

    if (result is LRResult.GUARD_WIN or LRResult.PRISONER_WIN) {
      addRoundTimeCapped(CV_LR_BONUS_TIME.Value, CV_MAX_TIME_FOR_LR.Value);
      messages.LastRequestDecided(lr, result).ToAllChat();

      // TODO: Re-enable when Gangs plugin is migrated
      // Credit granting for LR win
    }

    // TODO: Re-enable when Stats plugin is migrated
    // API.Stats?.PushStat(new ServerStat("JB_LASTREQUEST_RESULT",
    //   $"{lr.Prisoner.SteamID} {result.ToString()}"));

    lr.OnEnd(result);
    ActiveLRs.Remove(lr);
    return true;
  }

  private void OnDrop(CEntityInstance entity, CEntityInstance newparent) {
    if (!entity.IsValid) return;
    if (!WeaponTag.WEAPONS.Contains(entity.DesignerName)
      && !WeaponTag.UTILITY.Contains(entity.DesignerName))
      return;

    var weapon = Utilities.GetEntityFromIndex<CCSWeaponBase>((int)entity.Index);
    var owner  = weapon?.PrevOwner.Get()?.OriginalController.Get();

    if (owner == null || weapon == null || !weapon.IsValid) return;

    var lr = ((ILastRequestManager)this).GetActiveLR(owner);
    if (lr == null) return;

    if (newparent.IsValid) return;

    var color = owner.Team == CsTeam.CounterTerrorist ? Color.Blue : Color.Red;
    weapon.SetColor(color);

    if (lr is not IDropListener listener) return;
    listener.OnWeaponDrop(owner, weapon);
  }

  private HookResult OnCanAcquire(DynamicHook hook) {
    if (ActiveLRs.Count == 0) return HookResult.Continue;
    var player = hook.GetParam<CCSPlayer_ItemServices>(0)
     .Pawn.Value.Controller.Value?.As<CCSPlayerController>();
    var data = VirtualFunctions.GetCSWeaponDataFromKey.Invoke(-1,
      hook.GetParam<CEconItemView>(1).ItemDefinitionIndex.ToString());

    if (player == null || !player.IsValid) return HookResult.Continue;

    var method = hook.GetParam<AcquireMethod>(2);
    if (method != AcquireMethod.PickUp) return HookResult.Continue;

    if (ActiveLRs.Any(lr => lr.PreventEquip(player, data))) {
      hook.SetReturn(AcquireResult.NotAllowedByMode);
      return HookResult.Handled;
    }

    return HookResult.Continue;
  }

  public HookResult OnTakeDamage(EventPlayerHurt ev, GameEventInfo info) {
    var player   = ev.Userid;
    var attacker = ev.Attacker;
    if (player == null || !player.IsReal()) return HookResult.Continue;
    if (!ShouldBlockDamage(player, attacker)) return HookResult.Continue;
    if (player.PlayerPawn.IsValid) {
      var playerPawn = player.PlayerPawn.Value!;
      playerPawn.Health += ev.DmgHealth;
    }

    info.DontBroadcast = false;
    ev.DmgArmor        = ev.DmgHealth = 0;
    return HookResult.Handled;
  }

  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    foreach (var lr in ActiveLRs.ToList())
      EndLastRequest(lr, LRResult.TIMED_OUT);

    IsLREnabled = false;
    return HookResult.Continue;
  }

  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    IsLREnabledForRound = true;
    IsLREnabled         = false;
    foreach (var player in Utilities.GetPlayers())
      MenuManager.CloseActiveMenu(player);

    foreach (var lr in ActiveLRs.ToList())
      EndLastRequest(lr, LRResult.TIMED_OUT);
    ActiveLRs.Clear();
    return HookResult.Continue;
  }

  public HookResult OnPlayerDeath(EventPlayerDeath @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal() || RoundUtil.IsWarmup())
      return HookResult.Continue;

    var activeLr = ((ILastRequestManager)this).GetActiveLR(player);
    if (activeLr != null && activeLr.State != LRState.COMPLETED) {
      var isPrisoner = activeLr.Prisoner.Slot == player.Slot;
      EndLastRequest(activeLr,
        isPrisoner ? LRResult.GUARD_WIN : LRResult.PRISONER_WIN);

      return HookResult.Continue;
    }

    if (!IsLREnabledForRound) return HookResult.Continue;

    if (player.Team != CsTeam.Terrorist) return HookResult.Continue;
    checkLR();
    return HookResult.Continue;
  }

  public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event,
    GameEventInfo info) {
    var player = @event.Userid;

    if (player == null) return HookResult.Continue;

    if (!player.IsReal() || RoundUtil.IsWarmup()) return HookResult.Continue;

    if (IsLREnabled) {
      var activeLr = ((ILastRequestManager)this).GetActiveLR(player);
      if (activeLr != null)
        EndLastRequest(activeLr,
          player.Team == CsTeam.Terrorist ?
            LRResult.GUARD_WIN :
            LRResult.PRISONER_WIN);

      return HookResult.Continue;
    }

    if (!IsLREnabledForRound) return HookResult.Continue;

    if (player.Team != CsTeam.Terrorist) return HookResult.Continue;

    checkLR();
    return HookResult.Continue;
  }

  private void checkLR() {
    Server.RunOnTick(Server.TickCount + 32, () => {
      if (IsLREnabled) return;
      if (Utilities.GetPlayers().All(p => p.Team != CsTeam.CounterTerrorist))
        return;
      if (countAlivePrisoners() > CV_PRISONER_TO_LR.Value) return;
      EnableLR();
    });
  }

  private int countAlivePrisoners() {
    return Utilities.GetPlayers().Count(prisonerCountsToLR);
  }

  private bool prisonerCountsToLR(CCSPlayerController player) {
    if (!player.IsReal()) return false;
    if (!player.PawnIsAlive) return false;
    return player.Team == CsTeam.Terrorist;
  }

  private void addRoundTimeCapped(int time, int max) {
    var timeleft                    = RoundUtil.GetTimeRemaining();
    if (timeleft + time > max) time = max - timeleft;
    RoundUtil.AddTimeRemaining(time);
  }
}
