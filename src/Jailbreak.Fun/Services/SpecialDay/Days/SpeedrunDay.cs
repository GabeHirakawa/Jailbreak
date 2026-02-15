using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Utils;
using Jailbreak.Contracts.Validators;
using Jailbreak.Fun.Enums;
using Jailbreak.Fun.Locale;
using Jailbreak.Fun.Utils;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Fun.Services.SpecialDay.Days;

public class SpeedrunDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  public static readonly FakeConVar<int> CV_INITIAL_SPEEDRUN_TIME =
    new("css_jb_speedrun_initial_time",
      "Duration in seconds to grant the speedrunner", 20);

  public static readonly FakeConVar<int> CV_FIRST_ROUND_FREEZE =
    new("css_jb_speedrun_first_round_freeze",
      "Duration in seconds to give players time to read the rules of speedrun",
      6);

  public static readonly FakeConVar<int> CV_FREEZE_TIME =
    new("css_jb_speedrun_freeze_time",
      "Duration in seconds to freeze players before the speedrun starts", 2);

  public static readonly FakeConVar<int> CV_WIN_TIME_BASE =
    new("css_jb_speedrun_win_time_base",
      "Base duration in seconds to give the winner to kill other competitors",
      25);

  public static readonly FakeConVar<int> CV_WIN_TIME_BONUS =
    new("css_jb_speedrun_win_time_bonus",
      "Bonus duration in seconds to give the winner for every competitor", 5);

  public static readonly FakeConVar<int> CV_WIN_TIME_MAX =
    new("css_jb_speedrun_win_max",
      "Max time to give the winner regardless of bonus", 60);

  public static readonly FakeConVar<string> CV_WIN_WEAPONS = new(
    "css_jb_speedrun_win_weapons",
    "Weapon(s) to give to the winner to kill other competitors",
    "weapon_knife,weapon_negev",
    customValidators: new ItemValidator(allowMultiple: true));

  public static readonly FakeConVar<string> CV_LOSERS_WEAPONS = new(
    "css_jb_speedrun_loser_weapons",
    "Weapon(s) to give to the losers to use against the winner", "",
    customValidators: new ItemValidator(allowMultiple: true));

  public static readonly FakeConVar<bool> CV_WINNER_DAMAGEABLE = new(
    "css_jb_speedrun_winner_damageable", "Whether the winner can be damaged");

  public static readonly FakeConVar<bool> CV_LOSER_DAMAGEABLE = new(
    "css_jb_speedrun_loser_damageable", "Whether the losers can be damaged",
    true);

  public static readonly FakeConVar<int> CV_TELEPORT_TYPE =
    new("css_jb_speedrun_teleport_type",
      "0 = Dont teleport at end, 1 = Teleport losers to winner, 2 = Teleport winner to loser(s)",
      2);

  public static readonly FakeConVar<int> CV_MAX_PLAYERS_TO_FINISH = new(
    "css_jb_speedrun_finish_at",
    "Number of players required to declare a winner", 2,
    customValidators: new RangeValidator<int>(2, 10));

  private readonly HashSet<int> finishedPlayers = [];
  private readonly Random rng = new();

  // TODO: Re-enable trail system when Draw/Trail services are migrated
  // private readonly Dictionary<int, ActivePlayerTrail<VectorTrailSegment>>
  //   activeTrails = new();
  // private AbstractTrail<BeamTrailSegment>? bestTrail;
  // private BeamedPolylineShape? targetCircle;

  private LinkedList<(int, float)> finishTimestampList = [];
  private float? bestTime;
  private int? bestTimePlayerSlot;
  private int round, playersAliveAtStart;
  private Timer? roundEndTimer;
  private float? roundStartTime;
  private CCSPlayerController? speedrunner;
  private Vector? start;
  private Vector? target;
  private ISpeedDayLocale Msg => (ISpeedDayLocale)Locale;

  public override SDType Type => SDType.SPEEDRUN;
  public override SpecialDaySettings Settings => new SpeedrunSettings();
  public ISDInstanceLocale Locale => new SpeedrunDayLocale();

  public override void Setup() {
    foreach (var player in Utilities.GetPlayers()
     .Where(p => p is { Team: CsTeam.Terrorist or CsTeam.CounterTerrorist }))
      player.Respawn();

    speedrunner = getRunner();

    if (speedrunner == null) {
      speedrunner = PlayerUtil.GetAlive().FirstOrDefault();
      if (speedrunner == null) {
        panic("Could not find a speedrunner");
        return;
      }
    }

    Timers[0.1f] += () => {
      foreach (var player in PlayerUtil.GetAlive()) {
        player.RemoveWeapons();
        if (player.Slot == speedrunner.Slot) continue;
        player.SetColor(Color.FromArgb(100, 255, 255, 255));
      }
    };
    Timers[CV_FIRST_ROUND_FREEZE.Value - 4] += () => {
      if (!speedrunner.IsValid || speedrunner.Connected
        != PlayerConnectedState.PlayerConnected)
        speedrunner = getRunner();
      if (speedrunner == null) {
        panic("Speedrunner is invalid, and we cannot find a new one");
        return;
      }

      Msg.RunnerAssigned(speedrunner).ToAllChat();
      speedrunner.SetColor(Color.DodgerBlue);
      Msg.YouAreRunner(CV_INITIAL_SPEEDRUN_TIME.Value).ToChat(speedrunner);
    };
    Timers[CV_FIRST_ROUND_FREEZE.Value] += () => {
      if (!speedrunner.IsValid || speedrunner.Connected
        != PlayerConnectedState.PlayerConnected) {
        speedrunner = getRunner();
        if (speedrunner == null) {
          panic(
            "Original speedrunner is invalid, and we cannot find a new one");
          return;
        }

        speedrunner.SetColor(Color.DodgerBlue);
        Msg.RunnerLeftAndReassigned(speedrunner).ToAllChat();
        Msg.YouAreRunner(CV_INITIAL_SPEEDRUN_TIME.Value).ToChat(speedrunner);
      }

      start = speedrunner.PlayerPawn.Value!.AbsOrigin!.Clone();
      speedrunner.UnFreeze();
      // TODO: bestTrail = createFirstTrail(speedrunner);
    };

    if (CV_INITIAL_SPEEDRUN_TIME.Value > 30)
      Timers[
        CV_INITIAL_SPEEDRUN_TIME.Value + CV_FIRST_ROUND_FREEZE.Value - 30] += ()
        => {
        if (target != null) return;
        if (!speedrunner.IsValid || speedrunner.Connected
          != PlayerConnectedState.PlayerConnected)
          speedrunner = getRunner();
        if (speedrunner == null) {
          panic(
            "Original speedrunner is invalid, and we cannot find a new one");
          return;
        }

        Msg.RuntimeLeft(RoundUtil.GetTimeRemaining()).ToChat(speedrunner);
      };

    Timers[CV_INITIAL_SPEEDRUN_TIME.Value + CV_FIRST_ROUND_FREEZE.Value - 10] +=
      () => {
        if (target != null) return;
        Msg.RuntimeLeft(RoundUtil.GetTimeRemaining()).ToChat(speedrunner);
      };
    Timers[CV_INITIAL_SPEEDRUN_TIME.Value + CV_FIRST_ROUND_FREEZE.Value] +=
      Execute;

    base.Setup();
  }

  public override void Execute() {
    if (target != null) return;
    if (speedrunner == null) {
      panic("Execute: Speedrunner is null");
      return;
    }

    target = speedrunner.Pawn.Value?.AbsOrigin;
    if (target == null) {
      panic("Execute: Could not get AbsOrigin of speedrunner");
      return;
    }

    if (start == null || speedrunner.PlayerPawn.Value == null
      || speedrunner.PlayerPawn.Value.AbsOrigin == null
      || start.DistanceSquared(speedrunner.PlayerPawn.Value.AbsOrigin) < 100)
      panic("Execute: Start is null or too close to speedrunner");

    target = target.Clone();
    // TODO: Re-enable target circle when Draw services are migrated
    // targetCircle = beamShapeFactory.CreateShape(target!, BeamShapeType.CIRCLE, 10);
    // targetCircle.SetColor(Color.Green);
    // targetCircle.Draw();

    var timeSpent = CV_INITIAL_SPEEDRUN_TIME.Value;
    // TODO: Calculate actual time from trail segments when trail system is migrated
    // if (bestTrail != null) {
    //   timeSpent = bestTrail.GetEndSegment()!.GetSpawnTime()
    //     - bestTrail.GetStartSegment()!.GetSpawnTime();
    //   if (bestTrail is ActivePlayerTrail<BeamTrailSegment> active)
    //     active.StopTracking();
    // }

    bestTime = timeSpent;

    var minTime = CV_INITIAL_SPEEDRUN_TIME.Value * 0.5;

    startRound((int)Math.Ceiling(Math.Max(timeSpent * 1.1, minTime)));

    Plugin.RegisterListener<Listeners.OnTick>(checkFinishers);
  }

  private CCSPlayerController? getRunner() {
    var runner = PlayerUtil.GetRandomFromTeam(rng.Next(2) == 0 ?
      CsTeam.Terrorist :
      CsTeam.CounterTerrorist);
    runner ??= PlayerUtil.GetAlive().FirstOrDefault();
    return runner;
  }

  private void startRound(int seconds) {
    roundStartTime = null;

    var alive = PlayerUtil.GetAlive().ToArray();
    playersAliveAtStart = PlayerUtil.GetAlive().Count();
    Msg.BeginRound(++round, getEliminations(playersAliveAtStart), seconds)
     .ToAllChat();

    RoundUtil.SetTimeRemaining(seconds + CV_FREEZE_TIME.Value);

    foreach (var player in alive) {
      var pawn = player.PlayerPawn.Value;
      if (pawn == null) continue;
      pawn.Teleport(start, velocity: Vector.Zero);
      player.Freeze();
    }

    // TODO: resetTrails();
    finishedPlayers.Clear();
    finishTimestampList.Clear();

    Plugin.AddTimer(CV_FREEZE_TIME.Value, () => {
      foreach (var player in PlayerUtil.GetAlive()) player.UnFreeze();
      roundStartTime = Server.CurrentTime;
    }, TimerFlags.STOP_ON_MAPCHANGE);

    roundEndTimer = Plugin.AddTimer(seconds + CV_FREEZE_TIME.Value, endRound,
      TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void checkFinishers() {
    if (target == null || roundStartTime == null) return;

    var minDist = getRequiredDistance();
    // TODO: targetCircle?.SetRadius(minDist / 2);
    // TODO: targetCircle?.Update();
    var required = MathF.Pow(minDist, 2);

    LinkedList<(int, float)> notFinished = [];

    foreach (var player in PlayerUtil.GetAlive()) {
      if (finishedPlayers.Contains(player.Slot)) continue;
      var pos = player.Pawn.Value?.AbsOrigin;
      if (pos == null) continue;
      var dist = pos.DistanceSquared(target);
      if (dist >= required * 1.25f) {
        notFinished.AddLast((player.Slot, dist));
        continue;
      }

      var hdist = pos.HorizontalDistanceSquared(target);
      if (hdist >= required) {
        notFinished.AddLast((player.Slot, dist));
        continue;
      }

      onFinish(player);
    }

    notFinished =
      new LinkedList<(int, float)>(notFinished.OrderBy(x => x.Item2));

    sendDistances(notFinished);
  }

  private void sendDistances(LinkedList<(int, float)> unfinished) {
    if (target == null) {
      panic("sendDistances: Target is null");
      return;
    }

    var rules = ServerExtensions.GetGameRules();
    if (rules != null)
      rules.GameRestart = rules.RestartRoundTime < Server.CurrentTime;

    var originalCompletions = new LinkedList<(int, float)>(finishTimestampList);

    if (unfinished.Count > 0)
      foreach (var (slot, dist) in unfinished)
        finishTimestampList.AddLast((slot, dist));

    const int totalLines = 8;
    var       pos        = 1;
    var       current    = finishTimestampList.First;

    string? top = null;
    while (current != null) {
      var display = 0;
      var lines   = "";
      var player  = Utilities.GetPlayerFromSlot(current.Value.Item1);

      if (player == null || !player.IsValid || player.IsBot) {
        pos++;
        current = current.Next;
        continue;
      }

      var playerLine = current;

      var d = 0;
      while (playerLine != null && (display < totalLines / 2
        || pos - d > finishTimestampList.Count - totalLines
        && display < totalLines)) {
        var (slot, dist) = playerLine.Value;
        playerLine       = playerLine.Previous;
        var p = Utilities.GetPlayerFromSlot(slot);
        if (p == null) continue;
        lines = generateHtmlLine(p, pos - d++, dist) + (d == 1 ? "" : "<br>")
          + lines;
        display++;
      }

      current    = current.Next;
      playerLine = current;

      d = 0;
      while (playerLine != null && display < totalLines) {
        var (slot, dist) = playerLine.Value;
        playerLine       = playerLine.Next;
        var p = Utilities.GetPlayerFromSlot(slot);
        if (p == null) continue;
        lines += "<br>" + generateHtmlLine(p, pos + ++d, dist);
        display++;
      }

      pos++;
      top ??= lines;
      player.PrintToCenterHtml(lines);
    }

    if (top != null)
      foreach (var player in Utilities.GetPlayers().Where(p => !p.PawnIsAlive))
        player.PrintToCenterHtml(top);

    finishTimestampList = originalCompletions;
  }

  private string generateHtmlLine(CCSPlayerController player, int position,
    float distance) {
    string color;
    var    eliminations = getEliminations(playersAliveAtStart);
    var    suffix       = "";

    var isSafe     = position < eliminations && distance < 0;
    var isInDanger = position > playersAliveAtStart - eliminations;

    var text = $"{position} {player.PlayerName}";

    if (isSafe) {
      color  = "00FF00";
      suffix = "<font color=\"#00FF00\"> | S</font>";
    } else if (!isInDanger) {
      var percentDanger = (position - 1 - finishedPlayers.Count)
        / (float)eliminations;
      percentDanger = Math.Clamp(percentDanger, 0, 1);
      var green = 255;
      var red   = (int)(255 * percentDanger);
      color = $"{red:X2}{green:X2}00";
    } else {
      var precentLosing = (position - playersAliveAtStart + eliminations)
        / (float)eliminations;
      precentLosing = Math.Clamp(precentLosing, 0, 1);
      var red   = 255;
      var green = 255 - (int)(255 * precentLosing);
      color  = $"{red:X2}{green:X2}00";
      suffix = "<font color=\"#FF0000\"> | E</font>";
    }

    if (distance < 0) {
      var time = roundStartTime == null ?
        0 :
        MathF.Abs(distance) - roundStartTime.Value;
      text += $" - {time:F4}";
    } else { text += $" - {distance:N0}"; }

    return $"<font color=\"#{color}\">{text}</font>{suffix}";
  }

  private void onFinish(CCSPlayerController player) {
    if (roundStartTime == null) {
      panic("onFinish: roundStartTime is null");
      return;
    }

    var time = Server.CurrentTime - roundStartTime!.Value;
    if (bestTime == null || time < bestTime) {
      bestTime           = time;
      bestTimePlayerSlot = player.Slot;
      Msg.BestTime(player, time).ToAllChat();
      player.SetColor(Color.FromArgb(255, Color.Gold));
    } else {
      Msg.PlayerTime(player, finishedPlayers.Count + 1, -time).ToAllChat();
    }

    finishTimestampList.AddLast((player.Slot, -Server.CurrentTime));
    finishedPlayers.Add(player.Slot);
    var eliminations = getEliminations(PlayerUtil.GetAlive().Count());
    // TODO: activeTrails[player.Slot].StopTracking();

    var taking = playersAliveAtStart - eliminations;
    if (finishedPlayers.Count >= taking) endRound();

    if (!player.IsValid) return;

    if (bestTimePlayerSlot != null && bestTimePlayerSlot == player.Slot) return;

    var alpha = Math.Max(255 - finishedPlayers.Count * 20, 0);
    player.SetColor(Color.FromArgb(alpha, Color.White));
  }

  // https://www.desmos.com/calculator/e1qwgpmtmz
  private float getRequiredDistance() {
    if (roundStartTime == null) return 0;
    var elapsedSeconds = (float)(Server.CurrentTime - roundStartTime);

    return 10 + elapsedSeconds + MathF.Pow(elapsedSeconds, 3.3f) / 2500;
  }

  private void endRound() {
    roundEndTimer?.Kill();
    if (target == null) {
      panic("endRound: Target is null");
      return;
    }

    var aliveCount          = PlayerUtil.GetAlive().Count();
    var playersDiedMidRound = playersAliveAtStart - aliveCount;
    var toEliminate         = getEliminations(aliveCount) - playersDiedMidRound;

    var ctMade = PlayerUtil.FromTeam(CsTeam.CounterTerrorist).Count() < 4;
    var tMade  = PlayerUtil.FromTeam(CsTeam.Terrorist).Count() < 4;

    var nonCompleters = new LinkedList<(int, float)>();
    foreach (var player in PlayerUtil.GetAlive()) {
      if (player.Team == CsTeam.CounterTerrorist) ctMade = true;
      if (player.Team == CsTeam.Terrorist) tMade         = true;
      if (!finishedPlayers.Add(player.Slot)) continue;

      var dist = player.PlayerPawn.Value?.AbsOrigin?.Distance(target);
      if (dist == null) continue;
      nonCompleters.AddLast((player.Slot, dist.Value));
    }

    nonCompleters =
      new LinkedList<(int, float)>(nonCompleters.OrderBy(t => t.Item2));

    foreach (var nc in nonCompleters) finishTimestampList.AddLast(nc);

    if (aliveCount > 1)
      if (ctMade != tMade && round == 1) {
        var random = PlayerUtil.GetRandomFromTeam(tMade ?
          CsTeam.CounterTerrorist :
          CsTeam.Terrorist);

        if (random != null) {
          Msg.ImpossibleLocation(
            ctMade ? CsTeam.Terrorist : CsTeam.CounterTerrorist, random);
          // TODO: Re-enable trail-based path selection
          // bestTrail?.Kill();
          // activeTrails[random.Slot].StopTracking();
          // bestTrail = PulsatingBeamTrail.FromTrail(Plugin, activeTrails[random.Slot]);
          // target = bestTrail!.GetEndSegment()!.GetEnd();
        }

        toEliminate = 2;
        round--;
      }

    announceTimes();

    if (aliveCount <= CV_MAX_PLAYERS_TO_FINISH.Value) {
      if (finishTimestampList.Count == 0) {
        panic("No slowest times found");
        return;
      }

      var winner =
        Utilities.GetPlayerFromSlot(finishTimestampList.First!.Value.Item1);

      if (winner == null || !winner.IsValid) {
        panic("endRound: Winner is null");
        return;
      }

      // TODO: targetCircle?.Remove();

      var losers = PlayerUtil.GetAlive()
       .Where(p => p.Slot != winner.Slot)
       .ToList();

      var timeToSet = CV_WIN_TIME_BASE.Value
        + CV_WIN_TIME_BONUS.Value * losers.Count;

      Msg.PlayerWon(winner).ToAllChat();

      foreach (var loser in losers) {
        loser.SetColor(Color.White);
        if (CV_TELEPORT_TYPE.Value == 1)
          loser.Teleport(winner);
        else if (CV_TELEPORT_TYPE.Value == 2) winner.Teleport(loser);
        if (CV_LOSER_DAMAGEABLE.Value) EnableDamage(loser);
      }

      if (CV_WINNER_DAMAGEABLE.Value) EnableDamage(winner);

      VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(
        OnCanAcquire, HookMode.Pre);

      foreach (var weapon in CV_LOSERS_WEAPONS.Value.Split(','))
        foreach (var loser in losers)
          loser.GiveNamedItem(weapon);

      foreach (var weapon in CV_WIN_WEAPONS.Value.Split(','))
        winner.GiveNamedItem(weapon);

      Plugin.RemoveListener<Listeners.OnTick>(checkFinishers);
      RoundUtil.SetTimeRemaining(Math.Min(timeToSet, CV_WIN_TIME_MAX.Value));
      Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
      return;
    }

    var roundTimeWas = Math.Ceiling(Server.CurrentTime - roundStartTime!.Value);
    var nextRoundTime = (int)Math.Ceiling((bestTime ?? 20) + 10 - round * 1.5);

    if (toEliminate <= 0) {
      Msg.NoneEliminated.ToAllChat();
      Plugin.AddTimer(3f, () => { startRound(nextRoundTime); },
        TimerFlags.STOP_ON_MAPCHANGE);
      return;
    }

    nextRoundTime = (int)Math.Min(roundTimeWas, Math.Max(nextRoundTime, 5));
    var slowest = finishTimestampList.Last;

    if (ctMade != tMade && round == 0) {
      bool killedCt = false, killedT = false;
      while (slowest != null) {
        var (slot, _) = slowest.Value;
        var player = Utilities.GetPlayerFromSlot(slot);
        if (player == null || !player.IsValid) {
          slowest = slowest.Previous;
          continue;
        }

        switch (player.Team) {
          case CsTeam.CounterTerrorist when !killedCt:
            killedCt = true;
            eliminatePlayer(player);
            toEliminate--;
            break;
          case CsTeam.Terrorist when !killedT:
            killedT = true;
            eliminatePlayer(player);
            toEliminate--;
            break;
        }

        if (killedCt && killedT) break;
        slowest = slowest.Previous;
      }
    }

    for (var i = 0; i < toEliminate; i++) {
      if (slowest == null) break;
      var (slot, _) = slowest.Value;
      slowest       = slowest.Previous;
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null || !player.IsValid) continue;
      EnableDamage(player);
      player.CommitSuicide(false, true);
      Msg.PlayerEliminated(player).ToAllChat();
    }

    Plugin.AddTimer(3f, () => { startRound(nextRoundTime); },
      TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void eliminatePlayer(CCSPlayerController player) {
    EnableDamage(player);
    player.CommitSuicide(false, true);
    Msg.PlayerEliminated(player).ToAllChat();
  }

  private void panic(string reason) {
    Server.PrintToChatAll($" {ChatColors.Red}PANIC: {reason}");
    Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
    RoundUtil.SetTimeRemaining(1);
  }

  private int getEliminations(int players) {
    return players switch {
      <= 4  => 1,
      <= 8  => 3,
      <= 12 => 4,
      <= 20 => 5,
      <= 35 => 8,
      <= 40 => 10,
      <= 64 => 15,
      _     => players / 5
    };
  }

  private void announceTimes() {
    var position = finishedPlayers.Count;
    var slowest  = finishTimestampList.Last;

    while (slowest != null) {
      if (slowest.Value.Item2 <= 0) break;
      var (slot, dist) = slowest.Value;
      var player = Utilities.GetPlayerFromSlot(slot);
      slowest = slowest.Previous;
      if (player == null) continue;
      Msg.PlayerTime(player, position--, dist).ToChat(player);
    }
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);

    Plugin.RemoveListener<Listeners.OnTick>(checkFinishers);
    // TODO: bestTrail?.Kill();
    roundEndTimer?.Kill();

    // TODO: foreach (var trail in activeTrails.Values) trail.Kill();
    // TODO: activeTrails.Clear();

    return result;
  }

  private class SpeedrunSettings : SpecialDaySettings {
    public SpeedrunSettings() {
      CtTeleport   = TeleportType.RANDOM_STACKED;
      TTeleport    = TeleportType.RANDOM_STACKED;
      StripToKnife = true;
      ConVarValues["mp_ignore_round_win_conditions"] = true;
      if (new Random().Next(3) == 0) WithAutoBhop();
      WithFriendlyFire();
    }

    public override Func<int> RoundTime
      => () => CV_INITIAL_SPEEDRUN_TIME.Value + CV_FIRST_ROUND_FREEZE.Value;

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      return new HashSet<string>();
    }

    public override float FreezeTime(CCSPlayerController player) {
      return CV_FIRST_ROUND_FREEZE.Value;
    }
  }
}
