using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Objects;
using Jailbreak.LastRequest.Enums;
using Jailbreak.LastRequest.Services;

namespace Jailbreak.LastRequest.Locale;

public class LastRequestLocale : ILastRequestLocale {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}LR>") {
      Plain = false, Panorama = false, Chat = true
    };

  #region General LR

  public IView DamageBlockedInsideLastRequest
    => new SimpleView { PREFIX, "You or they are in LR, damage blocked." };

  public IView DamageBlockedNotInSameLR
    => new SimpleView {
      PREFIX, "You are not in the same LR as them, damage blocked."
    };

  public IView LastRequestEnabled() {
    return new SimpleView {
      {
        PREFIX,
        $"Last Request activated. Type {ChatColors.BlueGrey}!lr{ChatColors.Grey} to start a last request."
      }
    };
  }

  public IView LastRequestDisabled() {
    return new SimpleView {
      { PREFIX, $"Last Request {ChatColors.Red}disabled{ChatColors.Grey}." }
    };
  }

  public IView LastRequestNotEnabled() {
    return new SimpleView { PREFIX, "Last Request is not enabled." };
  }

  public IView InvalidLastRequest(string query) {
    return new SimpleView { PREFIX, "Invalid Last Request: ", query };
  }

  public IView InformLastRequest(AbstractLastRequest lr) {
    return new SimpleView {
      PREFIX,
      lr.Prisoner,
      "is starting a",
      ChatColors.White + lr.Type.ToFriendlyString(),
      "LR against",
      lr.Guard,
      "."
    };
  }

  public IView LastRequestDecided(AbstractLastRequest lr, LRResult result) {
    var tNull = !lr.Prisoner.IsReal();
    var gNull = !lr.Guard.IsReal();
    if (tNull && gNull)
      return new SimpleView { PREFIX, "Last Request decided." };

    if (tNull && result == LRResult.PRISONER_WIN)
      return new SimpleView {
        PREFIX, lr.Guard, "lost the LR, but the prisoner left the game?"
      };

    if (gNull && result == LRResult.GUARD_WIN)
      return new SimpleView {
        PREFIX, lr.Prisoner, "lost the LR, but the guard left the game?"
      };

    return result switch {
      LRResult.TIMED_OUT => new SimpleView {
        PREFIX, ChatColors.Grey.ToString(), "Last Request timed out."
      },
      LRResult.INTERRUPTED => new SimpleView {
        PREFIX, ChatColors.Grey.ToString(), "Last Request interrupted."
      },
      _ => new SimpleView {
        PREFIX, result == LRResult.PRISONER_WIN ? lr.Prisoner : lr.Guard, "won."
      }
    };
  }

  public IView CannotLR(string reason) {
    return new SimpleView {
      PREFIX,
      $"You cannot LR, {ChatColors.BlueGrey + reason + ChatColors.Grey}."
    };
  }

  public IView CannotLR(CCSPlayerController player, string reason) {
    return new SimpleView {
      PREFIX,
      "You cannot LR",
      player,
      ", " + ChatColors.BlueGrey + reason + ChatColors.Red + "."
    };
  }

  public IView LastRequestCountdown(int seconds) {
    return new SimpleView { PREFIX, "Starting in", seconds, "..." };
  }

  public IView WinByDefault(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "won by default." };
  }

  public IView WinByHealth(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "won by health." };
  }

  public IView WinByReason(CCSPlayerController player, string reason) {
    return new SimpleView { PREFIX, player, "won by", reason + "." };
  }

  public IView Win(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "won." };
  }

  public IView LastRequestRebel(CCSPlayerController player, int tHealth) {
    return new SimpleView {
      PREFIX,
      player,
      $"{ChatColors.LightRed}has decided to {ChatColors.DarkRed}LR Rebel {ChatColors.LightRed}with",
      tHealth,
      $"{ChatColors.LightRed}HP!"
    };
  }

  public IView LastRequestRebelDisabled() {
    return new SimpleView {
      PREFIX, "Rebelling during last request is disabled."
    };
  }

  public IView CannotLastRequestRebelCt() {
    return new SimpleView {
      PREFIX, "You cannot rebel as a CT during the last request."
    };
  }

  #endregion

  #region Rock Paper Scissors

  public IView RpsPlayerMadeChoice(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "made their choice." };
  }

  public IView RpsBothPlayersMadeChoice() {
    return new SimpleView {
      PREFIX, "Both players rocked, papered, and scissored! (ew)"
    };
  }

  public IView RpsTie() {
    return new SimpleView { PREFIX, "It's a tie! Let's go again!" };
  }

  public IView RpsResults(CCSPlayerController guard,
    CCSPlayerController prisoner, int guardPick, int prisonerPick) {
    return new SimpleView {
      PREFIX,
      "Results:",
      guard,
      "picked",
      ToRPS(guardPick),
      "and",
      prisoner,
      "picked",
      ToRPS(prisonerPick)
    };
  }

  private static string ToRPS(int pick) {
    return pick switch {
      0 => "Rock",
      1 => "Paper",
      2 => "Scissors",
      _ => "Unknown"
    };
  }

  #endregion

  #region Coinflip

  public IView CfFailedToChooseInTime(bool choice) {
    return new SimpleView {
      PREFIX,
      "You failed to choose in time, defaulting to",
      $"{ChatColors.Green} {(choice ? "Heads" : "Tails")}{ChatColors.Grey}."
    };
  }

  public IView CfGuardChose(CCSPlayerController guard, bool choice) {
    return new SimpleView {
      PREFIX,
      guard,
      "chose",
      $" {ChatColors.Green}{(choice ? "Heads" : "Tails")}{ChatColors.Grey}, flipping..."
    };
  }

  public IView CfCoinLandsOn(bool heads) {
    return new SimpleView {
      PREFIX,
      "The coin landed on",
      $" {ChatColors.Green}{(heads ? "Heads" : "Tails")}{ChatColors.Grey}."
    };
  }

  #endregion

  #region Bullet For Bullet

  public IView B4bPlayerGoesFirst(CCSPlayerController player) {
    return new SimpleView {
      PREFIX, "Randomly selected", player, "to go first."
    };
  }

  public IView B4bWeaponSelected(CCSPlayerController player, string weapon) {
    return new SimpleView { PREFIX, player, "picked", weapon };
  }

  #endregion

  #region Gun Toss

  public IView GtPlayerThrewGunDistance(CCSPlayerController player,
    float dist) {
    return new SimpleView {
      { PREFIX, player, "threw their gun", dist, "units." }
    };
  }

  #endregion

  #region Race

  public IView RaceEndRaceInstruction
    => new SimpleView {
      {
        PREFIX,
        $"Type {ChatColors.Blue}!endrace{ChatColors.White} to set the end point!"
      },
      SimpleView.NEWLINE, {
        PREFIX,
        $"Type {ChatColors.Blue}!endrace{ChatColors.White} to set the end point!"
      },
      SimpleView.NEWLINE, {
        PREFIX,
        $"Type {ChatColors.Blue}!endrace{ChatColors.White} to set the end point!"
      }
    };

  public IView RaceStartingMessage(CCSPlayerController prisoner) {
    return new SimpleView {
      {
        PREFIX, prisoner,
        "is racing you. Pay attention to where they set the end point!"
      }
    };
  }

  public IView RaceNotInRaceLR() {
    return new SimpleView {
      {
        PREFIX,
        $"{ChatColors.Red}You must be in a race {ChatColors.Blue + "!lr" + ChatColors.Red} to use this."
      }
    };
  }

  public IView RaceNotInPendingState() {
    return new SimpleView {
      {
        PREFIX,
        ChatColors.Red + "You must be in the pending state to use this command."
      }
    };
  }

  #endregion
}
