using Jailbreak.Updater.Models;

namespace Jailbreak.Updater.Services;

public interface IUpdateService {
  UpdateState State { get; }
  Task<bool> CheckForUpdate();
  void ApplyUpdate(string pluginsDirectory);
  void FinalizeIfApplying();
}
