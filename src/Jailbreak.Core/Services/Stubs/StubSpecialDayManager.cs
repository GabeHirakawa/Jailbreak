namespace Jailbreak.Core.Services.Stubs;

/// <summary>
/// No-op implementation of ISpecialDayManager. Will be replaced when SpecialDay is migrated (Task 12).
/// </summary>
public class StubSpecialDayManager : ISpecialDayManager {
  public bool IsSDRunning => false;
}
