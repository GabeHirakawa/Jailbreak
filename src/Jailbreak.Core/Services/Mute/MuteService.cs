using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Services;
using Jailbreak.Core.Locale;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Core.Services.Mute;

/// <summary>
/// Voice mute system for peace periods. Mutes prisoners and guards
/// with configurable durations based on context.
/// Migrated from Jailbreak.Mute.MuteSystem.
/// </summary>
public class MuteService : IMuteService {
  private DateTime ctPeaceEnd = DateTime.MinValue;
  private DateTime lastPeace = DateTime.MinValue;

  private readonly ICoreLocale locale;
  private BasePlugin? parent;
  private DateTime peaceEnd = DateTime.MinValue;

  private Timer? prisonerTimer, guardTimer;
  private IWardenService warden = null!;

  public MuteService(ICoreLocale locale) {
    this.locale = locale;
  }

  /// <summary>
  /// Set the warden service reference. Must be called before Initialize
  /// to break the circular dependency (Warden needs Mute, Mute needs Warden).
  /// </summary>
  public void SetWardenService(IWardenService wardenService) {
    warden = wardenService;
  }

  public void Initialize(BasePlugin basePlugin) {
    parent = basePlugin;
    basePlugin.RegisterListener<Listeners.OnClientVoice>(OnPlayerSpeak);
  }

  public void Dispose() {
    parent?.RemoveListener<Listeners.OnClientVoice>(OnPlayerSpeak);
  }

  public void PeaceMute(MuteReason reason) {
    var duration   = getPeaceDuration(reason);
    var ctDuration = Math.Min(10, duration);
    foreach (var player in Utilities.GetPlayers()
     .Where(player => !warden.IsWarden(player)))
      mute(player);

    switch (reason) {
      case MuteReason.ADMIN:
        locale.PeaceEnactedByAdmin(duration).ToAllChat();
        break;
      case MuteReason.WARDEN_TAKEN:
        locale.GeneralPeaceEnacted(duration).ToAllChat();
        break;
      case MuteReason.WARDEN_INVOKED:
        locale.WardenEnactedPeace(duration).ToAllChat();
        break;
      case MuteReason.INITIAL_WARDEN:
        locale.GeneralPeaceEnacted(duration).ToAllChat();
        break;
    }

    peaceEnd   = DateTime.Now.AddSeconds(duration);
    ctPeaceEnd = DateTime.Now.AddSeconds(ctDuration);
    lastPeace  = DateTime.Now;

    guardTimer?.Kill();
    prisonerTimer?.Kill();

    guardTimer    = parent!.AddTimer(ctDuration, unmuteGuards);
    prisonerTimer = parent!.AddTimer(duration, unmutePrisoners);
  }

  public void UnPeaceMute() {
    if (guardTimer != null) unmuteGuards();
    if (prisonerTimer != null) unmutePrisoners();
  }

  public bool IsPeaceEnabled() { return DateTime.Now < peaceEnd; }

  public DateTime GetLastPeace() { return lastPeace; }

  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    UnPeaceMute();
    foreach (var player in Utilities.GetPlayers()) unmute(player);
    return HookResult.Continue;
  }

  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    UnPeaceMute();
    return HookResult.Continue;
  }

  public HookResult OnDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal()) return HookResult.Continue;

    mute(player);
    return HookResult.Continue;
  }

  private void unmuteGuards() {
    foreach (var player in Utilities.GetPlayers()
     .Where(player => player is {
        Team: CsTeam.CounterTerrorist, PawnIsAlive: true
      }))
      unmute(player);

    if (guardTimer != null) locale.UnmutedGuards.ToAllChat();
    guardTimer = null;
  }

  private void unmutePrisoners() {
    foreach (var player in Utilities.GetPlayers()
     .Where(player => player is { Team: CsTeam.Terrorist, PawnIsAlive: true }))
      unmute(player);

    if (prisonerTimer != null) locale.UnmutedPrisoners.ToAllChat();
    prisonerTimer = null;
  }

  private int getPeaceDuration(MuteReason reason) {
    var prisoners = Utilities.GetPlayers()
     .Count(c => c is { Team: CsTeam.Terrorist, PawnIsAlive: true });
    // https://www.desmos.com/calculator/gwd9cqw4yq
    var baseTime = (int)Math.Floor((prisoners + 30) / 5.0) * 5;

    return reason switch {
      MuteReason.ADMIN          => baseTime,
      MuteReason.WARDEN_TAKEN   => baseTime / 4,
      MuteReason.INITIAL_WARDEN => baseTime,
      MuteReason.WARDEN_INVOKED => baseTime / 2,
      _                         => baseTime
    };
  }

  private void mute(CCSPlayerController player) {
    if (bypassMute(player)) return;
    player.VoiceFlags |= VoiceFlags.Muted;
  }

  private void unmute(CCSPlayerController player) {
    player.VoiceFlags &= ~VoiceFlags.Muted;
  }

  private void OnPlayerSpeak(int playerSlot) {
    var player = Utilities.GetPlayerFromSlot(playerSlot);
    if (player == null || !player.IsReal()) return;

    if (warden.IsWarden(player)) {
      // Always let the warden speak
      unmute(player);
      return;
    }

    if (!player.PawnIsAlive && !bypassMute(player)) {
      // Normal players can't speak when dead
      locale.DeadReminder.ToCenter(player);
      mute(player);
      return;
    }

    if (isMuted(player)) {
      // Remind any muted players they're muted
      locale.MuteReminder.ToCenter(player);
      return;
    }

    if (!bypassMute(player)) return;

    // Warn admins if they're not muted
    if (IsPeaceEnabled()) {
      if (player.Team == CsTeam.CounterTerrorist && DateTime.Now >= ctPeaceEnd)
        return;
      locale.PeaceReminder.ToCenter(player);
    }

    if (!player.PawnIsAlive) locale.AdminDeadReminder.ToCenter(player);
  }

  private bool isMuted(CCSPlayerController player) {
    if (!player.IsReal()) return false;
    return (player.VoiceFlags & VoiceFlags.Muted) != 0;
  }

  private bool bypassMute(CCSPlayerController player) {
    return player.IsReal()
      && AdminManager.PlayerHasPermissions(player, "@css/chat");
  }
}
