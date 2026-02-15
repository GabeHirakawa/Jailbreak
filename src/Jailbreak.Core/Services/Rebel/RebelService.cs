using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Core.Services.Logs;
using Jailbreak.Core.Locale;
using Jailbreak.Core.Services.Warden;

namespace Jailbreak.Core.Services.Rebel;

/// <summary>
/// Manages rebel state for prisoners. Tracks rebel timers and applies visual indicators.
/// Migrated from Jailbreak.Rebel.RebelManager.
/// </summary>
public class RebelService : IRebelService {
  public static readonly FakeConVar<int> CV_REBEL_TIME =
    new("css_jb_rebel_time", "Time to mark a rebel for", 45,
      ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 500));

  private readonly ICoreLocale locale;
  private readonly IRichLogService logs;
  private readonly ISpecialTreatmentService stService;

  private readonly Dictionary<CCSPlayerController, long> rebelTimes = new();
  private bool enabled = true;

  public RebelService(ICoreLocale locale, IRichLogService logs,
    ISpecialTreatmentService stService) {
    this.locale = locale;
    this.logs = logs;
    this.stService = stService;
  }

  public void Initialize(BasePlugin basePlugin) {
    basePlugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    basePlugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    basePlugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);

    basePlugin.AddTimer(1f, () => {
      foreach (var player in GetActiveRebels()) {
        if (!player.IsValid
          || player.Connected != PlayerConnectedState.PlayerConnected)
          continue;

        if (GetRebelTimeLeft(player) <= 0) {
          UnmarkRebel(player);
          continue;
        }

        applyRebelColor(player);
      }
    }, TimerFlags.REPEAT);
  }

  public ISet<CCSPlayerController> GetActiveRebels() {
    return rebelTimes.Keys.ToHashSet();
  }

  public long GetRebelTimeLeft(CCSPlayerController player) {
    if (rebelTimes.TryGetValue(player, out var time))
      return time - DateTimeOffset.Now.ToUnixTimeSeconds();

    return 0;
  }

  public bool MarkRebel(CCSPlayerController player, long time = -1) {
    if (!enabled) return false;
    if (!rebelTimes.ContainsKey(player))
      logs.Append(logs.Player(player), "is now a rebel.");

    stService.Revoke(player, false);

    // TODO: Re-enable MStats integration when available
    // var pos = player.Pawn.Value?.AbsOrigin;
    // if (pos != null)
    //   API.Stats?.PushStat(new ServerStat("JB_REBEL_STARTED", ...));

    if (time == -1) time = CV_REBEL_TIME.Value;

    rebelTimes[player] = DateTimeOffset.Now.ToUnixTimeSeconds() + time;
    applyRebelColor(player);
    return true;
  }

  public void UnmarkRebel(CCSPlayerController player) {
    if (rebelTimes.ContainsKey(player)) {
      locale.NoLongerRebel.ToChat(player);
      logs.Append(logs.Player(player), "is no longer a rebel.");
    }

    rebelTimes.Remove(player);
    applyRebelColor(player);
  }

  public void DisableRebelForRound() { enabled = false; }

  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    enabled = true;
    rebelTimes.Clear();
    foreach (var player in Utilities.GetPlayers()) applyRebelColor(player);

    return HookResult.Continue;
  }

  private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event,
    GameEventInfo info) {
    if (@event.Userid == null) return HookResult.Continue;
    if (rebelTimes.ContainsKey(@event.Userid)) rebelTimes.Remove(@event.Userid);

    return HookResult.Continue;
  }

  private HookResult OnPlayerDeath(EventPlayerDeath @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null) return HookResult.Continue;
    if (!player.IsReal()) return HookResult.Continue;
    rebelTimes.Remove(player);
    return HookResult.Continue;
  }

  // https://www.desmos.com/calculator/g2v6vvg7ax
  private float getRebelTimePercentage(CCSPlayerController player) {
    var x = GetRebelTimeLeft(player);
    if (x > CV_REBEL_TIME.Value) return 1;
    if (x <= 0) return 0;
    return (float)(100 - (CV_REBEL_TIME.Value - x)
      * Math.Sqrt(CV_REBEL_TIME.Value - x) / 3.8f) / 100;
  }

  private Color getRebelColor(CCSPlayerController player) {
    var percent = getRebelTimePercentage(player);
    percent = Math.Clamp(percent, 0, 1);
    var percentRgb          = 255 - (int)Math.Round(percent * 255.0);
    var color               = Color.FromArgb(255, 255, percentRgb, percentRgb);
    if (percent <= 0) color = Color.White;

    return color;
  }

  private void applyRebelColor(CCSPlayerController player) {
    if (!player.IsReal() || player.Pawn.Value == null) return;
    var color = getRebelColor(player);

    player.SetColor(color);

    player.ColorScreen(
      Color.FromArgb(8 + (int)Math.Round(getRebelTimePercentage(player) * 32),
        Color.Red), 1f, 1.5f, PlayerExtensions.FadeFlags.FADE_OUT);
  }
}
