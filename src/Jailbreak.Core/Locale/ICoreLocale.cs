using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Core.Locale;

/// <summary>
/// Consolidated locale interface for all Core plugin messages.
/// Replaces the 13 separate stub locale interfaces from Services/Stubs/.
/// </summary>
public interface ICoreLocale {
  #region Warden

  IView PickingShortly { get; }
  IView NoWardens { get; }
  IView NowFreeday { get; }
  IView WardenLeft { get; }
  IView WardenDied { get; }
  IView BecomeNextWarden { get; }
  IView JoinRaffle { get; }
  IView LeaveRaffle { get; }
  IView NotWarden { get; }
  IView FireCommandFailed { get; }
  IView CannotWardenDuringWarmup { get; }
  IView TogglingNotEnabled { get; }
  IView PassWarden(CCSPlayerController player);
  IView NewWarden(CCSPlayerController player);
  IView CurrentWarden(CCSPlayerController? player);
  IView FireCommandSuccess(CCSPlayerController player);
  IView FireWarden(CCSPlayerController player);
  IView FireWarden(CCSPlayerController player, CCSPlayerController admin);
  IView MarkerPlaced();
  IView MarkerRemoved(string marker);
  IView AutoWardenToggled(bool enabled);

  #endregion

  #region Warden Commands - Count

  IView NoMarkerSet { get; }
  IView PrisonersInMarker(int prisoners);
  IView CannotCountYet(int seconds);

  #endregion

  #region Warden Commands - Open

  IView CellsOpened { get; }
  IView OpeningFailed { get; }
  IView AlreadyOpened { get; }
  IView CellsOpenedBy(CCSPlayerController? player);
  IView CellsOpenedWithPrisoners(int prisoners);
  IView CellsOpenedSnitchPrisoners(int prisoners);
  IView CannotOpenYet(int seconds);

  #endregion

  #region Warden Commands - Chicken

  IView ChickenSpawned { get; }
  IView ChickenSpawnFailed { get; }
  IView TooManyChickens { get; }

  #endregion

  #region Warden Commands - Soccer

  IView SoccerSpawned { get; }
  IView SoccerSpawnFailed { get; }
  IView TooManySoccers { get; }

  #endregion

  #region Warden Commands - Roll

  IView Roll(int roll);

  #endregion

  #region Special Treatment

  IView STGranted { get; }
  IView STRevoked { get; }
  IView STGrantedTo(CCSPlayerController player);
  IView STRevokedFrom(CCSPlayerController player);

  #endregion

  #region Mute / Peace

  IView PeaceActive { get; }
  IView UnmutedGuards { get; }
  IView UnmutedPrisoners { get; }
  IView MuteReminder { get; }
  IView PeaceReminder { get; }
  IView DeadReminder { get; }
  IView AdminDeadReminder { get; }
  IView PeaceEnactedByAdmin(int seconds);
  IView WardenEnactedPeace(int seconds);
  IView GeneralPeaceEnacted(int seconds);

  #endregion

  #region Rebel

  IView NoLongerRebel { get; }

  #endregion

  #region C4

  IView JihadC4Pickup { get; }
  IView JihadC4Received { get; }
  IView JihadC4Usage1 { get; }

  #endregion

  #region Logs

  IView BeginJailbreakLogs { get; }
  IView EndJailbreakLogs { get; }
  IView CreateLog(params FormatObject[] objects);

  #endregion

  #region Warden Commands - Marker

  IView MarkerChangingNotEnabled { get; }
  IView MarkerTypeChanged(string type);
  IView MarkerColorChanged(string color);

  #endregion

  #region Generic Commands

  IView PlayerNotFound(string query);
  IView PlayerFoundMultiple(string query);
  IView CommandOnCooldown(DateTime cooldownEndsAt);
  IView InvalidParameter(string parameter, string expected);
  IView NoPermissionMessage(string permission);
  IView Error(string message);

  #endregion

  #region Last Guard

  IView LGStarted(CCSPlayerController lastGuard, int ctHealth, int tHealth);

  #endregion
}
