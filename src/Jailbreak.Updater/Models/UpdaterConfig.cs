using System.Text.Json.Serialization;

namespace Jailbreak.Updater.Models;

public class UpdaterConfig {
  [JsonPropertyName("updateChannel")]
  public string UpdateChannel { get; set; } = "stable";

  [JsonPropertyName("checkOnMapChange")]
  public bool CheckOnMapChange { get; set; } = true;

  [JsonPropertyName("autoApply")]
  public bool AutoApply { get; set; } = true;

  [JsonPropertyName("repository")]
  public string Repository { get; set; } = "edgegamers/Jailbreak";
}
