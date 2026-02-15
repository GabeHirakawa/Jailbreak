using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Fun.Enums;
using Jailbreak.Fun.Locale;
using Jailbreak.Fun.Services.SpecialDay;

namespace Jailbreak.Fun.Commands;

public class SpecialDayCommands {
  private readonly SpecialDayFactory factory;
  private readonly IFunLocale locale;
  private readonly SpecialDayManager manager;
  private SpecialDayMenuSelector menuSelector = null!;
  private BasePlugin plugin = null!;

  public SpecialDayCommands(SpecialDayFactory factory, IFunLocale locale,
    SpecialDayManager manager) {
    this.factory = factory;
    this.locale  = locale;
    this.manager = manager;
  }

  public void Start(BasePlugin basePlugin) {
    plugin       = basePlugin;
    menuSelector = new SpecialDayMenuSelector(factory, plugin);
  }

  public void OnSpecialDayCommand(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor != null && manager.IsSDRunning && info.ArgCount == 1) {
      if (manager.CurrentSD is ISpecialDayMessageProvider messaged)
        locale.SpecialDayRunning(messaged.Locale.Name).ToChat(executor);
      else
        locale
         .SpecialDayRunning(manager.CurrentSD?.Type.ToString() ?? "Unknown")
         .ToChat(executor);
      return;
    }

    if (info.ArgCount == 1) {
      if (executor == null) {
        Server.PrintToConsole("css_sd [SD]");
        return;
      }

      MenuManager.OpenCenterHtmlMenu(plugin, executor, menuSelector.GetMenu());
      return;
    }

    var type = SDTypeExtensions.FromString(info.GetArg(1));
    if (type == null) {
      if (executor != null)
        locale.InvalidSpecialDay(info.GetArg(1)).ToChat(executor);
      return;
    }

    var canStart = manager.CanStartSpecialDay(type.Value, executor);
    if (!canStart) return;
    manager.InitiateSpecialDay(type.Value);
  }
}
