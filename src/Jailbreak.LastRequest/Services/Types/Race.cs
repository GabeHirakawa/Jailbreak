using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.LastRequest.Enums;
using Jailbreak.LastRequest.Locale;
using Jailbreak.LastRequest.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.LastRequest.Services.Types;

public class Race(BasePlugin plugin, ILastRequestManager manager,
  ILastRequestLocale messages, IBeamShapeFactory shapeFactory,
  CCSPlayerController prisoner, CCSPlayerController guard)
  : TeleportingRequest(plugin, manager, prisoner, guard) {
  private DateTime raceStart;
  private Timer? raceTimer;
  private BeamedPolylineShape? start, end;
  private Vector? startLocation, endLocation;

  public override LRType Type => LRType.RACE;

  public override void Setup() {
    base.Setup();

    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();

    messages.RaceEndRaceInstruction.ToChat(Prisoner);
    messages.RaceStartingMessage(Prisoner).ToChat(Guard);

    startLocation = Prisoner.Pawn.Value?.AbsOrigin?.Clone();

    if (startLocation == null) return;
    start = shapeFactory.CreateShape(startLocation, BeamShapeType.CIRCLE, 20,
      16);
    start.SetColor(Color.Aqua);
    start.Draw();

    if (Guard.Pawn.Value != null) Guard.Pawn.Value.TakesDamage = false;
  }

  // Called when the prisoner types !endrace
  public override void Execute() {
    endLocation = Prisoner.Pawn.Value?.AbsOrigin?.Clone();

    if (endLocation == null) return;
    end = shapeFactory.CreateShape(endLocation, BeamShapeType.CIRCLE, 10,
      32);
    end.SetColor(Color.Red);
    end.Draw();

    Prisoner.Pawn.Value?.Teleport(startLocation);
    Guard.Pawn.Value?.Teleport(startLocation);

    if (Prisoner.Pawn.Value != null) Prisoner.Pawn.Value.TakesDamage = false;

    Guard.Freeze();
    Prisoner.Freeze();

    Plugin.AddTimer(1, () => {
      Guard.UnFreeze();
      Prisoner.UnFreeze();
    });

    raceStart = DateTime.Now;

    raceTimer = Plugin.AddTimer(0.1f, tick, TimerFlags.REPEAT);
  }

  private void tick() {
    if (!Prisoner.IsValid || !Guard.IsValid) {
      Manager.EndLastRequest(this, LRResult.INTERRUPTED);
      return;
    }

    if (Prisoner.AbsOrigin == null || Guard.AbsOrigin == null) return;
    var requiredDistance       = getRequiredDistance();
    var requiredDistanceSqured = MathF.Pow(requiredDistance, 2);

    end?.SetRadius(requiredDistance / 2);
    end?.Update();

    if (endLocation == null) return;
    var guardDist = Guard.Pawn.Value?.AbsOrigin?.Clone()
     .DistanceSquared(endLocation);

    if (guardDist < requiredDistanceSqured) {
      Manager.EndLastRequest(this, LRResult.GUARD_WIN);
      return;
    }

    var prisonerDist = Prisoner.Pawn.Value?.AbsOrigin?.Clone()
     .DistanceSquared(endLocation);
    if (prisonerDist < requiredDistanceSqured)
      Manager.EndLastRequest(this, LRResult.PRISONER_WIN);
  }

  // https://www.desmos.com/calculator/e1qwgpmtmz
  private float getRequiredDistance() {
    var elapsedSeconds = (DateTime.Now - raceStart).TotalSeconds;

    return (float)(10 + elapsedSeconds + Math.Pow(elapsedSeconds, 2.9) / 5000);
  }

  public override void OnEnd(LRResult result) {
    State = LRState.COMPLETED;
    if (Prisoner.Pawn.Value != null) Prisoner.Pawn.Value.TakesDamage = true;
    if (Guard.Pawn.Value != null) Guard.Pawn.Value.TakesDamage       = true;
    switch (result) {
      case LRResult.GUARD_WIN:
        Prisoner.Pawn.Value?.CommitSuicide(false, true);
        break;
      case LRResult.PRISONER_WIN:
        Guard.Pawn.Value?.CommitSuicide(false, true);
        break;
    }

    raceTimer?.Kill();
    start?.Remove();
    end?.Remove();
  }
}
