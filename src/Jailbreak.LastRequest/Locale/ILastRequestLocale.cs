using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Formatting;
using Jailbreak.LastRequest.Enums;
using Jailbreak.LastRequest.Services;

namespace Jailbreak.LastRequest.Locale;

/// <summary>
/// Consolidated locale for the entire LastRequest system.
/// Merges ILRLocale, ILRRPSLocale, ILRCFLocale, ILRB4BLocale, ILRGunTossLocale, ILRRaceLocale.
/// </summary>
public interface ILastRequestLocale {
  #region General LR

  IView DamageBlockedInsideLastRequest { get; }
  IView DamageBlockedNotInSameLR { get; }
  IView LastRequestEnabled();
  IView LastRequestDisabled();
  IView LastRequestNotEnabled();
  IView InvalidLastRequest(string query);
  IView InformLastRequest(AbstractLastRequest lr);
  IView LastRequestDecided(AbstractLastRequest lr, LRResult result);
  IView CannotLR(string reason);
  IView CannotLR(CCSPlayerController player, string reason);
  IView LastRequestCountdown(int seconds);
  IView WinByDefault(CCSPlayerController player);
  IView WinByHealth(CCSPlayerController player);
  IView WinByReason(CCSPlayerController player, string reason);
  IView Win(CCSPlayerController player);
  IView LastRequestRebel(CCSPlayerController player, int tHealth);
  IView LastRequestRebelDisabled();
  IView CannotLastRequestRebelCt();

  #endregion

  #region Rock Paper Scissors

  IView RpsPlayerMadeChoice(CCSPlayerController player);
  IView RpsBothPlayersMadeChoice();
  IView RpsTie();

  IView RpsResults(CCSPlayerController guard, CCSPlayerController prisoner,
    int guardPick, int prisonerPick);

  #endregion

  #region Coinflip

  IView CfFailedToChooseInTime(bool choice);
  IView CfGuardChose(CCSPlayerController guard, bool choice);
  IView CfCoinLandsOn(bool heads);

  #endregion

  #region Bullet For Bullet

  IView B4bPlayerGoesFirst(CCSPlayerController player);
  IView B4bWeaponSelected(CCSPlayerController player, string weapon);

  #endregion

  #region Gun Toss

  IView GtPlayerThrewGunDistance(CCSPlayerController player, float dist);

  #endregion

  #region Race

  IView RaceEndRaceInstruction { get; }
  IView RaceStartingMessage(CCSPlayerController prisoner);
  IView RaceNotInRaceLR();
  IView RaceNotInPendingState();

  #endregion
}
