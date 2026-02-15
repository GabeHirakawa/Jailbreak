using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Core.Services.Stubs;

// TODO: Replace with real locale implementations when locale is migrated (Task 10)

public class StubWardenLocale : IWardenLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView PickingShortly => Stub;
  public IView NoWardens => Stub;
  public IView NowFreeday => Stub;
  public IView WardenLeft => Stub;
  public IView WardenDied => Stub;
  public IView BecomeNextWarden => Stub;
  public IView JoinRaffle => Stub;
  public IView LeaveRaffle => Stub;
  public IView NotWarden => Stub;
  public IView FireCommandFailed => Stub;
  public IView CannotWardenDuringWarmup => Stub;
  public IView TogglingNotEnabled => Stub;
  public IView PassWarden(CCSPlayerController player) => Stub;
  public IView NewWarden(CCSPlayerController player) => Stub;
  public IView CurrentWarden(CCSPlayerController? player) => Stub;
  public IView FireCommandSuccess(CCSPlayerController player) => Stub;
  public IView FireWarden(CCSPlayerController player) => Stub;
  public IView FireWarden(CCSPlayerController player, CCSPlayerController admin) => Stub;
  public IView MarkerPlaced() => Stub;
  public IView MarkerRemoved(string marker) => Stub;
  public IView AutoWardenToggled(bool enabled) => Stub;
}

public class StubWardenCmdCountLocale : IWardenCmdCountLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView NoMarkerSet => Stub;
  public IView PrisonersInMarker(int prisoners) => Stub;
  public IView CannotCountYet(int seconds) => Stub;
}

public class StubWardenCmdOpenLocale : IWardenCmdOpenLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView CellsOpened => Stub;
  public IView OpeningFailed => Stub;
  public IView AlreadyOpened => Stub;
  public IView CellsOpenedBy(CCSPlayerController? player) => Stub;
  public IView CellsOpenedWithPrisoners(int prisoners) => Stub;
  public IView CellsOpenedSnitchPrisoners(int prisoners) => Stub;
  public IView CannotOpenYet(int seconds) => Stub;
}

public class StubWardenCmdChickenLocale : IWardenCmdChickenLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView ChickenSpawned => Stub;
  public IView SpawnFailed => Stub;
  public IView TooManyChickens => Stub;
}

public class StubWardenCmdSoccerLocale : IWardenCmdSoccerLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView SoccerSpawned => Stub;
  public IView SpawnFailed => Stub;
  public IView TooManySoccers => Stub;
}

public class StubWardenCmdRollLocale : IWardenCmdRollLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView Roll(int roll) => Stub;
}

public class StubWardenSTLocale : IWardenSTLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView Granted => Stub;
  public IView Revoked => Stub;
  public IView GrantedTo(CCSPlayerController player) => Stub;
  public IView RevokedFrom(CCSPlayerController player) => Stub;
}

public class StubWardenPeaceLocale : IWardenPeaceLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView PeaceActive => Stub;
  public IView UnmutedGuards => Stub;
  public IView UnmutedPrisoners => Stub;
  public IView MuteReminder => Stub;
  public IView PeaceReminder => Stub;
  public IView DeadReminder => Stub;
  public IView AdminDeadReminder => Stub;
  public IView PeaceEnactedByAdmin(int seconds) => Stub;
  public IView WardenEnactedPeace(int seconds) => Stub;
  public IView GeneralPeaceEnacted(int seconds) => Stub;
}

public class StubRebelLocale : IRebelLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView NoLongerRebel => Stub;
}

public class StubC4Locale : IC4Locale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView JihadC4Pickup => Stub;
  public IView JihadC4Received => Stub;
  public IView JihadC4Usage1 => Stub;
}

public class StubLogLocale : ILogLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView BeginJailbreakLogs => Stub;
  public IView EndJailbreakLogs => Stub;
  public IView CreateLog(params FormatObject[] objects) => Stub;
}

public class StubWardenCmdMarkerLocale : IWardenCmdMarkerLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView ChangingNotEnabled => Stub;
  public IView TypeChanged(string type) => Stub;
  public IView ColorChanged(string color) => Stub;
}

public class StubGenericCmdLocale : IGenericCmdLocale {
  private static readonly IView Stub = new SimpleView { "..." };
  public IView PlayerNotFound(string query) => Stub;
  public IView PlayerFoundMultiple(string query) => Stub;
  public IView CommandOnCooldown(DateTime cooldownEndsAt) => Stub;
  public IView InvalidParameter(string parameter, string expected) => Stub;
  public IView NoPermissionMessage(string permission) => Stub;
  public IView Error(string message) => Stub;
}
