using CounterStrikeSharp.API.Core;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Service for managing game event logs.
/// Migrated from Jailbreak.Public.Mod.Logs.ILogService.
/// </summary>
public interface ILogService {
  void Append(string message);
  IEnumerable<string> GetMessages();
  void Clear();
  void PrintLogs(CCSPlayerController? player);
}
