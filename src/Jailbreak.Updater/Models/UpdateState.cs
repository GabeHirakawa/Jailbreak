using System.Text.Json.Serialization;

namespace Jailbreak.Updater.Models;

public class UpdateState {
  [JsonPropertyName("installedVersion")]
  public string InstalledVersion { get; set; } = "0.0.0";

  [JsonPropertyName("stagedVersion")]
  public string? StagedVersion { get; set; }

  [JsonPropertyName("lastCheck")]
  public DateTime? LastCheck { get; set; }

  [JsonPropertyName("status")]
  public string Status { get; set; } = "idle";
}
