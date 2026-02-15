namespace Jailbreak.Core.Services.Stubs;

/// <summary>
/// Stub interface for special day manager. Will be replaced when SpecialDay is migrated (Task 12).
/// </summary>
public interface ISpecialDayManager {
  bool IsSDRunning { get; }
}
