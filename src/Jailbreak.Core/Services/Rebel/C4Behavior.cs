using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Core.Locale;

namespace Jailbreak.Core.Services.Rebel;

/// <summary>
/// Manages C4 (jihad bomb) mechanics for prisoners.
/// Migrated from Jailbreak.Rebel.C4Bomb.C4Behavior.
/// External dependencies (Gangs, MStats, LastRequest) are commented out
/// until their respective plugins are migrated.
/// </summary>
public class C4Behavior : IC4Service {
  public static readonly FakeConVar<bool> CV_GIVE_BOMB = new("css_jb_c4_give",
    "Whether to give a random prisoner a bomb at the beginning of the round.",
    true);

  public static readonly FakeConVar<float> CV_C4_DELAY = new("css_jb_c4_delay",
    "Time in seconds that the bomb takes to explode", .75f,
    ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0, 2));

  public static readonly FakeConVar<float> CV_C4_RADIUS =
    new("css_jb_c4_radius", "Bomb explosion radius", 350,
      ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0, 10000));

  public static readonly FakeConVar<float> CV_C4_BASE_DAMAGE =
    new("css_jb_c4_damage", "Base damage to apply", 340, ConVarFlags.FCVAR_NONE,
      new RangeValidator<float>(0, 10000));

  private readonly ICoreLocale locale;
  private readonly IRebelService rebelService;
  private readonly IServiceProvider provider;

  private readonly Dictionary<CC4, C4Metadata> bombs = new();
  // TODO: Re-enable bomb icon caching when Gangs plugin is migrated (Task 15)
  // private readonly Dictionary<ulong, string> cachedBombIcons = new();

  private int roundStart = 0;

  private readonly Dictionary<int, int> deathToKiller = new();
  private bool giveNextRound = true;

  private BasePlugin? plugin;

  public C4Behavior(ICoreLocale locale, IRebelService rebelService,
    IServiceProvider provider) {
    this.locale = locale;
    this.rebelService = rebelService;
    this.provider = provider;
  }

  public void ClearActiveC4s() {
    bombs.Clear();
    deathToKiller.Clear();
  }

  public void TryGiveC4ToPlayer(CCSPlayerController player) {
    var bombEntity = new CC4(player.GiveNamedItem("weapon_c4"));
    bombs.Add(bombEntity, new C4Metadata(false));

    locale.JihadC4Received.ToChat(player);
    locale.JihadC4Usage1.ToChat(player);
  }

  public void StartDetonationAttempt(CCSPlayerController player, float delay,
    CC4 bombEntity) {
    if (plugin == null) return;
    // TODO: Re-enable MStats integration when available
    // var pos = player.Pawn.Value?.AbsOrigin;
    // if (pos != null)
    //   API.Stats?.PushStat(new ServerStat("JB_BOMB_ATTEMPT", ...));

    bombs[bombEntity].IsDetonating = true;

    rebelService.MarkRebel(player);

    Server.RunOnTick(Server.TickCount + (int)(64 * delay),
      () => detonate(player, bombEntity));
    player.EmitSound("jb.jihad");
  }

  public void TryGiveC4ToRandomTerrorist() {
    plugin!.AddTimer(1, () => {
      var validTerroristPlayers = Utilities.GetPlayers()
       .Where(player => player is {
          Team       : CsTeam.Terrorist,
          PawnIsAlive: true,
          IsBot      : false,
          IsValid    : true
        })
       .ToList();
      var numOfTerrorists = validTerroristPlayers.Count;
      if (numOfTerrorists == 0) return;

      Random rnd         = new();
      var    randomIndex = rnd.Next(numOfTerrorists);
      TryGiveC4ToPlayer(validTerroristPlayers[randomIndex]);
    });
  }

  public void DontGiveC4NextRound() { giveNextRound = false; }

  public void Initialize(BasePlugin basePlugin) {
    plugin = basePlugin;
    plugin.RegisterListener<Listeners.OnPlayerButtonsChanged>(
      playerButtonsChanged);
  }

  private void playerButtonsChanged(CCSPlayerController player,
    PlayerButtons pressed, PlayerButtons released) {
    if ((pressed & PlayerButtons.Use) == 0) return;

    foreach (var (bomb, meta) in bombs) {
      if (!bomb.IsValid || meta.IsDetonating) continue;

      var bombCarrier = bomb.OwnerEntity.Value?.As<CCSPlayerPawn>()
       .Controller.Value?.As<CCSPlayerController>();
      if (bombCarrier == null || !bombCarrier.IsValid
        || bombCarrier.Slot != player.Slot)
        continue;

      var activeWeapon = bombCarrier.PlayerPawn.Value?.WeaponServices
      ?.ActiveWeapon.Value;
      if (activeWeapon == null || !activeWeapon.IsValid
        || activeWeapon.Handle != bomb.Handle)
        continue;

      StartDetonationAttempt(bombCarrier, CV_C4_DELAY.Value, bomb);
    }
  }

  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    roundStart = Server.TickCount;
    ClearActiveC4s();
    // TODO: Re-enable bomb icon refresh when Gangs plugin is migrated (Task 15)
    // refreshBombIcons();

    if (!CV_GIVE_BOMB.Value) return HookResult.Continue;
    if (!giveNextRound) {
      giveNextRound = true;
      return HookResult.Continue;
    }

    TryGiveC4ToRandomTerrorist();
    return HookResult.Continue;
  }

  public HookResult OnPlayerDropC4(EventBombDropped @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;

    var bombEntity = Utilities.GetEntityFromIndex<CC4>((int)@event.Entindex);
    if (bombEntity == null) return HookResult.Continue;

    bombs.TryGetValue(bombEntity, out var bombMetadata);
    if (bombMetadata == null) return HookResult.Continue;

    if (bombMetadata.IsDetonating) {
      bombEntity.Remove();
      return HookResult.Stop;
    }

    return HookResult.Continue;
  }

  // Thank you https://github.com/exkludera/cs2-killfeed-icons/blob/main/src/main.cs
  public HookResult OnPlayerDeath(EventPlayerDeath ev, GameEventInfo info) {
    var victim = ev.Userid;

    if (victim == null || !victim.IsValid) return HookResult.Continue;
    if (!deathToKiller.TryGetValue(victim.Slot, out var killerSlot))
      return HookResult.Continue;

    var killer = Utilities.GetPlayerFromSlot(killerSlot);
    if (killer == null || !killer.IsValid) return HookResult.Continue;

    // TODO: Re-enable bomb icon kill feed when Gangs plugin is migrated (Task 15)
    var killerIcon = "weapon_c4";
    ev.Attacker = killer;
    ev.Weapon   = killerIcon;
    return HookResult.Continue;
  }

  private void detonate(CCSPlayerController player, CC4 bomb) {
    if (!player.IsValid || !player.IsReal() || !player.PawnIsAlive) {
      if (bomb.IsValid) bomb.Remove();
      bombs.Remove(bomb);
      return;
    }

    if (Server.TickCount - roundStart < CV_C4_DELAY.Value * 64) return;

    var killed = 0;
    // TODO: Re-enable LastRequest check when LastRequest plugin is migrated (Task 11)
    // var lrs = provider.GetRequiredService<ILastRequestManager>();
    foreach (var ct in Utilities.GetPlayers()
     .Where(p => p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true })) {
      // TODO: Re-enable LR check
      // var lr = lrs.GetActiveLR(ct);
      // if (lr != null) {
      //   var otherLr = lrs.GetActiveLR(player);
      //   if (otherLr == null || otherLr != lr) continue;
      // }

      var distanceFromBomb =
        ct.PlayerPawn.Value!.AbsOrigin!.Distance(player.PlayerPawn.Value
         .AbsOrigin!);
      if (distanceFromBomb > CV_C4_RADIUS.Value) continue;

      var damage = CV_C4_BASE_DAMAGE.Value;
      damage *= (CV_C4_RADIUS.Value - distanceFromBomb) / CV_C4_RADIUS.Value;
      float healthRef = ct.PlayerPawn.Value.Health;
      if (healthRef <= damage) {
        deathToKiller[ct.Slot] = player.Slot;
        ct.CommitSuicide(true, true);
        killed++;
      } else {
        ct.PlayerPawn.Value.Health -= (int)damage;
        Utilities.SetStateChanged(ct.PlayerPawn.Value, "CBaseEntity",
          "m_iHealth");
      }
    }

    // If they didn't have the C4 make sure to remove it.
    player.CommitSuicide(true, true);
    bombs.Remove(bomb);

    // TODO: Re-enable Gangs eco credits when Gangs plugin is migrated (Task 15)
    // if (API.Gangs != null && killed > 0) { ... }

    player.EmitSound("jb.jihadExplosion");
    var particleSystemEntity =
      Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
    particleSystemEntity.EffectName =
      "particles/explosions_fx/explosion_c4_500.vpcf";
    particleSystemEntity.StartActive = true;

    particleSystemEntity.Teleport(player.PlayerPawn.Value!.AbsOrigin!,
      new QAngle(), new Vector());
    particleSystemEntity.DispatchSpawn();

    // TODO: Re-enable MStats integration when available
    // API.Stats?.PushStat(new ServerStat("JB_BOMB_EXPLODED", killed.ToString()));
  }

  private class C4Metadata(bool isDetonating) {
    public bool IsDetonating { get; set; } = isDetonating;
  }
}
