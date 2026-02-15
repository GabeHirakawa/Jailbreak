using Jailbreak.Updater.Models;

namespace Jailbreak.Updater.Services;

public interface IUpdateService {
  UpdateState State { get; }
  Task<bool> CheckForUpdate();
  Task ApplyUpdate(string pluginsDirectory);
  void FinalizeIfApplying();
}
