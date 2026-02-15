using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Console command handler for viewing game logs.
/// Migrated from Jailbreak.Logs.LogsCommand.
/// </summary>
public class LogsCommand {
  private readonly ILogService logs;

  public LogsCommand(ILogService logs) {
    this.logs = logs;
  }

  [RequiresPermissionsOr("@css/ban", "@css/generic", "@css/kick")]
  public void Command_Logs(CCSPlayerController? executor, CommandInfo info) {
    logs.PrintLogs(executor);
  }
}
