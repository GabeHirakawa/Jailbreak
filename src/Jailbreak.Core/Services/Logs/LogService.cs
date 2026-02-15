using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Formatting.Objects;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Core.Services.Stubs;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Manages game event logs with rich formatting support.
/// Migrated from Jailbreak.Logs.LogsManager.
/// </summary>
public class LogService : IRichLogService {
  private readonly ILogLocale locale;
  private readonly IRichPlayerTag richPlayerTag;
  private readonly List<IView> logMessages = [];

  public LogService(ILogLocale locale, IRichPlayerTag richPlayerTag) {
    this.locale = locale;
    this.richPlayerTag = richPlayerTag;
  }

  public void Append(string message) {
    logMessages.Add(locale.CreateLog(message));
  }

  public IEnumerable<string> GetMessages() {
    return logMessages.SelectMany(view => view.ToWriter().Plain);
  }

  public void Clear() { logMessages.Clear(); }

  public void PrintLogs(CCSPlayerController? player) {
    if (player == null || !player.IsReal()) {
      locale.BeginJailbreakLogs.ToServerConsole();
      foreach (var log in logMessages) log.ToServerConsole();
      locale.EndJailbreakLogs.ToServerConsole();
      return;
    }

    locale.BeginJailbreakLogs.ToConsole(player);
    foreach (var log in logMessages) log.ToConsole(player);
    locale.EndJailbreakLogs.ToConsole(player);
  }

  public void Append(params FormatObject[] objects) {
    logMessages.Add(locale.CreateLog(objects));
  }

  public FormatObject Player(CCSPlayerController playerController) {
    return new TreeFormatObject {
      playerController,
      $"[{playerController.UserId}]",
      richPlayerTag.Rich(playerController)
    };
  }

  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    locale.BeginJailbreakLogs.ToServerConsole().ToAllConsole();

    //  By default, print all logs to player consoles at the end of the round.
    foreach (var log in logMessages) log.ToServerConsole().ToAllConsole();

    locale.EndJailbreakLogs.ToServerConsole().ToAllConsole();
    return HookResult.Continue;
  }

  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    Clear();
    return HookResult.Continue;
  }
}
